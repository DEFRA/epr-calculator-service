using System.Diagnostics.CodeAnalysis;
using System.Text;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.CancelledProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.CommsCost;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Detail;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.LaDisposalCost;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Lapcap;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Modulation;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.OtherCosts;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.PartialObligations;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ProjectedProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.RejectedProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ScaledupProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;
using EPR.Calculator.Service.Function.Features.BillingRun.Contexts;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter;

public interface IBillingFileExporter
{
    Task<string> Export(BillingRunContext runContext, CalcResult calcResult);
}

[SuppressMessage("Constructor has 8 parameters, which is greater than the 7 authorized.", "S107", Justification = "This is suppressed for now and will be refactored later")]
public class BillingFileExporter(
    IMaterialService materialService,
    ICalcResultLateReportingExporter lateReportingExporter,
    ICalcResultDetailExporter resultDetailExporter,
    ICalcResultOnePlusFourApportionmentExporter onePlusFourApportionmentExporter,
    ICalcResultLaDisposalCostExporter laDisposalCostExporter,
    ICalcResultModulationExporter modulationExporter,
    ICalcResultScaledupProducersExporter scaledUpProducersExporter,
    ICalcResultPartialObligationsExporter partialObligationsExporter,
    ICalcResultProjectedProducersExporter projectedProducersExporter,
    ICalcResultLapcapDataExporter lapcapDataExporter,
    ICalcResultParameterOtherCostExporter parameterOtherCostExporter,
    ICalcResultCommsCostExporter commsCostExporter,
    ICalcResultSummaryExporter summaryExporter,
    ICalcResultCancelledProducersExporter cancelledProducersExporter,
    ICalcResultRejectedProducersExporter rejectedProducersExporter
)  : IBillingFileExporter
{
    public async Task<string> Export(BillingRunContext runContext, CalcResult calcResult)
    {
        var materials = await materialService.GetMaterials();
        var csvContent = new StringBuilder();
        resultDetailExporter.Export(calcResult.CalcResultDetail, csvContent);

        lapcapDataExporter.Export(calcResult.CalcResultLapcapData, materials, csvContent);

        lateReportingExporter.Export(calcResult.CalcResultLateReportingTonnageData, materials, csvContent);

        parameterOtherCostExporter.Export(calcResult.CalcResultParameterOtherCost, csvContent);

        onePlusFourApportionmentExporter.Export(calcResult.CalcResultOnePlusFourApportionment, csvContent);

        commsCostExporter.Export(calcResult.CalcResultCommsCostReportDetail, materials, csvContent);

        laDisposalCostExporter.Export(runContext, calcResult.CalcResultLaDisposalCostData, materials, csvContent);

        if (calcResult.Smcw is not null && calcResult.CalcResultModulation is not null)
            modulationExporter.Export(calcResult.CalcResultLaDisposalCostData, calcResult.Smcw, calcResult.CalcResultModulation, csvContent);

        cancelledProducersExporter.Export(calcResult.CalcResultCancelledProducers, csvContent);

        if (runContext.RequiresModulation)
        {
            var accepted = GetProjectedProducerForExport(calcResult.CalcResultProjectedProducers, runContext.AcceptedProducerIds);
            projectedProducersExporter.Export(accepted, materials, csvContent);
        }
        else
        {
            var acceptedProducers = GetScaledUpProducersForExport(calcResult.CalcResultScaledupProducers, runContext.AcceptedProducerIds);
            scaledUpProducersExporter.Export(acceptedProducers, materials, false, csvContent);
        }

        partialObligationsExporter.Export(runContext, GetPartialObligationsForExport(calcResult.CalcResultPartialObligations, runContext.AcceptedProducerIds), materials, csvContent);

        var acceptedCalcResultSummary = GetAcceptedProducersCalcResults(calcResult.CalcResultSummary, runContext.AcceptedProducerIds);

        summaryExporter.Export(runContext, acceptedCalcResultSummary, materials, csvContent);

        csvContent = ResetTotals(csvContent.ToString());

        rejectedProducersExporter.Export(calcResult.CalcResultRejectedProducers, csvContent);

        return csvContent.ToString();
    }

    private static CalcResultSummary GetAcceptedProducersCalcResults(CalcResultSummary calcResultSummary, ImmutableHashSet<int> acceptedProducerIds)
    {
        return new CalcResultSummary
        {
            LocalAuthorityDisposalCostsSectionOne     = calcResultSummary.LocalAuthorityDisposalCostsSectionOne,
            CommsCostsSectionTwoA                     = calcResultSummary.CommsCostsSectionTwoA,
            CommsCostsHeaderFor2bTitle                = calcResultSummary.CommsCostsHeaderFor2bTitle,
            LaDataPrepSection4                        = calcResultSummary.LaDataPrepSection4,
            ProducerDisposalFees                      = GetAcceptedProducerDisposalFees(calcResultSummary.ProducerDisposalFees.ToList(), acceptedProducerIds),
            SchemeAdministratorOperatingCosts         = calcResultSummary.SchemeAdministratorOperatingCosts,
            SaSetupCostsSection5                      = calcResultSummary.SaSetupCostsSection5,
            TotalOnePlus2A2B2CFeeWithBadDebtProvision = calcResultSummary.TotalOnePlus2A2B2CFeeWithBadDebtProvision,
            TwoCCommsCosts                            = calcResultSummary.TwoCCommsCosts
        };
    }

    private static ImmutableList<CalcResultSummaryProducerDisposalFees> GetAcceptedProducerDisposalFees(
        IReadOnlyCollection<CalcResultSummaryProducerDisposalFees> producerDisposalFees,
        ImmutableHashSet<int> acceptedProducerIds)
    {
        return producerDisposalFees
            .Where(x => acceptedProducerIds.Contains(x.ProducerIdInt) || x.ProducerIdInt == 0)
            .Select(x => x.isOverallTotalRow ? ZeroedTotalRow : x)
            .ToImmutableList();
    }

    // TODO can we remove this row?
    // TODO look into why we need a new one each time - i.e. it's being mutated
    // TODO we should just write this directly into the CSV if it's needed
    private static CalcResultSummaryProducerDisposalFees ZeroedTotalRow => new()
    {
        ProducerId                            = string.Empty,
        SubsidiaryId                          = string.Empty,
        ProducerName                          = string.Empty,
        TradingName                           = string.Empty,
        Level                                 = string.Empty,
        StatusCode                            = string.Empty,
        IsProducerScaledup                    = string.Empty,
        IsPartialObligation                   = string.Empty,
        JoinerDate                            = string.Empty,
        LeaverDate                            = CommonConstants.Totals,
        TonnageChangeCount                    = string.Empty,
        TonnageChangeAdvice                   = string.Empty,
        isTotalRow                            = true,
        isOverallTotalRow                     = true,
        LocalAuthorityDisposalCostsSectionOne = CalcResultSummaryBadDebtProvision.Empty,
        CommsCostsSectionTwoA                 = CalcResultSummaryBadDebtProvision.Empty,
        CommsCostsSectionTwoC                 = CalcResultSummaryBadDebtProvision.Empty,
        BillingInstructionSection             = new CalcResultSummaryBillingInstruction { SuggestedBillingInstruction = string.Empty }
    };

    private static CalcResultScaledupProducers GetScaledUpProducersForExport(
        CalcResultScaledupProducers producers,
        ImmutableHashSet<int> acceptedProducerIds)
    {
        return new CalcResultScaledupProducers
        {
            ScaledupProducers = GetScaledupProducers(producers.ScaledupProducers, acceptedProducerIds)
        };
    }

    private static ImmutableList<CalcResultScaledupProducer> GetScaledupProducers(
        IReadOnlyCollection<CalcResultScaledupProducer>? scaledUpProducers,
        ImmutableHashSet<int> acceptedProducerIds)
    {
        return scaledUpProducers?
            .Where(x => acceptedProducerIds.Contains(x.ProducerId))
            .ToImmutableList() ?? [];
    }

    private static CalcResultPartialObligations GetPartialObligationsForExport(
        CalcResultPartialObligations producers,
        ImmutableHashSet<int> acceptedProducerIds)
    {
        var acceptedProducers = producers.PartialObligations?
            .Where(x => acceptedProducerIds.Contains(x.ProducerId) || x.ProducerId == 0)
            .ToImmutableList() ?? [];

        return new CalcResultPartialObligations
        {
            PartialObligations = acceptedProducers
        };
    }

    private static CalcResultProjectedProducers GetProjectedProducerForExport(
        CalcResultProjectedProducers producers,
        ImmutableHashSet<int> acceptedProducerIds)
    {
        var isAccepted = (int producerId) => acceptedProducerIds.Contains(producerId) || producerId == 0;
        var acceptedH2Producers = producers.H2ProjectedProducers?.Where(x => isAccepted(x.ProducerId)).ToImmutableList() ?? [];
        var acceptedH1Producers = producers.H1ProjectedProducers?.Where(x => isAccepted(x.ProducerId)).ToImmutableList() ?? [];

        return new CalcResultProjectedProducers
        {
            H2ProjectedProducers = acceptedH2Producers,
            H1ProjectedProducers = acceptedH1Producers
        };
    }

    private static StringBuilder ResetTotals(string sb)
    {
        var idx = sb.LastIndexOf(CommonConstants.Totals, StringComparison.Ordinal);
        if (idx < 0)
            return new StringBuilder(sb);
        var exceptTotals = sb.Substring(0, idx + CommonConstants.Totals.Length + 2);
        return new StringBuilder().Append(exceptTotals);
    }
}
