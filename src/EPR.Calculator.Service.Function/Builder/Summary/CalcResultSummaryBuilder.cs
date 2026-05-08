using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.Service.Function.Builder.ParametersOther;
using EPR.Calculator.Service.Function.Builder.Summary.BillingInstructions;
using EPR.Calculator.Service.Function.Builder.Summary.Common;
using EPR.Calculator.Service.Function.Builder.Summary.CommsCostTwoA;
using EPR.Calculator.Service.Function.Builder.Summary.CommsCostTwoBTotalBill;
using EPR.Calculator.Service.Function.Builder.Summary.LaDataPrepCosts;
using EPR.Calculator.Service.Function.Builder.Summary.OneAndTwoA;
using EPR.Calculator.Service.Function.Builder.Summary.OnePlus2A2B2C;
using EPR.Calculator.Service.Function.Builder.Summary.SaSetupCosts;
using EPR.Calculator.Service.Function.Builder.Summary.ThreeSa;
using EPR.Calculator.Service.Function.Builder.Summary.TonnageVsAllProducer;
using EPR.Calculator.Service.Function.Builder.Summary.TotalBillBreakdown;
using EPR.Calculator.Service.Function.Builder.Summary.TwoCCommsCost;
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
        ImmutableList<MaterialDetail> materials,
        CalcResult calcResult,
        SelfManagedConsumerWaste smcw
    );
}

