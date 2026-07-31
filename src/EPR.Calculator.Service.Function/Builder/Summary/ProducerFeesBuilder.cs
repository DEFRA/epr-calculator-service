using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.Summary.Common;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.Utils;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Builder.Summary;

public interface IProducerFeesBuilder
{
    Task<ProducerFees> ConstructAsync(
        RunContext runContext,
        FeesState state
    );
}

public record FeesState
{
    public required ImmutableList<MaterialDetail> Materials { get; init; }
    public required SelfManagedConsumerWaste Smcw { get; init; }
    public required CalcResultParameterOtherCost OtherCost { get; init; }
    public required CalcResultCommsCost CommsCost { get; init; }
    public required CalcResultOnePlusFourApportionment Apportionment { get; init; }
    public required CalcResultLaDisposalCostData DisposalCost { get; init; }
    public required ModulationResult? Modulation { get; init; }
    public required CalcResultLapcapData LapcapData { get; init; }
}

public class ProducerFeesBuilder(
    ApplicationDBContext context,
    IInvoicedProducerService invoicedProducerService)
    : IProducerFeesBuilder
{
    public async Task<ProducerFees> ConstructAsync(
        RunContext runContext,
        FeesState state
    )
    {
        var runProducerMaterialDetails = await (
            from pd in context.ProducerDetail
            join prm in context.ProducerMaterialPackaging on pd.Id equals prm.ProducerDetailId
            where pd.CalculatorRunId == runContext.RunId
            select new CalcResultProducerAndReportMaterialDetail
            {
                ProducerDetail = pd,
                ProducerMaterialPackaging = prm,
            }
        ).ToListAsync();

        var projectedMaterialsLookup = runProducerMaterialDetails
            .ToLookup(
                x => (x.ProducerDetail.ProducerId, x.ProducerDetail.SubsidiaryId),
                x => x.ProducerMaterialPackaging
            );

        var producerDetails = runProducerMaterialDetails
            .Select(x => x.ProducerDetail)
            .DistinctBy(x => (x.ProducerId, x.SubsidiaryId))
            .OrderBy(pd => pd.ProducerId)
            .ThenBy(pd => pd.SubsidiaryId)
            .ToImmutableList();

        var producerInvoicedMaterialNetTonnage = await invoicedProducerService.GetLatestAcceptedInvoicedProducers(runContext.RelativeYear);

        // PERF: Replace per-(producer, material) linear scans of the invoiced records collection with an O(1) lookup.
        var invoicedNetTonnageByProducerMaterial = BuildInvoicedNetTonnageByProducerMaterial(producerInvoicedMaterialNetTonnage);

        // Household + PublicBin + HDC.
        // PERF: wrap in an index so downstream callers (TonnageVsAllProducerUtil / 2B / 2C) get O(1)
        // per-producer percentage lookups instead of paying O(producers) per call.
        var totalPackagingTonnage = new TotalPackagingTonnageIndex(GetTotalPackagingTonnagePerRun(runProducerMaterialDetails, state, runContext.RunId));

        var organisations = await (
            from run in context.CalculatorRuns
            join crodm in context.CalculatorRunOrganisationDataMaster on run.CalculatorRunOrganisationDataMasterId equals crodm.Id
            join crodd in context.CalculatorRunOrganisationDataDetails on crodm.Id equals crodd.CalculatorRunOrganisationDataMasterId
            where run.Id == runContext.RunId && crodd.ObligationStatus == ObligationStates.Obligated
            select new Organisation
            {
                OrganisationId   = crodd.OrganisationId,
                SubsidiaryId     = crodd.SubsidiaryId,
                OrganisationName = crodd.OrganisationName,
                TradingName      = crodd.TradingName,
                StatusCode       = crodd.StatusCode,
                JoinerDate       = crodd.JoinerDate,
                LeaverDate       = crodd.LeaverDate
            })
            .Distinct()
            .ToImmutableListAsync();

        var parentOrganisations = organisations.Where(o => o.SubsidiaryId == null).ToImmutableList();

        // PERF: Replace per-row FirstOrDefault scans with O(1) dictionary lookups.
        var organisationsByKey = BuildOrganisationsByKey(organisations);
        var parentOrganisationsById = BuildParentOrganisationsById(parentOrganisations);

        var rowBuilder = new ProducerRowBuilder(
            invoicedNetTonnageByProducerMaterial,
            organisationsByKey,
            parentOrganisationsById
        );

        return GetProducerFees(
            runContext,
            projectedMaterialsLookup,
            producerDetails,
            state,
            totalPackagingTonnage,
            producerInvoicedMaterialNetTonnage,
            rowBuilder
        );
    }

    private static ImmutableDictionary<(int, int), decimal?> BuildInvoicedNetTonnageByProducerMaterial(
        IReadOnlyList<InvoicedProducer> invoicedProducers)
    {
        var builder = ImmutableDictionary.CreateBuilder<(int, int), decimal?>();
        foreach (var invoicedProducer in invoicedProducers)
        {
            // Preserve FirstOrDefault semantics (the previous LINQ kept only the first matching record).
            builder.TryAdd((invoicedProducer.ProducerId, invoicedProducer.MaterialId), invoicedProducer.InvoicedNetTonnage);
        }
        return builder.ToImmutable();
    }

    private static ImmutableDictionary<(int, string?), Organisation> BuildOrganisationsByKey(
        IReadOnlyList<Organisation> organisations)
    {
        var builder = ImmutableDictionary.CreateBuilder<(int, string?), Organisation>();
        foreach (var org in organisations)
        {
            builder.TryAdd((org.OrganisationId, org.SubsidiaryId), org);
        }
        return builder.ToImmutable();
    }

    private static ImmutableDictionary<int, Organisation> BuildParentOrganisationsById(
        IReadOnlyList<Organisation> parents)
    {
        var builder = ImmutableDictionary.CreateBuilder<int, Organisation>();
        foreach (var org in parents)
        {
            builder.TryAdd(org.OrganisationId, org);
        }
        return builder.ToImmutable();
    }

    [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
    private static ProducerFees GetProducerFees(
        RunContext runContext,
        ILookup<(int, string?), ProducerMaterialPackaging> projectedMaterialsLookup,
        IReadOnlyList<ProducerDetail> orderedProducerDetails,
        FeesState state,
        IReadOnlyList<TotalPackagingTonnagePerRun> totalPackagingTonnage,
        IReadOnlyList<InvoicedProducer> producerInvoicedMaterialNetTonnage,
        ProducerRowBuilder rowBuilder
    )
    {
        var result = new ProducerFees { CalculatorRunId = runContext.RunId, Total = new() { ProducerId = 0, SubsidiaryId = string.Empty, ProducerName = string.Empty } };

        if (orderedProducerDetails.Count == 0)
        {
            result.Total = ProducerRowBuilder.GetOverallTotalRow([], state.Materials);
            BillingInstructionsProducer.SetValues(result, producerInvoicedMaterialNetTonnage, state.OtherCost);
            return result;
        }

        var producerDisposalFees = new List<ProducerFeeDetail>();

        foreach (var producerAndSubsidiaries in orderedProducerDetails.GroupBy(x => x.ProducerId))
        {
            var subsidiariesList = producerAndSubsidiaries.ToList();
            bool hasGroupTotalRow = !(subsidiariesList.Count == 1 && subsidiariesList[0].SubsidiaryId == null);

            // Build L2 rows first so the L1 total can be derived by aggregation.
            var l2Rows = subsidiariesList
                .Select(producer => rowBuilder.GetProducerRow(runContext, projectedMaterialsLookup, hasGroupTotalRow, subsidiariesList, producer, state, totalPackagingTonnage))
                .ToList();

            if (hasGroupTotalRow)
                producerDisposalFees.Add(rowBuilder.GetL1TotalRow(subsidiariesList[0].ProducerId, l2Rows, state));

            producerDisposalFees.AddRange(l2Rows);
        }

        var l1Rows = producerDisposalFees.Where(r => r.FeeDetail.Level == CommonConstants.LevelOne.ToString()).ToList();
        result.Total = ProducerRowBuilder.GetOverallTotalRow(l1Rows, state.Materials);
        result.Details = producerDisposalFees;

        // Section 2b comms cost
        TwoBCommsCostProducer.SetValues(state, result);
        TwoCCommsCostProducer.SetValues(state, result);

        // Section Total bill (1 + 2a + 2b + 2c)
        OnePlus2A2B2CProducer.SetValues(result);

        // Section-3 SA Operating costs section
        ThreeSaCostsProducer.SetValues(state, result);

        // Section-4 LA data prep costs
        LaDataPrepCostsProducer.SetValues(state, result);

        // Section-5 SA setup costs
        SaSetupCostsProducer.SetValues(state, result);

        // Total bill section
        TotalBillBreakdownProducer.SetValues(result);

        // Billing instructions section
        BillingInstructionsProducer.SetValues(result, producerInvoicedMaterialNetTonnage, state.OtherCost);

        return result;
    }

    public static ImmutableList<TotalPackagingTonnagePerRun> GetTotalPackagingTonnagePerRun(
        IReadOnlyList<CalcResultProducerAndReportMaterialDetail> allResults,
        FeesState state,
        int runId
    )
    {
        var allProducerDetails = allResults.Select(x => x.ProducerDetail).DistinctBy(x => (x.ProducerId, x.SubsidiaryId));
        var allProducerReportedMaterials = allResults.Select(x => x.ProducerMaterialPackaging);

        var result =
            (from p in allProducerDetails
             join pm in allProducerReportedMaterials on p.Id equals pm.ProducerDetailId
             join m in state.Materials on pm.MaterialId equals m.Id
             where p.CalculatorRunId == runId &&
             (
                 pm.PackagingType == PackagingTypes.Household
                   || pm.PackagingType == PackagingTypes.PublicBin
                   || (pm.PackagingType == PackagingTypes.HouseholdDrinksContainers && m.Code == MaterialCodes.Glass)
             )
             group new { m = pm, p } by new { p.ProducerId, p.SubsidiaryId } into g
             select new TotalPackagingTonnagePerRun
             {
                 ProducerId            = g.Key.ProducerId,
                 SubsidiaryId          = g.Key.SubsidiaryId,
                 TotalPackagingTonnage = g.Sum(x => x.m.PackagingTonnage),
             }
            ).ToImmutableList();

        return result;
    }
}
