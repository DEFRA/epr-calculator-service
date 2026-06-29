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
using EPR.Calculator.API.Data.DataModels;

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

        var scaledupIds = calcResult.CalcResultScaledupProducers.ScaledupProducers.Select(p => p.ProducerId).ToList();
        var partialIds = calcResult.CalcResultPartialObligations.PartialObligations.Select(p => (p.ProducerId, p.SubsidiaryId)).ToList();
        summaryExporter.Export(runContext, acceptedCalcResultSummary, materials, scaledupIds, partialIds, csvContent);

        csvContent = ResetTotals(csvContent.ToString());

        rejectedProducersExporter.Export(calcResult.CalcResultRejectedProducers, csvContent);

        return csvContent.ToString();
    }

    private static CalcResultSummary GetAcceptedProducersCalcResults(CalcResultSummary calcResultSummary, ImmutableHashSet<int> acceptedProducerIds)
    {
        var billingOverallTotal = ZeroedTotalRow();
        billingOverallTotal.LADisposalCostsSection1                          = calcResultSummary.OverallTotal.LADisposalCostsSection1;
        billingOverallTotal.CommsCostsSection2a                              = calcResultSummary.OverallTotal.CommsCostsSection2a;
        billingOverallTotal.CommsCostsSection2b                              = calcResultSummary.OverallTotal.CommsCostsSection2b;
        billingOverallTotal.CommsCostsSection2c                              = calcResultSummary.OverallTotal.CommsCostsSection2c;
        billingOverallTotal.SaOperatingCostsSection3                         = calcResultSummary.OverallTotal.SaOperatingCostsSection3;
        billingOverallTotal.LaDataPrepSection4                               = calcResultSummary.OverallTotal.LaDataPrepSection4;
        billingOverallTotal.SaSetupCostsSection5                             = calcResultSummary.OverallTotal.SaSetupCostsSection5;
        billingOverallTotal.ProducerOverallPercentageOfCostsForOnePlus2A2B2C = calcResultSummary.OverallTotal.ProducerOverallPercentageOfCostsForOnePlus2A2B2C;
        return new CalcResultSummary
        {
            ProducerDisposalFees = GetAcceptedProducerDisposalFees(calcResultSummary.ProducerDisposalFees.ToList(), acceptedProducerIds),
            OverallTotal         = billingOverallTotal
        };
    }

    private static ImmutableList<CalcResultSummaryProducerDisposalFees> GetAcceptedProducerDisposalFees(
        IReadOnlyCollection<CalcResultSummaryProducerDisposalFees> producerDisposalFees,
        ImmutableHashSet<int> acceptedProducerIds)
    {
        return producerDisposalFees
            .Where(x => acceptedProducerIds.Contains(x.ProducerId))
            .ToImmutableList();
    }

    // TODO can we remove this row? // NOSONAR
    private static CalcResultSummaryProducerDisposalFees ZeroedTotalRow() => new()
    {
        //TODO
        CalculatorRunId            = 0,
        ProducerId                 = 0,
        SubsidiaryId               = string.Empty,
        ProducerName               = string.Empty,
        TradingName                = string.Empty,
        Level                      = string.Empty,
        StatusCode                 = string.Empty,
        JoinerDate                 = string.Empty,
        LeaverDate                 = CommonConstants.Totals,
        TonnageChangeCount         = string.Empty,
        TonnageChangeAdvice        = string.Empty,
        IsOverallTotal               = true,
        BillingInstructionSection  = new CalcResultSummaryBillingInstruction { SuggestedBillingInstruction = string.Empty }
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
        IReadOnlyCollection<CalcResultScaledupProducer> scaledUpProducers,
        ImmutableHashSet<int> acceptedProducerIds)
    {
        return scaledUpProducers
            .Where(x => acceptedProducerIds.Contains(x.ProducerId))
            .ToImmutableList();
    }

    private static CalcResultPartialObligations GetPartialObligationsForExport(
        CalcResultPartialObligations producers,
        ImmutableHashSet<int> acceptedProducerIds
    ) => new ()
        {
            PartialObligations = producers.PartialObligations.Where(x => acceptedProducerIds.Contains(x.ProducerId) || x.ProducerId == 0).ToImmutableList()
        };

    private static CalcResultProjectedProducers GetProjectedProducerForExport(
        CalcResultProjectedProducers producers,
        ImmutableHashSet<int> acceptedProducerIds)
    {
        bool isAccepted(int producerId) =>
            acceptedProducerIds.Contains(producerId) || producerId == 0;

        return new CalcResultProjectedProducers
        {
            H1ProjectedProducers = producers.H1ProjectedProducers.Where(x => isAccepted(x.ProducerId)).ToImmutableList(),
            H2ProjectedProducers = producers.H2ProjectedProducers.Where(x => isAccepted(x.ProducerId)).ToImmutableList()
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
