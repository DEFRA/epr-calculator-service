using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.Detail;
using EPR.Calculator.Service.Function.Builder.RejectedProducers;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Logging;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;

namespace EPR.Calculator.Service.Function.Builder;

public interface IBillingBuilder
{
    Task<BillingResult> BuildAsync(RunContext runContext, CancellationToken cancellationToken);
}

public class BillingBuilder(
    ICalcResultDetailBuilder calcResultDetailBuilder,
    ICalcResultRejectedProducersBuilder rejectedProducersBuilder,
    ICalcResultReader calcResultReader,
    ITelemetryClient telemetryClient,
    ILogger<BillingBuilder> logger
)  : IBillingBuilder
{
    public Task<BillingResult> BuildAsync(RunContext runContext, CancellationToken cancellationToken) =>
        telemetryClient.TrackDuration(nameof(BillingBuilder), () => BuildResult(runContext, cancellationToken));

    private async Task<BillingResult> BuildResult(RunContext runContext, CancellationToken cancellationToken)
    {
        var details = await logger.LogDuration(
            () => calcResultDetailBuilder.ConstructAsync(runContext),
            nameof(calcResultDetailBuilder));

        var lapcap = await logger.LogDuration(
            () => calcResultReader.ReadLapcapData(runContext.RunId, cancellationToken),
            nameof(calcResultReader.ReadLapcapData));

        var lateReportingTonnage = await logger.LogDuration(
            () => calcResultReader.ReadLateReportingTonnage(runContext.RunId, cancellationToken),
            nameof(calcResultReader.ReadLateReportingTonnage));

        var otherCost = await logger.LogDuration(
            () => calcResultReader.ReadParameterOtherCost(runContext.RunId, cancellationToken),
            nameof(calcResultReader.ReadParameterOtherCost));

        var apportionment = await logger.LogDuration(
            () => calcResultReader.ReadOnePlusFourApportionment(runContext.RunId, cancellationToken),
            nameof(calcResultReader.ReadOnePlusFourApportionment));

        CalcResultProjectedProducers? projectedProducers = null;

        if (runContext.RequiresModulation)
        {
            await logger.LogDuration(async () =>
            {
                projectedProducers = new CalcResultProjectedProducers
                {
                    H1ProjectedProducers = await calcResultReader.ReadH1ProjectedData(runContext.RunId, cancellationToken),
                    H2ProjectedProducers = await calcResultReader.ReadH2ProjectedData(runContext.RunId, cancellationToken)
                };
            }, "ReadProjectedData");
        }

        CalcResultScaledupProducers? scaledUpProducers = null;

        if (runContext.RequiresScaling)
        {
            await logger.LogDuration(async () =>
            {
                scaledUpProducers = new CalcResultScaledupProducers
                {
                    ScaledupProducers = await calcResultReader.ReadScaledData(runContext.RunId, cancellationToken)
                };
            }, nameof(calcResultReader.ReadScaledData));
        }

        var partialObligations = await logger.LogDuration(
            () => calcResultReader.ReadPartialData(runContext.RunId, cancellationToken),
            nameof(calcResultReader.ReadPartialData));

        var rejectedProducers = await logger.LogDuration(
            () => rejectedProducersBuilder.ConstructAsync(runContext),
            nameof(rejectedProducersBuilder));

        var cancelledProducers = (
            await logger.LogDuration(
                async () =>
                {
                    var cancelledProducers = await calcResultReader.ReadCancelledProducers(runContext.RunId, cancellationToken);
                    var rejectedProducerIds = rejectedProducers.Select(r => r.ProducerId).ToHashSet();
                    return cancelledProducers.Where(p => !rejectedProducerIds.Contains(p.ProducerId)).ToImmutableList();
                },
                nameof(calcResultReader.ReadCancelledProducers))
        );

        var selfManagedConsumerWaste = await logger.LogDuration(
            () => calcResultReader.ReadSmcw(runContext.RunId, cancellationToken),
            nameof(calcResultReader.ReadSmcw));

        var disposalCost = await logger.LogDuration(
            () => calcResultReader.ReadLaDisposalCostData(runContext.RunId, cancellationToken),
            nameof(calcResultReader.ReadLaDisposalCostData));

        var commsCost = await logger.LogDuration(
            () => calcResultReader.ReadCommsCost(runContext.RunId, cancellationToken),
            nameof(calcResultReader.ReadCommsCost));

        ModulationResult? modulation = null;

        if (runContext.RequiresModulation)
        {
            modulation = await logger.LogDuration(
                () => calcResultReader.ReadModulationResult(runContext.RunId, cancellationToken),
                nameof(calcResultReader.ReadModulationResult));
        }

        var producerFees = await logger.LogDuration(
                () => calcResultReader.ReadProducerFees(runContext.RunId, cancellationToken),
                nameof(calcResultReader.ReadProducerFees));

        return new BillingResult
        {
            CalcResultDetail = details,
            CalcResultLapcapData = lapcap,
            CalcResultLaDisposalCostData = disposalCost,
            CalcResultCommsCostReportDetail = commsCost,
            CalcResultParameterOtherCost = otherCost,
            CalcResultLateReportingTonnageData = lateReportingTonnage,
            CalcResultOnePlusFourApportionment = apportionment,
            CalcResultPartialObligations = partialObligations,
            CalcResultProjectedProducers = projectedProducers,
            CalcResultScaledupProducers = scaledUpProducers,
            CalcResultCancelledProducers = cancelledProducers,
            CalcResultRejectedProducers = rejectedProducers,
            CalcResultModulation = modulation,
            Smcw = selfManagedConsumerWaste,
            ProducerFees = producerFees
        };
    }
}
