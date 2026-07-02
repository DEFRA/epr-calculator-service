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
using EPR.Calculator.Service.Function.Features.BillingRuns.Contexts;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Logging;
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
    ICalcResultRejectedProducersExporter rejectedProducersExporter,
    ILogger<BillingFileExporter> logger
)  : IBillingFileExporter
{
    public async Task<string> Export(BillingRunContext runContext, CalcResult calcResult)
    {
        var materials = await materialService.GetMaterials();
        var csvContent = new StringBuilder();

        logger.LogDuration(
            () => resultDetailExporter.Export(calcResult.CalcResultDetail, csvContent),
            nameof(resultDetailExporter)
        );

        logger.LogDuration(
            () => lapcapDataExporter.Export(calcResult.CalcResultLapcapData, materials, csvContent),
            nameof(lapcapDataExporter)
        );

        logger.LogDuration(
            () => lateReportingExporter.Export(calcResult.CalcResultLateReportingTonnageData, materials, csvContent),
            nameof(lateReportingExporter)
        );

        logger.LogDuration(
            () => parameterOtherCostExporter.Export(calcResult.CalcResultParameterOtherCost, csvContent),
            nameof(parameterOtherCostExporter)
        );

        logger.LogDuration(
            () => onePlusFourApportionmentExporter.Export(calcResult.CalcResultOnePlusFourApportionment, csvContent),
            nameof(onePlusFourApportionmentExporter)
        );

        logger.LogDuration(
            () => commsCostExporter.Export(calcResult.CalcResultCommsCostReportDetail, materials, csvContent),
            nameof(commsCostExporter)
        );

        logger.LogDuration(
            () => laDisposalCostExporter.Export(runContext, calcResult.CalcResultLaDisposalCostData, materials, csvContent),
            nameof(laDisposalCostExporter)
        );

        if (calcResult.Smcw is not null && calcResult.CalcResultModulation is not null)
            logger.LogDuration(
                () => modulationExporter.Export(calcResult.CalcResultLaDisposalCostData, calcResult.Smcw, calcResult.CalcResultModulation, csvContent),
                nameof(modulationExporter)
            );

        logger.LogDuration(
            () => cancelledProducersExporter.Export(calcResult.CalcResultCancelledProducers, csvContent),
            nameof(cancelledProducersExporter)
        );

        if (runContext.RequiresModulation)
        {
            var accepted = GetProjectedProducerForExport(calcResult.CalcResultProjectedProducers, runContext.AcceptedProducerIds);
            logger.LogDuration(
                () => projectedProducersExporter.Export(accepted, materials, csvContent),
                nameof(projectedProducersExporter)
            );
        }
        else
        {
            var acceptedProducers = GetScaledUpProducersForExport(calcResult.CalcResultScaledupProducers, runContext.AcceptedProducerIds);
            logger.LogDuration(
                () => scaledUpProducersExporter.Export(acceptedProducers, materials, false, csvContent),
                nameof(scaledUpProducersExporter)
            );
        }

        logger.LogDuration(
            () => partialObligationsExporter.Export(runContext, GetPartialObligationsForExport(calcResult.CalcResultPartialObligations, runContext.AcceptedProducerIds), materials, csvContent),
            nameof(partialObligationsExporter)
        );

        var acceptedCalcResultSummary = GetAcceptedProducersCalcResults(calcResult.CalcResultSummary, runContext.AcceptedProducerIds);
        logger.LogDuration(
            () => summaryExporter.Export(runContext, acceptedCalcResultSummary, materials, csvContent),
            nameof(summaryExporter)
        );

        csvContent = ResetTotals(csvContent.ToString());

        logger.LogDuration(
            () => rejectedProducersExporter.Export(calcResult.CalcResultRejectedProducers, csvContent),
            nameof(rejectedProducersExporter)
        );

        return csvContent.ToString();
    }

    private static CalcResultSummary GetAcceptedProducersCalcResults(CalcResultSummary calcResultSummary, ImmutableHashSet<int> acceptedProducerIds)
    {
        return new CalcResultSummary
        {
            LADisposalCostsSection1                   = calcResultSummary.LADisposalCostsSection1,
            CommsCostsSection2a                       = calcResultSummary.CommsCostsSection2a,
            CommsCostsSection2b                       = calcResultSummary.CommsCostsSection2b,
            CommsCostsSection2c                       = calcResultSummary.CommsCostsSection2c,
            SaOperatingCostsSection3                  = calcResultSummary.SaOperatingCostsSection3,
            LaDataPrepSection4                        = calcResultSummary.LaDataPrepSection4,
            SaSetupCostsSection5                      = calcResultSummary.SaSetupCostsSection5,
            TotalOnePlus2A2B2CFeeWithBadDebtProvision = calcResultSummary.TotalOnePlus2A2B2CFeeWithBadDebtProvision,
            ProducerDisposalFees                      = GetAcceptedProducerDisposalFees(calcResultSummary.ProducerDisposalFees.ToList(), acceptedProducerIds),
            OverallTotal                              = ZeroedTotalRow
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
    private static readonly CalcResultSummaryProducerDisposalFees ZeroedTotalRow = new()
    {
        ProducerId                 = 0,
        SubsidiaryId               = string.Empty,
        ProducerName               = string.Empty,
        TradingName                = string.Empty,
        Level                      = string.Empty,
        StatusCode                 = string.Empty,
        IsProducerScaledup         = string.Empty,
        IsPartialObligation        = string.Empty,
        JoinerDate                 = string.Empty,
        LeaverDate                 = CommonConstants.Totals,
        TonnageChangeCount         = string.Empty,
        TonnageChangeAdvice        = string.Empty,
        IsTotalRow                 = true,
        IsOverallTotalRow          = true,
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
