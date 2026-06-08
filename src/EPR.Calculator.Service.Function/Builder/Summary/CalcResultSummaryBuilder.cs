using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.Service.Function.Builder.ParametersOther;
using EPR.Calculator.Service.Function.Builder.Summary.Common;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.Utils;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Builder.Summary;

public interface ICalcResultSummaryBuilder
{
    Task<CalcResultSummary> ConstructAsync(
        RunContext runContext,
        IImmutableList<MaterialDetail> materialDetails,
        CalcResult calcResult,
        SelfManagedConsumerWaste smcw
    );
}

[ExcludeFromCodeCoverage]
public class CalcResultSummaryBuilder(
    ApplicationDBContext context,
    IInvoicedProducerService invoicedProducerService)
    : ICalcResultSummaryBuilder
{
    public async Task<CalcResultSummary> ConstructAsync(
        RunContext runContext,
        IImmutableList<MaterialDetail> materialDetails,
        CalcResult calcResult,
        SelfManagedConsumerWaste smcw
    )
    {
        var scaledupProducers = calcResult.CalcResultScaledupProducers.ScaledupProducers ?? [];
        var partialObligations = calcResult.CalcResultPartialObligations.PartialObligations ?? [];

        var runProducerMaterialDetails = await (
            from pd in context.ProducerDetail
            join prm in context.ProducerReportedMaterialProjected on pd.Id equals prm.ProducerDetailId
            where pd.CalculatorRunId == runContext.RunId
            select new CalcResultProducerAndReportMaterialDetail
            {
                ProducerDetail = pd,
                ProducerReportedMaterialProjected = prm,
            }
        ).ToListAsync();

        var projectedMaterialsLookup = runProducerMaterialDetails
            .ToLookup(
                x => (x.ProducerDetail.ProducerId, x.ProducerDetail.SubsidiaryId),
                x => x.ProducerReportedMaterialProjected
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

        var defaultParams = await GetDefaultParamsAsync(runContext.RunId);

        // Household + PublicBin + HDC.
        // PERF: wrap in an index so downstream callers (TonnageVsAllProducerUtil / 2B / 2C) get O(1)
        // per-producer percentage lookups instead of paying O(producers) per call.
        var totalPackagingTonnage = new TotalPackagingTonnageIndex(GetTotalPackagingTonnagePerRun(runProducerMaterialDetails, materialDetails, runContext.RunId));

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
            scaledupProducers,
            partialObligations,
            organisationsByKey,
            parentOrganisationsById);

        return GetCalcResultSummary(
            runContext,
            projectedMaterialsLookup,
            producerDetails,
            materialDetails,
            calcResult,
            totalPackagingTonnage,
            producerInvoicedMaterialNetTonnage,
            defaultParams,
            smcw,
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

    private Task<List<DefaultParamResultsClass>> GetDefaultParamsAsync(int runId)
    {
        return (
            from run in context.CalculatorRuns.AsNoTracking()
            join defaultMaster in context.DefaultParameterSettings.AsNoTracking() on run.DefaultParameterSettingMasterId equals defaultMaster.Id
            join defaultDetail in context.DefaultParameterSettingDetail.AsNoTracking() on defaultMaster.Id equals defaultDetail.DefaultParameterSettingMasterId
            join defaultTemplate in context.DefaultParameterTemplateMasterList.AsNoTracking() on defaultDetail.ParameterUniqueReferenceId equals defaultTemplate.ParameterUniqueReferenceId
            where run.Id == runId
            select new DefaultParamResultsClass
            {
                ParameterValue = defaultDetail.ParameterValue,
                ParameterCategory = defaultTemplate.ParameterCategory,
                ParameterType = defaultTemplate.ParameterType,
                ParameterUniqueReference = defaultDetail.ParameterUniqueReferenceId
            }
        ).ToListAsync();
    }

    private static CalcResultSummary GetCalcResultSummary(
        RunContext runContext,
        ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
        IReadOnlyList<ProducerDetail> orderedProducerDetails,
        IReadOnlyList<MaterialDetail> materials,
        CalcResult calcResult,
        IReadOnlyList<TotalPackagingTonnagePerRun> totalPackagingTonnage,
        IReadOnlyList<InvoicedProducer> producerInvoicedMaterialNetTonnage,
        IReadOnlyList<DefaultParamResultsClass> defaultParams,
        SelfManagedConsumerWaste smcw,
        ProducerRowBuilder rowBuilder
    )
    {
        var result = new CalcResultSummary();

        if (orderedProducerDetails.Count > 0)
        {
            var producerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>();

            foreach (var producerAndSubsidiaries in orderedProducerDetails.GroupBy(x => x.ProducerId))
            {
                var subsidiariesList = producerAndSubsidiaries.ToList();
                bool hasGroupTotalRow = !(subsidiariesList.Count == 1 && subsidiariesList[0].SubsidiaryId == null);

                // Build L2 rows first so the L1 total can be derived by aggregation.
                var l2Rows = subsidiariesList
                    .Select(producer => rowBuilder.GetProducerRow(runContext, projectedMaterialsLookup, hasGroupTotalRow, subsidiariesList, producer, materials, calcResult, totalPackagingTonnage, smcw))
                    .ToList();

                if (hasGroupTotalRow)
                    producerDisposalFees.Add(rowBuilder.GetL1TotalRow(subsidiariesList[0].ProducerId, l2Rows, calcResult, smcw, materials));

                producerDisposalFees.AddRange(l2Rows);
            };

            // Overall total: aggregate all Level-1 rows (one per producer group).
            var l1Rows = producerDisposalFees.Where(r => r.Level == CommonConstants.LevelOne.ToString()).ToList();
            var allTotalRow = rowBuilder.GetOverallTotalRow(l1Rows, materials);
            producerDisposalFees.Add(allTotalRow);

            result.ProducerDisposalFees = producerDisposalFees;

            // Section-(1) & (2a)
            SectionOneAndTwoAProducer.SetValues(allTotalRow, result);


            // Section 2b comms cost
            TwoBCommsCostProducer.SetValues(calcResult, result);

            TwoCCommsCostProducer.SetValues(calcResult, result);

            // Section Total bill (1 + 2a + 2b + 2c)
            OnePlus2A2B2CProducer.SetValues(result);

            // Section-3 SA Operating costs section
            ThreeSaCostsProducer.SetValues(calcResult, result);

            // Section-4 LA data prep costs
            LaDataPrepCostsProducer.SetValues(calcResult, result);

            // Section-5 SA setup costs
            SaSetupCostsProducer.SetValues(calcResult, result);

            // Total bill section
            TotalBillBreakdownProducer.SetValues(result);

            // Billing instructions section
            BillingInstructionsProducer.SetValues(result, producerInvoicedMaterialNetTonnage, defaultParams);
        }

        return result;
    }

    // TODO shouldn't need to call ths repeatedly
    public static ImmutableList<TotalPackagingTonnagePerRun> GetTotalPackagingTonnagePerRun(
        IReadOnlyList<CalcResultProducerAndReportMaterialDetail> allResults,
        IReadOnlyList<MaterialDetail> materials,
        int runId
    )
    {
        var allProducerDetails = allResults.Select(x => x.ProducerDetail).DistinctBy(x => (x.ProducerId, x.SubsidiaryId));
        var allProducerReportedMaterials = allResults.Select(x => x.ProducerReportedMaterialProjected);

        var result =
            (from p in allProducerDetails
             join pm in allProducerReportedMaterials on p.Id equals pm.ProducerDetailId
             join m in materials on pm.MaterialId equals m.Id
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
