using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.CancelledProducers;
using EPR.Calculator.Service.Function.Builder.CommsCost;
using EPR.Calculator.Service.Function.Builder.Detail;
using EPR.Calculator.Service.Function.Builder.ErrorReport;
using EPR.Calculator.Service.Function.Builder.LaDisposalCost;
using EPR.Calculator.Service.Function.Builder.Lapcap;
using EPR.Calculator.Service.Function.Builder.LateReportingTonnages;
using EPR.Calculator.Service.Function.Builder.Modulation;
using EPR.Calculator.Service.Function.Builder.OnePlusFourApportionment;
using EPR.Calculator.Service.Function.Builder.ParametersOther;
using EPR.Calculator.Service.Function.Builder.PartialObligations;
using EPR.Calculator.Service.Function.Builder.ProjectedProducers;
using EPR.Calculator.Service.Function.Builder.ScaledupProducers;
using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Logging;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;

namespace EPR.Calculator.Service.Function.Builder;

public interface IResultBuilder
{
    Task<CalcResult> BuildAsync(RunContext runContext, CancellationToken cancellationToken);
}

[SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
public class ResultBuilder(
    ICalcResultLapcapDataBuilder lapcapDataBuilder,
    ICalcResultLateReportingBuilder lateReportingTonnageBuilder,
    ICalcResultParameterOtherCostBuilder otherCostsBuilder,
    ICalcResultOnePlusFourApportionmentBuilder onePlusFourApportionmentBuilder,
    ICalcResultCancelledProducersBuilder cancelledProducersBuilder,
    IReportedProducerService reportedProducersService,
    ICalcResultProjectedProducersBuilder projectedProducersBuilder,
    ICalcResultScaledupProducersBuilder scaledUpProducersBuilder,
    ICalcResultDetailBuilder calcResultDetailBuilder,
    ICalcResultCommsCostBuilder commsCostsBuilder,
    ICalcRunLaDisposalCostBuilder laDisposalCostsBuilder,
    ICalcResultPartialObligationBuilder partialObligationsBuilder,
    IProducerFeesBuilder producerFeesBuilder,
    ICalcResultErrorReportBuilder errorReportBuilder,
    ISelfManagedConsumerWasteService selfManagedConsumerWasteService,
    ICalcResultModulationBuilder modulationBuilder,
    ICalcResultWriter calcResultWriter,
    IMaterialService materialService,
    ITelemetryClient telemetryClient,
    ILogger<ResultBuilder> logger
)  : IResultBuilder
{
    public Task<CalcResult> BuildAsync(RunContext runContext, CancellationToken cancellationToken) =>
        telemetryClient.TrackDuration(nameof(ResultBuilder), () => BuildResult(runContext, cancellationToken));

    private async Task<CalcResult> BuildResult(RunContext runContext, CancellationToken cancellationToken)
    {
        var materials = await materialService.GetMaterials();

        var details = await logger.LogDuration(
            () => calcResultDetailBuilder.ConstructAsync(runContext),
            nameof(calcResultDetailBuilder));

        var lapcap = await logger.LogDuration(
            () => lapcapDataBuilder.ConstructAsync(runContext, materials),
            nameof(lapcapDataBuilder));

        await logger.LogDuration(
            () => calcResultWriter.StoreLapcapData(runContext.RunId, lapcap, cancellationToken),
            nameof(calcResultWriter.StoreLapcapData));

        var lateReportingTonnage = await logger.LogDuration(
            () => lateReportingTonnageBuilder.ConstructAsync(runContext, materials),
            nameof(lateReportingTonnageBuilder));

        await logger.LogDuration(
            () => calcResultWriter.StoreLateReportingTonnage(runContext.RunId, lateReportingTonnage, cancellationToken),
            nameof(calcResultWriter.StoreLateReportingTonnage));

        var otherCost = await logger.LogDuration(
            () => otherCostsBuilder.ConstructAsync(runContext),
            nameof(otherCostsBuilder));

        await logger.LogDuration(
            () => calcResultWriter.StoreParameterOtherCost(runContext.RunId, otherCost, cancellationToken),
            nameof(calcResultWriter.StoreParameterOtherCost));

        var apportionment = logger.LogDuration(
            () => onePlusFourApportionmentBuilder.Construct(lapcap, otherCost),
            nameof(onePlusFourApportionmentBuilder));

        await logger.LogDuration(
            () => calcResultWriter.StoreOnePlusFourApportionment(runContext.RunId, apportionment, cancellationToken),
            nameof(calcResultWriter.StoreOnePlusFourApportionment));

        var cancelledProducers = await logger.LogDuration(
            () => cancelledProducersBuilder.ConstructAsync(runContext, materials),
            nameof(cancelledProducersBuilder));

        await logger.LogDuration(
            () => calcResultWriter.StoreCancelledProducers(runContext.RunId, cancelledProducers, cancellationToken),
            nameof(calcResultWriter.StoreCancelledProducers));

        var producers = await reportedProducersService.GetProducers(runContext);
        CalcResultProjectedProducers? projectedProducers = null;
        CalcResultScaledupProducers? scaledUpProducers = null;

        if (runContext.RequiresModulation)
        {
            (producers, projectedProducers) = logger.LogDuration(
                // ReSharper disable once AccessToModifiedClosure
                () => projectedProducersBuilder.Construct(runContext, materials, producers),
                nameof(projectedProducersBuilder));

            await logger.LogDuration(
                () => calcResultWriter.StoreProjectedH1Data(runContext.RunId, projectedProducers.H1ProjectedProducers, cancellationToken),
                nameof(calcResultWriter.StoreProjectedH1Data));

            await logger.LogDuration(
                () => calcResultWriter.StoreProjectedH2Data(runContext.RunId, projectedProducers.H2ProjectedProducers, cancellationToken),
                nameof(calcResultWriter.StoreProjectedH2Data));
        }

        if (runContext.RequiresScaling)
        {
            (producers, scaledUpProducers) = await logger.LogDuration(
                () => scaledUpProducersBuilder.ConstructAsync(runContext, materials, producers),
                nameof(scaledUpProducersBuilder));

            await logger.LogDuration(
                () => calcResultWriter.StoreScaledData(runContext.RunId, scaledUpProducers.ScaledupProducers, cancellationToken),
                nameof(calcResultWriter.StoreScaledData));
        }

        (producers, var partialObligations) = await logger.LogDuration(
            () => partialObligationsBuilder.ConstructAsync(runContext, materials, producers),
            nameof(partialObligationsBuilder));

        await logger.LogDuration(
            () => calcResultWriter.StorePartialData(runContext.RunId, partialObligations.PartialObligations, cancellationToken),
            nameof(calcResultWriter.StorePartialData));

        await logger.LogDuration(
            () => calcResultWriter.StoreProducerMaterialPackaging(producers, cancellationToken),
            nameof(calcResultWriter.StoreProducerMaterialPackaging));

        var selfManagedConsumerWaste = await logger.LogDuration(
            () => selfManagedConsumerWasteService.Calculate(runContext, materials),
            nameof(selfManagedConsumerWasteService));

        await logger.LogDuration(
            () => calcResultWriter.StoreSmcw(runContext.RunId, selfManagedConsumerWaste, cancellationToken),
            nameof(calcResultWriter.StoreSmcw));

        var disposalCost = await logger.LogDuration(
            () => laDisposalCostsBuilder.ConstructAsync(runContext, materials, lapcap, lateReportingTonnage, selfManagedConsumerWaste),
            nameof(laDisposalCostsBuilder));

        await logger.LogDuration(
            () => calcResultWriter.StoreLaDisposalCostData(runContext.RunId, disposalCost, cancellationToken),
            nameof(calcResultWriter.StoreLaDisposalCostData));

        var commsCost = await logger.LogDuration(
            () => commsCostsBuilder.ConstructAsync(runContext, materials, apportionment, lateReportingTonnage),
            nameof(commsCostsBuilder));

        await logger.LogDuration(
            () => calcResultWriter.StoreCommsCost(runContext.RunId, commsCost, cancellationToken),
            nameof(calcResultWriter.StoreCommsCost));

        ModulationResult? modulation = null;

        if (runContext.RequiresModulation)
        {
            modulation = await logger.LogDuration(
                () => modulationBuilder.ConstructAsync(runContext, materials, disposalCost, selfManagedConsumerWaste),
                nameof(modulationBuilder));

            await logger.LogDuration(
                () => calcResultWriter.StoreModulationResult(runContext.RunId, modulation, cancellationToken),
                nameof(calcResultWriter.StoreModulationResult));
        }

        var producerFees = await logger.LogDuration(
            () => producerFeesBuilder.ConstructAsync(runContext, new FeesState
            {
                Materials     = materials,
                CommsCost     = commsCost,
                OtherCost     = otherCost,
                Apportionment = apportionment,
                DisposalCost  = disposalCost,
                Modulation    = modulation,
                LapcapData    = lapcap,
                Smcw          = selfManagedConsumerWaste
            }),
            nameof(producerFeesBuilder));

        await logger.LogDuration(
            () => calcResultWriter.StoreProducerFees(runContext.RunId, producerFees, cancellationToken),
            nameof(calcResultWriter.StoreProducerFees));

        var errors = logger.LogDuration(
            () => errorReportBuilder.Construct(runContext),
            nameof(errorReportBuilder));

        return new CalcResult
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
            CalcResultErrorReports = errors,
            CalcResultModulation = modulation,
            Smcw = selfManagedConsumerWaste,
            ProducerFees = producerFees
        };
    }
}
