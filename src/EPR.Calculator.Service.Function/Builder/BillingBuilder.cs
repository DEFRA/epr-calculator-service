using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.Service.Function.Builder.CancelledProducers;
using EPR.Calculator.Service.Function.Builder.CommsCost;
using EPR.Calculator.Service.Function.Builder.Detail;
using EPR.Calculator.Service.Function.Builder.LaDisposalCost;
using EPR.Calculator.Service.Function.Builder.Lapcap;
using EPR.Calculator.Service.Function.Builder.LateReportingTonnages;
using EPR.Calculator.Service.Function.Builder.OnePlusFourApportionment;
using EPR.Calculator.Service.Function.Builder.ParametersOther;
using EPR.Calculator.Service.Function.Builder.RejectedProducers;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Logging;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;

namespace EPR.Calculator.Service.Function.Builder;

public interface IBillingBuilder
{
    Task<CalcResult> BuildAsync(RunContext runContext, CancellationToken cancellationToken);
}

[SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
public class BillingBuilder(
    ICalcResultLapcapDataBuilder lapcapDataBuilder,
    ICalcResultLateReportingBuilder lateReportingTonnageBuilder,
    ICalcResultParameterOtherCostBuilder otherCostsBuilder,
    ICalcResultOnePlusFourApportionmentBuilder onePlusFourApportionmentBuilder,
    ICalcResultCancelledProducersBuilder cancelledProducersBuilder,
    ICalcResultDetailBuilder calcResultDetailBuilder,
    ICalcResultCommsCostBuilder commsCostsBuilder,
    ICalcRunLaDisposalCostBuilder laDisposalCostsBuilder,
    ICalcResultRejectedProducersBuilder rejectedProducersBuilder,
    ICalcResultReader calcResultReader,
    IMaterialService materialService,
    ITelemetryClient telemetryClient,
    ILogger<BillingBuilder> logger
)  : IBillingBuilder
{
    public Task<CalcResult> BuildAsync(RunContext runContext, CancellationToken cancellationToken) =>
        telemetryClient.TrackDuration(nameof(BillingBuilder), () => BuildResult(runContext, cancellationToken));

    private async Task<CalcResult> BuildResult(RunContext runContext, CancellationToken cancellationToken)
    {
        var result = CalcResult.Empty;
        var materials = await materialService.GetMaterials();

        result.CalcResultDetail = await logger.LogDuration(
            () => calcResultDetailBuilder.ConstructAsync(runContext),
            nameof(calcResultDetailBuilder));

        result.CalcResultLapcapData = await logger.LogDuration(
            () => lapcapDataBuilder.ConstructAsync(runContext, materials),
            nameof(lapcapDataBuilder));

        result.CalcResultLateReportingTonnageData = await logger.LogDuration(
            () => lateReportingTonnageBuilder.ConstructAsync(runContext, materials),
            nameof(lateReportingTonnageBuilder));

        result.CalcResultParameterOtherCost = await logger.LogDuration(
            () => otherCostsBuilder.ConstructAsync(runContext),
            nameof(otherCostsBuilder));

        result.CalcResultOnePlusFourApportionment = logger.LogDuration(
            () => onePlusFourApportionmentBuilder.Construct(result),
            nameof(onePlusFourApportionmentBuilder));

        //TODO: Store/Read this?
        result.CalcResultCancelledProducers = await logger.LogDuration(
            () => cancelledProducersBuilder.ConstructAsync(runContext, materials),
            nameof(cancelledProducersBuilder));

        if (runContext.RequiresModulation)
        {
            result.CalcResultProjectedProducers.H1ProjectedProducers = (await logger.LogDuration(
                () => calcResultReader.ReadH1ProjectedData(runContext.RunId, cancellationToken),
                nameof(calcResultReader.ReadH1ProjectedData))).ToImmutableList();

            result.CalcResultProjectedProducers.H2ProjectedProducers = (await logger.LogDuration(
                () => calcResultReader.ReadH2ProjectedData(runContext.RunId, cancellationToken),
                nameof(calcResultReader.ReadH2ProjectedData))).ToImmutableList();
        }

        if (runContext.RequiresScaling)
        {
            result.CalcResultScaledupProducers.ScaledupProducers = (await logger.LogDuration(
                () => calcResultReader.ReadScaledData(runContext.RunId, cancellationToken),
                nameof(calcResultReader.ReadScaledData))).ToImmutableList();
        }

        result.CalcResultPartialObligations.PartialObligations = (await logger.LogDuration(
            () => calcResultReader.ReadPartialData(runContext.RunId, cancellationToken),
            nameof(calcResultReader.ReadPartialData))).ToImmutableList();

        result.CalcResultRejectedProducers = await logger.LogDuration(
            () => rejectedProducersBuilder.ConstructAsync(runContext),
            nameof(rejectedProducersBuilder));

        result.Smcw = await logger.LogDuration(
            () => calcResultReader.ReadSmcw(runContext.RunId, cancellationToken),
            nameof(calcResultReader.ReadSmcw));

        result.CalcResultLaDisposalCostData = await logger.LogDuration(
            () => laDisposalCostsBuilder.ConstructAsync(runContext, materials, result.CalcResultLapcapData, result.CalcResultLateReportingTonnageData, result.Smcw),
            nameof(laDisposalCostsBuilder));

        result.CalcResultCommsCostReportDetail = await logger.LogDuration(
            () => commsCostsBuilder.ConstructAsync(runContext, materials, result.CalcResultOnePlusFourApportionment, result.CalcResultLateReportingTonnageData),
            nameof(commsCostsBuilder));

        if (runContext.RequiresModulation)
        {
            result.CalcResultModulation = await logger.LogDuration(
                () => calcResultReader.ReadModulationResult(runContext.RunId, cancellationToken),
                nameof(calcResultReader.ReadModulationResult));
        }

        result.ProducerFees = await logger.LogDuration(
                () => calcResultReader.ReadProducerFees(runContext.RunId, cancellationToken),
                nameof(calcResultReader.ReadProducerFees));

        return result;
    }
}