public class CalcResultSummaryBuilder(
    ApplicationDBContext dbContext,
    IInvoicedProducerService invoicedProducerService)
    : ICalcResultSummaryBuilder
{
    public IEnumerable<CalcResultScaledupProducer> ScaledupProducers { get; set; } = [];
    public IEnumerable<CalcResultPartialObligation> PartialObligations { get; set; } = [];
    public IEnumerable<Organisation> Organisations { get; set; } = [];
    public IEnumerable<Organisation> ParentOrganisations { get; set; } = [];

    public async Task<CalcResultSummary> ConstructAsync(
        RunContext runContext,
        ImmutableList<MaterialDetail> materials,
        CalcResult calcResult,
        SelfManagedConsumerWaste smcw
    )
    {
        ScaledupProducers = calcResult.CalcResultScaledupProducers.ScaledupProducers ?? [];
        PartialObligations = calcResult.CalcResultPartialObligations.PartialObligations ?? [];

        var runProducerMaterialDetails = await (
            from pd in dbContext.ProducerDetail
            join prm in dbContext.ProducerReportedMaterialProjected on pd.Id equals prm.ProducerDetailId
            where pd.CalculatorRunId == runContext.RunId
            select new CalcResultProducerAndReportMaterialDetail
            {
                ProducerDetail = pd,
                ProducerReportedMaterialProjected = prm
            }
        ).ToListAsync();

        var projectedMaterialsLookup = runProducerMaterialDetails
            .ToLookup(
                x => (x.ProducerDetail.ProducerId, x.ProducerDetail.SubsidiaryId),
                x => x.ProducerReportedMaterialProjected
            );

        var producerDetails = runProducerMaterialDetails.Select(x => x.ProducerDetail).Distinct().ToList();

        var orderedProducerDetails = GetOrderedListOfProducersAssociatedRunId(
            runContext.RunId, producerDetails);

        var producerInvoicedMaterialNetTonnage = await invoicedProducerService.GetLatestAcceptedInvoicedProducerRecords(runContext.RelativeYear);

        var defaultParams = await GetDefaultParamsAsync(runContext.RunId);

        // Household + PublicBin + HDC
        var totalPackagingTonnage = GetTotalPackagingTonnagePerRun(runProducerMaterialDetails, materials, runContext.RunId);

        // Get organisations
        Organisations = await (
            from run in dbContext.CalculatorRuns
            join crodm in dbContext.CalculatorRunOrganisationDataMaster on run.CalculatorRunOrganisationDataMasterId equals crodm.Id
            join crodd in dbContext.CalculatorRunOrganisationDataDetails on crodm.Id equals crodd.CalculatorRunOrganisationDataMasterId
            where run.Id == runContext.RunId && crodd.ObligationStatus == ObligationStates.Obligated
            select new Organisation
            {
                OrganisationId = crodd.OrganisationId,
                SubsidiaryId = crodd.SubsidiaryId,
                OrganisationName = crodd.OrganisationName,
                TradingName = crodd.TradingName,
                StatusCode = crodd.StatusCode,
                JoinerDate = crodd.JoinerDate,
                LeaverDate = crodd.LeaverDate
            }
        ).Distinct().ToListAsync();

        ParentOrganisations = Organisations.Where(o => o.SubsidiaryId == null);

        var result = GetCalcResultSummary(
            runContext,
            projectedMaterialsLookup,
            orderedProducerDetails,
            materials,
            calcResult,
            totalPackagingTonnage,
            producerInvoicedMaterialNetTonnage,
            defaultParams,
            smcw
        );

        return result;
    }

    private Task<List<DefaultParamResultsClass>> GetDefaultParamsAsync(int runId)
    {
        return (
            from run in dbContext.CalculatorRuns.AsNoTracking()
            join defaultMaster in dbContext.DefaultParameterSettings.AsNoTracking() on run.DefaultParameterSettingMasterId equals defaultMaster.Id
            join defaultDetail in dbContext.DefaultParameterSettingDetail.AsNoTracking() on defaultMaster.Id equals defaultDetail.DefaultParameterSettingMasterId
            join defaultTemplate in dbContext.DefaultParameterTemplateMasterList.AsNoTracking() on defaultDetail.ParameterUniqueReferenceId equals defaultTemplate.ParameterUniqueReferenceId
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

    [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
    public CalcResultSummary GetCalcResultSummary(
        RunContext runContext,
        ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
        IEnumerable<ProducerDetail> orderedProducerDetails,
        ImmutableList<MaterialDetail> materials,
        CalcResult calcResult,
        IEnumerable<TotalPackagingTonnagePerRun> totalPackagingTonnage,
        ImmutableList<InvoicedProducerRecord> producerInvoicedMaterialNetTonnage,
        IEnumerable<DefaultParamResultsClass> defaultParams,
        SelfManagedConsumerWaste smcw
    )
    {
        var result = new CalcResultSummary();
        if (orderedProducerDetails.Any())
        {
            var producerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>();

            foreach (var producerAndSubsidiaries in orderedProducerDetails.GroupBy(x => x.ProducerId))
            {
                var subsidiariesList = producerAndSubsidiaries.ToList();

                if (!(subsidiariesList.Count == 1 && subsidiariesList[0].SubsidiaryId == null))
                    producerDisposalFees.Add(GetProducerTotalRow(runContext, projectedMaterialsLookup, producerAndSubsidiaries.ToList(), materials, calcResult, producerDisposalFees, false, totalPackagingTonnage, producerInvoicedMaterialNetTonnage, smcw));

                foreach (var producer in subsidiariesList)
                    producerDisposalFees.Add(GetProducerRow(runContext, projectedMaterialsLookup, producerDisposalFees, producerAndSubsidiaries.ToList(), producer, materials, calcResult, totalPackagingTonnage, producerInvoicedMaterialNetTonnage, smcw));
            }

            ;

            // Calculate the total for all the producers
            var allTotalRow = GetProducerTotalRow(runContext, projectedMaterialsLookup, orderedProducerDetails.ToList(), materials, calcResult, producerDisposalFees, true, totalPackagingTonnage, producerInvoicedMaterialNetTonnage, smcw);
            producerDisposalFees.Add(allTotalRow);

            result.ProducerDisposalFees = producerDisposalFees;

            // Section-(1) & (2a)
            result.TotalFeeforLADisposalCostswoBadDebtprovision1 = CalcResultOneAndTwoAUtil.GetTotalDisposalCostswoBadDebtprovision1(producerDisposalFees);
            result.BadDebtProvisionFor1 = CalcResultOneAndTwoAUtil.GetTotalBadDebtprovision1(producerDisposalFees);
            result.TotalFeeforLADisposalCostswithBadDebtprovision1 = CalcResultOneAndTwoAUtil.GetTotalDisposalCostswithBadDebtprovision1(producerDisposalFees);

            result.TotalFeeforCommsCostsbyMaterialwoBadDebtProvision2A = CalcResultOneAndTwoAUtil.GetTotalCommsCostswoBadDebtprovision2A(producerDisposalFees);
            result.BadDebtProvisionFor2A = CalcResultOneAndTwoAUtil.GetTotalBadDebtprovision2A(producerDisposalFees);
            result.TotalFeeforCommsCostsbyMaterialwithBadDebtprovision2A = CalcResultOneAndTwoAUtil.GetTotalCommsCostswithBadDebtprovision2A(producerDisposalFees);

            // 2b comms total
            result.CommsCostHeaderWithoutBadDebtFor2bTitle = CalcResultSummaryUtil.GetCommsCostHeaderWithoutBadDebtFor2bTitle(calcResult);
            result.CommsCostHeaderBadDebtProvisionFor2bTitle = CalcResultSummaryUtil.GetCommsCostHeaderBadDebtProvisionFor2bTitle(calcResult, result);
            result.CommsCostHeaderWithBadDebtFor2bTitle = CalcResultSummaryUtil.GetCommsCostHeaderWithBadDebtFor2bTitle(result);

            TwoCCommsCostUtil.UpdateHeaderTotal(calcResult, result);

            // Section Total bill (1 + 2a + 2b + 2c)
            OnePlus2A2B2CProducer.SetValues(result);

            // Section-3 SA Operating costs section
            ThreeSaCostsProducer.GetProducerSetUpCostsSection3(calcResult, result);

            // Section-4 LA data prep costs
            LaDataPrepCostsProducer.SetValues(calcResult, result);

            // Section-5 SA setup costs
            SaSetupCostsProducer.GetProducerSetUpCosts(calcResult, result);

            // Total bill section
            TotalBillBreakdownProducer.SetValues(result);

            // Billing instructions section
            BillingInstructionsProducer.SetValues(result, producerInvoicedMaterialNetTonnage, defaultParams);
        }

        // Set headers with calculated column index
        CalcResultSummaryUtil.SetHeaders(result, materials, runContext.RequiresModulation);

        return result;
    }

    public static IEnumerable<ProducerDetail> GetOrderedListOfProducersAssociatedRunId(
        int runId,
        IEnumerable<ProducerDetail> producerDetails)
    {
        return producerDetails.Where(pd => pd.CalculatorRunId == runId).OrderBy(pd => pd.ProducerId).ThenBy(pd => pd.SubsidiaryId).ToList();
    }

    [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
    public CalcResultSummaryProducerDisposalFees GetProducerTotalRow(
        RunContext runContext,
        ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
        List<ProducerDetail> producersAndSubsidiaries,
        ImmutableList<MaterialDetail> materials,
        CalcResult calcResult,
        IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees,
        bool isOverAllTotalRow,
        IEnumerable<TotalPackagingTonnagePerRun> totalPackagingTonnage,
        IEnumerable<InvoicedProducerRecord> producerInvoicedMaterialNetTonnage,
        SelfManagedConsumerWaste smcw
    )
    {
        var materialCosts = GetMaterialCosts(runContext, projectedMaterialsLookup, producersAndSubsidiaries, producerDisposalFees, materials, calcResult, isOverAllTotalRow, producerInvoicedMaterialNetTonnage, smcw);
        var communicationCosts = GetCommunicationCosts(projectedMaterialsLookup, producersAndSubsidiaries, materials, calcResult);

        // Compute Count/Advice for the producer-total (Level 1) row
        string? tonnageChangeCount = null;
        string? tonnageChangeAdvice = null;
        var isProducerScaledUp = string.Empty;
        var isPartialObligation = string.Empty;

        if (!isOverAllTotalRow)
        {
            (tonnageChangeCount, tonnageChangeAdvice) =
                TonnageChangeUtil.ComputeCountAndAdvice(CommonConstants.LevelOne.ToString(), materialCosts);
            isProducerScaledUp = CalcResultSummaryUtil.IsProducerScaledup(producersAndSubsidiaries[0], ScaledupProducers) ? CommonConstants.Yes : CommonConstants.No;
            isPartialObligation = CalcResultSummaryUtil.IsProducerPartiallyObligated(producersAndSubsidiaries[0], PartialObligations, true) ? CommonConstants.Yes : CommonConstants.No;
        }

        var producerForTotalRow = GetProducerDetailsForTotalRow(producersAndSubsidiaries[0].ProducerId, isOverAllTotalRow);
        const int overallTotalId = 0;

        var totalRow = new CalcResultSummaryProducerDisposalFees
        {
            ProducerIdInt = isOverAllTotalRow ? overallTotalId : producersAndSubsidiaries[0].ProducerId,
            ProducerId = isOverAllTotalRow ? string.Empty : producersAndSubsidiaries[0].ProducerId.ToString(),
            ProducerName = producerForTotalRow?.OrganisationName ?? string.Empty,
            SubsidiaryId = string.Empty,
            TradingName = producerForTotalRow?.TradingName ?? string.Empty,
            Level = isOverAllTotalRow ? string.Empty : CommonConstants.LevelOne.ToString(),
            IsProducerScaledup = isProducerScaledUp,
            IsPartialObligation = isPartialObligation,
            StatusCode = producerForTotalRow?.StatusCode,
            JoinerDate = producerForTotalRow?.JoinerDate,
            LeaverDate = isOverAllTotalRow ? CommonConstants.Totals : producerForTotalRow?.LeaverDate,
            ProducerDisposalFeesByMaterial = materialCosts,

            // Disposal fee summary
            TotalProducerDisposalFee = materialCosts.Sum(m => m.Value.ProducerDisposalFee.total ?? 0),
            BadDebtProvision = materialCosts.Values.Sum(m => m.BadDebtProvision),
            TotalProducerDisposalFeeWithBadDebtProvision = materialCosts.Values.Sum(m => m.ProducerDisposalFeeWithBadDebtProvision),
            EnglandTotal = materialCosts.Values.Sum(m => m.EnglandWithBadDebtProvision),
            WalesTotal = materialCosts.Values.Sum(m => m.WalesWithBadDebtProvision),
            ScotlandTotal = materialCosts.Values.Sum(m => m.ScotlandWithBadDebtProvision),
            NorthernIrelandTotal = materialCosts.Values.Sum(m => m.NorthernIrelandWithBadDebtProvision),

            // For Comms Start
            TotalProducerCommsFee = communicationCosts.Values.Sum(m => m.ProducerTotalCostWithoutBadDebtProvision),
            BadDebtProvisionComms = communicationCosts.Values.Sum(m => m.BadDebtProvision),
            TotalProducerCommsFeeWithBadDebtProvision = communicationCosts.Values.Sum(m => m.ProducerTotalCostwithBadDebtProvision),
            EnglandTotalComms = communicationCosts.Values.Sum(m => m.EnglandWithBadDebtProvision),
            WalesTotalComms = communicationCosts.Values.Sum(m => m.WalesWithBadDebtProvision),
            ScotlandTotalComms = communicationCosts.Values.Sum(m => m.ScotlandWithBadDebtProvision),
            NorthernIrelandTotalComms = communicationCosts.Values.Sum(m => m.NorthernIrelandWithBadDebtProvision),
            ProducerCommsFeesByMaterial = communicationCosts,

            // Set Count/Advice on the producer-total row
            TonnageChangeCount = tonnageChangeCount,
            TonnageChangeAdvice = tonnageChangeAdvice,

            // Section 1
            LocalAuthorityDisposalCostsSectionOne = GetLocalAuthorityDisposalCostsSectionOne(materialCosts),

            // Section 2a
            CommunicationCostsSectionTwoA = GetCommunicationCostsSectionTwoA(communicationCosts),

            // Section 2b
            CommunicationCostsSectionTwoB = GetCommunicationCostsSectionTwoB(calcResult, producersAndSubsidiaries, totalPackagingTonnage),

            // Section-3
            // Percentage of Producer Reported Tonnage vs All Producers
            PercentageofProducerReportedTonnagevsAllProducers = TonnageVsAllProducerUtil.GetPercentageofProducerReportedTonnagevsAllProducersTotal(producersAndSubsidiaries, totalPackagingTonnage),

            isTotalRow = true,
            isOverallTotalRow = isOverAllTotalRow
        };

        TwoCCommsCostUtil.UpdateTwoCTotals(calcResult, producerDisposalFees, isOverAllTotalRow, totalRow, producersAndSubsidiaries, totalPackagingTonnage);

        return totalRow;
    }

    [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
    public CalcResultSummaryProducerDisposalFees GetProducerRow(
        RunContext runContext,
        ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
        List<CalcResultSummaryProducerDisposalFees> producerDisposalFeesLookup,
        List<ProducerDetail> producerAndSubsidiaries,
        ProducerDetail producer,
        ImmutableList<MaterialDetail> materials,
        CalcResult calcResult,
        IEnumerable<TotalPackagingTonnagePerRun> totalPackagingTonnage,
        IEnumerable<InvoicedProducerRecord> producerInvoicedMaterialNetTonnage,
        SelfManagedConsumerWaste smcw
    )
    {
        var materialCostSummary = new Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial>();
        var commsCostSummary = new Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial>();
        var level = CalcResultSummaryUtil.GetLevelIndex(producerDisposalFeesLookup, producer);

        var orgData = Organisations.FirstOrDefault(o => o.OrganisationId == producer.ProducerId && o.SubsidiaryId == producer.SubsidiaryId);

        var result = new CalcResultSummaryProducerDisposalFees
        {
            ProducerIdInt = producer.ProducerId,
            ProducerId = producer.ProducerId.ToString(),
            ProducerName = producer.ProducerName ?? string.Empty,
            SubsidiaryId = producer.SubsidiaryId ?? string.Empty,
            TradingName = producer.TradingName ?? string.Empty,
            Level = level.ToString(),
            IsProducerScaledup = CalcResultSummaryUtil.IsProducerScaledup(producer, ScaledupProducers)
                ? CommonConstants.Yes
                : CommonConstants.No,
            IsPartialObligation = CalcResultSummaryUtil.IsProducerPartiallyObligated(producer, PartialObligations, false)
                ? CommonConstants.Yes
                : CommonConstants.No,
            StatusCode = orgData?.StatusCode,
            JoinerDate = orgData?.JoinerDate,
            LeaverDate = orgData?.LeaverDate
        };

        foreach (var material in materials)
        {
            var producerDisposalFeesByMaterial = BuildProducerDisposalFeesByMaterial(
                runContext,
                projectedMaterialsLookup,
                producerAndSubsidiaries,
                producer,
                material,
                calcResult,
                producerInvoicedMaterialNetTonnage,
                smcw,
                level);

            materialCostSummary.Add(material.Code, producerDisposalFeesByMaterial);
            result.TotalProducerDisposalFee += producerDisposalFeesByMaterial.ProducerDisposalFee.total ?? 0;
            result.BadDebtProvision += producerDisposalFeesByMaterial.BadDebtProvision;
            result.TotalProducerDisposalFeeWithBadDebtProvision += producerDisposalFeesByMaterial.ProducerDisposalFeeWithBadDebtProvision;
            result.EnglandTotal += producerDisposalFeesByMaterial.EnglandWithBadDebtProvision;
            result.WalesTotal += producerDisposalFeesByMaterial.WalesWithBadDebtProvision;
            result.ScotlandTotal += producerDisposalFeesByMaterial.ScotlandWithBadDebtProvision;
            result.NorthernIrelandTotal += producerDisposalFeesByMaterial.NorthernIrelandWithBadDebtProvision;

            var producerCommsFeesCostByMaterial = BuildProducerCommsFeesCostByMaterial(
                projectedMaterialsLookup,
                producer,
                material,
                calcResult
            );

            commsCostSummary.Add(material.Code, producerCommsFeesCostByMaterial);
            result.TotalProducerCommsFee += producerCommsFeesCostByMaterial.ProducerTotalCostWithoutBadDebtProvision;
            result.BadDebtProvisionComms += producerCommsFeesCostByMaterial.BadDebtProvision;
            result.TotalProducerCommsFeeWithBadDebtProvision += producerCommsFeesCostByMaterial.ProducerTotalCostwithBadDebtProvision;
            result.EnglandTotalComms += producerCommsFeesCostByMaterial.EnglandWithBadDebtProvision;
            result.WalesTotalComms += producerCommsFeesCostByMaterial.WalesWithBadDebtProvision;
            result.ScotlandTotalComms += producerCommsFeesCostByMaterial.ScotlandWithBadDebtProvision;
            result.NorthernIrelandTotalComms += producerCommsFeesCostByMaterial.NorthernIrelandWithBadDebtProvision;
        }

        result.ProducerDisposalFeesByMaterial = materialCostSummary;
        result.ProducerCommsFeesByMaterial = commsCostSummary;

        // Section 1
        result.LocalAuthorityDisposalCostsSectionOne = new CalcResultSummaryBadDebtProvision
        {
            TotalProducerFeeWithoutBadDebtProvision = result.TotalProducerDisposalFee,
            BadDebtProvision = result.BadDebtProvision,
            TotalProducerFeeWithBadDebtProvision = result.TotalProducerDisposalFeeWithBadDebtProvision,
            EnglandTotalWithBadDebtProvision = result.EnglandTotal,
            WalesTotalWithBadDebtProvision = result.WalesTotal,
            ScotlandTotalWithBadDebtProvision = result.ScotlandTotal,
            NorthernIrelandTotalWithBadDebtProvision = result.NorthernIrelandTotal
        };

        // Section 2a
        result.CommunicationCostsSectionTwoA = new CalcResultSummaryBadDebtProvision
        {
            TotalProducerFeeWithoutBadDebtProvision = result.TotalProducerCommsFee,
            BadDebtProvision = result.BadDebtProvisionComms,
            TotalProducerFeeWithBadDebtProvision = result.TotalProducerCommsFeeWithBadDebtProvision,
            EnglandTotalWithBadDebtProvision = result.EnglandTotalComms,
            WalesTotalWithBadDebtProvision = result.WalesTotalComms,
            ScotlandTotalWithBadDebtProvision = result.ScotlandTotalComms,
            NorthernIrelandTotalWithBadDebtProvision = result.NorthernIrelandTotalComms
        };

        // Section 2b
        result.CommunicationCostsSectionTwoB = new CalcResultSummaryBadDebtProvision
        {
            TotalProducerFeeWithoutBadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithoutBadDebtFor2b(calcResult, producer, totalPackagingTonnage),
            BadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsBadDebtProvisionFor2b(calcResult, producer, totalPackagingTonnage),
            TotalProducerFeeWithBadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithBadDebtFor2b(calcResult, producer, totalPackagingTonnage),
            EnglandTotalWithBadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsEnglandWithBadDebt(calcResult, producer, totalPackagingTonnage),
            WalesTotalWithBadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsWalesWithBadDebt(calcResult, producer, totalPackagingTonnage),
            ScotlandTotalWithBadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsScotlandWithBadDebt(calcResult, producer, totalPackagingTonnage),
            NorthernIrelandTotalWithBadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsNorthernIrelandWithBadDebt(calcResult, producer, totalPackagingTonnage)
        };

        var (countStr, advice) = TonnageChangeUtil.ComputeCountAndAdvice(result.Level, materialCostSummary);
        result.TonnageChangeCount = countStr;
        result.TonnageChangeAdvice = advice;

        // Section-3
        // Percentage of Producer Reported Tonnage vs All Producers
        result.PercentageofProducerReportedTonnagevsAllProducers = TonnageVsAllProducerUtil.GetPercentageofProducerReportedTonnagevsAllProducers(producer, totalPackagingTonnage);

        TwoCCommsCostUtil.UpdateTwoCRows(calcResult, result, producer, totalPackagingTonnage);

        return result;
    }

    private static RagRating GroupedRagRating(RagRating rating)
    {
        return rating switch
        {
            RagRating.Red or RagRating.RedMedical => RagRating.Red,
            RagRating.Amber or RagRating.AmberMedical => RagRating.Amber,
            RagRating.Green or RagRating.GreenMedical => RagRating.Green,
            _ => throw new ArgumentOutOfRangeException(nameof(rating))
        };
    }

    [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
    private CalcResultSummaryProducerDisposalFeesByMaterial BuildProducerDisposalFeesByMaterial(
        RunContext runContext,
        ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
        List<ProducerDetail> producerAndSubsidiaries,
        ProducerDetail producer,
        MaterialDetail material,
        CalcResult calcResult,
        IEnumerable<InvoicedProducerRecord> producerInvoicedMaterialNetTonnage,
        SelfManagedConsumerWaste smcw,
        int level)
    {
        var previousInvoicedNetTonnage = producerInvoicedMaterialNetTonnage
            .Where(x => x.MaterialId == material.Id && x.ProducerId == producer.ProducerId)
            .Select(x => x.InvoicedNetTonnage)
            .FirstOrDefault();

        var totalReportedTonnage = CalcResultSummaryUtil.GetReportedTonnage(projectedMaterialsLookup, producer, material);

        var selfManagedConsumerWasteData = smcw
            .ProducerTotals
            .Find(x => x.ProducerId == producer.ProducerId && x.SubsidiaryId == producer.SubsidiaryId && x.Level == level)?
            .SelfManagedConsumerWasteDataPerMaterials[material.Code] ?? SelfManagedConsumerWasteData.Zero;

        var l1TotalReportedTonnage = producerAndSubsidiaries.Sum(producer => CalcResultSummaryUtil.GetReportedTonnage(projectedMaterialsLookup, producer, material));
        var l1SelfManagedConsumerWasteData = CalcResultSummaryUtil.SumSelfManagedConsumerWasteData(producerAndSubsidiaries, material, isOverAllTotalRow: false, smcw);

        var producerDisposalFee =
            l1SelfManagedConsumerWasteData.SelfManagedConsumerWasteTonnage > l1TotalReportedTonnage
                ? (total: 0, red: 0, amber: 0, green: 0)
                : CalcResultSummaryUtil.GetProducerDisposalFee(material, calcResult, selfManagedConsumerWasteData);

        return new CalcResultSummaryProducerDisposalFeesByMaterial
        {
            HouseholdPackagingWasteTonnage = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.Household),
            HouseholdPackagingWasteTonnageRagRating = runContext.RequiresModulation
                ? Enum.GetValues<RagRating>().ToDictionary(
                    rag => rag,
                    rag => CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.Household, rag))
                : new Dictionary<RagRating, decimal>(),

            PublicBinTonnage = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.PublicBin),
            PublicBinTonnageRagRating = runContext.RequiresModulation
                ? Enum.GetValues<RagRating>().ToDictionary(
                    rag => rag,
                    rag => CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.PublicBin, rag))
                : new Dictionary<RagRating, decimal>(),

            HouseholdDrinksContainersTonnage = material.Code == MaterialCodes.Glass
                ? CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.HouseholdDrinksContainers)
                : new decimal(),
            HouseholdDrinksContainersTonnageRagRating = runContext.RequiresModulation && material.Code == MaterialCodes.Glass
                ? Enum.GetValues<RagRating>().ToDictionary(
                    rag => rag,
                    rag => CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.HouseholdDrinksContainers, rag))
                : new Dictionary<RagRating, decimal>(),

            TotalReportedTonnage = totalReportedTonnage,
            TotalReportedTonnageRagRating = runContext.RequiresModulation
                ? Enum.GetValues<RagRating>().GroupBy(GroupedRagRating).ToDictionary(
                    grp => grp.Key,
                    grp => grp.Sum(r => CalcResultSummaryUtil.GetReportedTonnage(projectedMaterialsLookup, producer, material, r)))
                : new(),

            SelfManagedConsumerWasteTonnage = selfManagedConsumerWasteData.SelfManagedConsumerWasteTonnage,
            ActionedSelfManagedConsumerWasteTonnage = selfManagedConsumerWasteData.ActionedSelfManagedConsumerWasteTonnage,
            ResidualSelfManagedConsumerWasteTonnage = selfManagedConsumerWasteData.ResidualSelfManagedConsumerWasteTonnage,
            NetReportedTonnage = selfManagedConsumerWasteData.NetReportedTonnage,
            TonnageChange = TonnageChangeUtil.ComputePerMaterialChange(level.ToString(), selfManagedConsumerWasteData.NetReportedTonnage.total, previousInvoicedNetTonnage),
            PricePerTonne = CalcResultSummaryUtil.GetPricePerTonne(material, calcResult),
            ProducerDisposalFee = producerDisposalFee,
            BadDebtProvision = CalcResultSummaryUtil.GetBadDebtProvision(calcResult, producerDisposalFee.total),
            ProducerDisposalFeeWithBadDebtProvision = CalcResultSummaryUtil.GetProducerDisposalFeeWithBadDebtProvision(calcResult, producerDisposalFee.total),
            EnglandWithBadDebtProvision = CalcResultSummaryUtil.GetCountryBadDebtProvision(calcResult, Countries.England, producerDisposalFee.total),
            WalesWithBadDebtProvision = CalcResultSummaryUtil.GetCountryBadDebtProvision(calcResult, Countries.Wales, producerDisposalFee.total),
            ScotlandWithBadDebtProvision = CalcResultSummaryUtil.GetCountryBadDebtProvision(calcResult, Countries.Scotland, producerDisposalFee.total),
            NorthernIrelandWithBadDebtProvision = CalcResultSummaryUtil.GetCountryBadDebtProvision(calcResult, Countries.NorthernIreland, producerDisposalFee.total),
            PreviousInvoicedTonnage = previousInvoicedNetTonnage.HasValue ? previousInvoicedNetTonnage.Value : null
        };
    }

    private CalcResultSummaryProducerCommsFeesCostByMaterial BuildProducerCommsFeesCostByMaterial(
        ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
        ProducerDetail producer,
        MaterialDetail material,
        CalcResult calcResult
    )
    {
        return new CalcResultSummaryProducerCommsFeesCostByMaterial
        {
            HouseholdPackagingWasteTonnage = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.Household),
            PublicBinTonnage = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.PublicBin),
            HouseholdDrinksContainersTonnage = material.Code == MaterialCodes.Glass
                ? CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.HouseholdDrinksContainers)
                : new decimal(),
            TotalReportedTonnage = CalcResultSummaryCommsCostTwoA.GetTotalReportedTonnage(projectedMaterialsLookup, producer, material),
            PriceperTonne = CalcResultSummaryCommsCostTwoA.GetPriceperTonneForComms(material, calcResult),
            ProducerTotalCostWithoutBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostWithoutBadDebtProvision(projectedMaterialsLookup, producer, material, calcResult),
            BadDebtProvision = CalcResultSummaryCommsCostTwoA.GetBadDebtProvisionForCommsCost(projectedMaterialsLookup, producer, material, calcResult),
            ProducerTotalCostwithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostwithBadDebtProvision(projectedMaterialsLookup, producer, material, calcResult),
            EnglandWithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetEnglandWithBadDebtProvisionForComms(projectedMaterialsLookup, producer, material, calcResult),
            WalesWithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetWalesWithBadDebtProvisionForComms(projectedMaterialsLookup, producer, material, calcResult),
            ScotlandWithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetScotlandWithBadDebtProvisionForComms(projectedMaterialsLookup, producer, material, calcResult),
            NorthernIrelandWithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetNorthernIrelandWithBadDebtProvisionForComms(projectedMaterialsLookup, producer, material, calcResult)
        };
    }

    public static IEnumerable<TotalPackagingTonnagePerRun> GetTotalPackagingTonnagePerRun(
        IEnumerable<CalcResultProducerAndReportMaterialDetail> allResults,
        IEnumerable<MaterialDetail> materials,
        int runId
    )
    {
        var allProducerDetails = allResults.Select(x => x.ProducerDetail).Distinct();
        var allProducerReportedMaterials = allResults.Select(x => x.ProducerReportedMaterialProjected);

        var result =
            (from p in allProducerDetails
                join pm in allProducerReportedMaterials on p.Id equals pm.ProducerDetailId
                join m in materials on pm.MaterialId equals m.Id
                where p.CalculatorRunId == runId &&
                      // TODO again, need this filter?
                      (
                          pm.PackagingType == PackagingTypes.Household || pm.PackagingType == PackagingTypes.PublicBin
                                                                       || (pm.PackagingType == PackagingTypes.HouseholdDrinksContainers && m.Code == MaterialCodes.Glass)
                      )
                group new { m = pm, p } by new { p.ProducerId, p.SubsidiaryId }
                into g
                select new TotalPackagingTonnagePerRun
                {
                    ProducerId = g.Key.ProducerId,
                    SubsidiaryId = g.Key.SubsidiaryId,
                    TotalPackagingTonnage = g.Sum(x => x.m.PackagingTonnage)
                }
            ).ToList();

        return result;
    }

    public Organisation? GetProducerDetailsForTotalRow(int producerId, bool isOverAllTotalRow)
    {
        if (isOverAllTotalRow)
            return null;

        var parentProducer = ParentOrganisations.FirstOrDefault(po => po.OrganisationId == producerId);
        return parentProducer;
    }

    [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
    private Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial> GetMaterialCosts(
        RunContext runContext,
        ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
        IEnumerable<ProducerDetail> producersAndSubsidiaries,
        IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees,
        ImmutableList<MaterialDetail> materials,
        CalcResult calcResult,
        bool isOverAllTotalRow,
        IEnumerable<InvoicedProducerRecord> producerInvoicedMaterialNetTonnage,
        SelfManagedConsumerWaste smcw)
    {
        var materialCosts = new Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial>();

        foreach (var material in materials)
        {
            var householdPackagingWasteTonnage = CalcResultSummaryUtil.GetTonnageTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, PackagingTypes.Household);
            var publicBinTonnage = CalcResultSummaryUtil.GetTonnageTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, PackagingTypes.PublicBin);
            var previousInvoicedNetTonnage = producerInvoicedMaterialNetTonnage
                .Where(x => x.MaterialId == material.Id
                            && x.ProducerId == producersAndSubsidiaries.FirstOrDefault()?.ProducerId)
                .Select(x => x.InvoicedNetTonnage)
                .FirstOrDefault();

            var producers = dbContext.ProducerDetail.ToList();
            var selfManagedConsumerWasteData = CalcResultSummaryUtil.SumSelfManagedConsumerWasteData(producersAndSubsidiaries, material, isOverAllTotalRow, smcw);
            var producerDisposalFee = CalcResultSummaryUtil.GetProducerDisposalFee(material, calcResult, selfManagedConsumerWasteData);

            // Compute TonnageChange per the story:
            // - Overall totals row: sum of Level-1 values
            // - Producer totals row (Level 1): per-material logic using net - previous, with null/zero handling
            var tonnageChange = isOverAllTotalRow
                ? TonnageChangeUtil.GetOverallChangeTotal(producerDisposalFees, material.Code)
                : TonnageChangeUtil.ComputePerMaterialChange(
                    CommonConstants.LevelOne.ToString(), // producer “total” row is Level 1 for this column
                    selfManagedConsumerWasteData.NetReportedTonnage.total,
                    previousInvoicedNetTonnage);

            materialCosts.Add(material.Code, new CalcResultSummaryProducerDisposalFeesByMaterial
            {
                HouseholdPackagingWasteTonnage = householdPackagingWasteTonnage,
                HouseholdPackagingWasteTonnageRagRating = runContext.RequiresModulation
                    ? Enum.GetValues<RagRating>().ToDictionary(
                        rag => rag,
                        rag => CalcResultSummaryUtil.GetTonnageTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, PackagingTypes.Household, rag))
                    : new Dictionary<RagRating, decimal>(),

                PublicBinTonnage = publicBinTonnage,
                PublicBinTonnageRagRating = runContext.RequiresModulation
                    ? Enum.GetValues<RagRating>().ToDictionary(
                        rag => rag,
                        rag => CalcResultSummaryUtil.GetTonnageTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, PackagingTypes.PublicBin, rag))
                    : new Dictionary<RagRating, decimal>(),

                HouseholdDrinksContainersTonnage = material.Code == MaterialCodes.Glass
                    ? CalcResultSummaryUtil.GetTonnageTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, PackagingTypes.HouseholdDrinksContainers)
                    : 0,
                HouseholdDrinksContainersTonnageRagRating = runContext.RequiresModulation && material.Code == MaterialCodes.Glass
                    ? Enum.GetValues<RagRating>().ToDictionary(
                        rag => rag,
                        rag => CalcResultSummaryUtil.GetTonnageTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, PackagingTypes.HouseholdDrinksContainers, rag))
                    : new Dictionary<RagRating, decimal>(),

                TotalReportedTonnage = CalcResultSummaryUtil.GetReportedTonnageTotal(projectedMaterialsLookup, producersAndSubsidiaries, material),
                TotalReportedTonnageRagRating = runContext.RequiresModulation
                    ? Enum.GetValues<RagRating>().GroupBy(GroupedRagRating).ToDictionary(
                        grp => grp.Key,
                        grp => grp.Sum(r => CalcResultSummaryUtil.GetReportedTonnageTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, r)))
                    : new(),

                SelfManagedConsumerWasteTonnage = selfManagedConsumerWasteData.SelfManagedConsumerWasteTonnage,
                ActionedSelfManagedConsumerWasteTonnage = selfManagedConsumerWasteData.ActionedSelfManagedConsumerWasteTonnage,
                ResidualSelfManagedConsumerWasteTonnage = selfManagedConsumerWasteData.ResidualSelfManagedConsumerWasteTonnage,
                NetReportedTonnage = selfManagedConsumerWasteData.NetReportedTonnage,
                PricePerTonne = CalcResultSummaryUtil.GetPricePerTonne(material, calcResult),
                ProducerDisposalFee = producerDisposalFee,
                BadDebtProvision = CalcResultSummaryUtil.GetBadDebtProvision(calcResult, producerDisposalFee.total),
                ProducerDisposalFeeWithBadDebtProvision = CalcResultSummaryUtil.GetProducerDisposalFeeWithBadDebtProvision(calcResult, producerDisposalFee.total),
                EnglandWithBadDebtProvision = CalcResultSummaryUtil.GetCountryBadDebtProvision(calcResult, Countries.England, producerDisposalFee.total),
                WalesWithBadDebtProvision = CalcResultSummaryUtil.GetCountryBadDebtProvision(calcResult, Countries.Wales, producerDisposalFee.total),
                ScotlandWithBadDebtProvision = CalcResultSummaryUtil.GetCountryBadDebtProvision(calcResult, Countries.Scotland, producerDisposalFee.total),
                NorthernIrelandWithBadDebtProvision = CalcResultSummaryUtil.GetCountryBadDebtProvision(calcResult, Countries.NorthernIreland, producerDisposalFee.total),
                PreviousInvoicedTonnage = CalcResultSummaryUtil.GetPreviousInvoicedTonnage(producerDisposalFees, producersAndSubsidiaries, ScaledupProducers, PartialObligations, material, isOverAllTotalRow, previousInvoicedNetTonnage),
                TonnageChange = tonnageChange
            });
        }

        return materialCosts;
    }

    private Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial> GetCommunicationCosts(
        ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
        IEnumerable<ProducerDetail> producersAndSubsidiaries,
        IEnumerable<MaterialDetail> materials,
        CalcResult calcResult
    )
    {
        var communicationCosts = new Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial>();

        foreach (var material in materials)
        {
            var publicBinTonnage = CalcResultSummaryUtil.GetTonnageTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, PackagingTypes.PublicBin);

            communicationCosts.Add(material.Code, new CalcResultSummaryProducerCommsFeesCostByMaterial
            {
                HouseholdPackagingWasteTonnage = CalcResultSummaryUtil.GetTonnageTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, PackagingTypes.Household),
                PublicBinTonnage = publicBinTonnage,
                HouseholdDrinksContainersTonnage = material.Code == MaterialCodes.Glass
                    ? CalcResultSummaryUtil.GetTonnageTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, PackagingTypes.HouseholdDrinksContainers)
                    : 0,
                TotalReportedTonnage = CalcResultSummaryCommsCostTwoA.GetTotalReportedTonnageTotal(projectedMaterialsLookup, producersAndSubsidiaries, material),
                PriceperTonne = CalcResultSummaryCommsCostTwoA.GetPriceperTonneForComms(material, calcResult),
                ProducerTotalCostWithoutBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostWithoutBadDebtProvisionTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, calcResult),
                BadDebtProvision = CalcResultSummaryCommsCostTwoA.GetBadDebtProvisionForCommsCostTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, calcResult),
                ProducerTotalCostwithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostwithBadDebtProvisionTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, calcResult),
                EnglandWithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetEnglandWithBadDebtProvisionForCommsTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, calcResult),
                WalesWithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetWalesWithBadDebtProvisionForCommsTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, calcResult),
                ScotlandWithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetScotlandWithBadDebtProvisionForCommsTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, calcResult),
                NorthernIrelandWithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetNorthernIrelandWithBadDebtProvisionForCommsTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, calcResult)
            });
        }

        return communicationCosts;
    }

    private static CalcResultSummaryBadDebtProvision GetLocalAuthorityDisposalCostsSectionOne(
        Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary)
    {
        return new CalcResultSummaryBadDebtProvision
        {
            TotalProducerFeeWithoutBadDebtProvision = materialCostSummary.Sum(m => m.Value.ProducerDisposalFee.total ?? 0),
            BadDebtProvision = materialCostSummary.Values.Sum(m => m.BadDebtProvision),
            TotalProducerFeeWithBadDebtProvision = materialCostSummary.Values.Sum(m => m.ProducerDisposalFeeWithBadDebtProvision),
            EnglandTotalWithBadDebtProvision = materialCostSummary.Values.Sum(m => m.EnglandWithBadDebtProvision),
            WalesTotalWithBadDebtProvision = materialCostSummary.Values.Sum(m => m.WalesWithBadDebtProvision),
            ScotlandTotalWithBadDebtProvision = materialCostSummary.Values.Sum(m => m.ScotlandWithBadDebtProvision),
            NorthernIrelandTotalWithBadDebtProvision = materialCostSummary.Values.Sum(m => m.NorthernIrelandWithBadDebtProvision)
        };
    }

    private static CalcResultSummaryBadDebtProvision GetCommunicationCostsSectionTwoA(
        Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostSummary)
    {
        return new CalcResultSummaryBadDebtProvision
        {
            TotalProducerFeeWithoutBadDebtProvision = commsCostSummary.Values.Sum(m => m.ProducerTotalCostWithoutBadDebtProvision),
            BadDebtProvision = commsCostSummary.Values.Sum(m => m.BadDebtProvision),
            TotalProducerFeeWithBadDebtProvision = commsCostSummary.Values.Sum(m => m.ProducerTotalCostwithBadDebtProvision),
            EnglandTotalWithBadDebtProvision = commsCostSummary.Values.Sum(m => m.EnglandWithBadDebtProvision),
            WalesTotalWithBadDebtProvision = commsCostSummary.Values.Sum(m => m.WalesWithBadDebtProvision),
            ScotlandTotalWithBadDebtProvision = commsCostSummary.Values.Sum(m => m.ScotlandWithBadDebtProvision),
            NorthernIrelandTotalWithBadDebtProvision = commsCostSummary.Values.Sum(m => m.NorthernIrelandWithBadDebtProvision)
        };
    }

    private static CalcResultSummaryBadDebtProvision GetCommunicationCostsSectionTwoB(
        CalcResult calcResult, IEnumerable<ProducerDetail> producersAndSubsidiaries, IEnumerable<TotalPackagingTonnagePerRun> totalPackagingTonnage)
    {
        return new CalcResultSummaryBadDebtProvision
        {
            TotalProducerFeeWithoutBadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithoutBadDebtFor2bTotalsRow(calcResult, producersAndSubsidiaries, totalPackagingTonnage),
            BadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsBadDebtProvisionFor2bTotalsRow(calcResult, producersAndSubsidiaries, totalPackagingTonnage),
            TotalProducerFeeWithBadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithBadDebtFor2bTotalsRow(calcResult, producersAndSubsidiaries, totalPackagingTonnage),
            EnglandTotalWithBadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsEnglandWithBadDebtTotalsRow(calcResult, producersAndSubsidiaries, totalPackagingTonnage),
            WalesTotalWithBadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsWalesWithBadDebtTotalsRow(calcResult, producersAndSubsidiaries, totalPackagingTonnage),
            ScotlandTotalWithBadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsScotlandWithBadDebtTotalsRow(calcResult, producersAndSubsidiaries, totalPackagingTonnage),
            NorthernIrelandTotalWithBadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsNorthernIrelandWithBadDebtTotalsRow(calcResult, producersAndSubsidiaries, totalPackagingTonnage)
        };
    }
}
