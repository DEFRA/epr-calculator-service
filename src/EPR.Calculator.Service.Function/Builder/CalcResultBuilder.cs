using System.Diagnostics.CodeAnalysis;
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
using EPR.Calculator.Service.Function.Builder.RejectedProducers;
using EPR.Calculator.Service.Function.Builder.ScaledupProducers;
using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Logging;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;

namespace EPR.Calculator.Service.Function.Builder;

public interface ICalcResultBuilder
{
    Task<CalcResult> BuildAsync(RunContext runContext, CancellationToken cancellationToken);
}

[SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
public class CalcResultBuilder(
    IParameterService parameterService,
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
    ICalcResultRejectedProducersBuilder rejectedProducersBuilder,
    ICalcResultErrorReportBuilder errorReportBuilder,
    ISelfManagedConsumerWasteService selfManagedConsumerWasteService,
    ICalcResultModulationBuilder modulationBuilder,
    ICalcResultReader calcResultReader,
    ICalcResultWriter calcResultWriter,
    IMaterialService materialService,
    ITelemetryClient telemetryClient,
    ILogger<CalcResultBuilder> logger
)  : ICalcResultBuilder
{
    public Task<CalcResult> BuildAsync(RunContext runContext, CancellationToken cancellationToken) =>
        telemetryClient.TrackDuration($"{runContext.RunType}RunResultBuilder", () => BuildResult(runContext, cancellationToken));

    private async Task<CalcResult> BuildResult(RunContext runContext, CancellationToken cancellationToken)
    {
        var result = new CalcResult
        {
            CalcResultDetail = await calcResultDetailBuilder.ConstructAsync(runContext),
            CalcResultLapcapData = new CalcResultLapcapData
            {
                ByMaterial = []
            },
            CalcResultLateReportingTonnageData = new CalcResultLateReportingTonnage
            {
                ByMaterial = []
            },
            CalcResultParameterOtherCost = new CalcResultParameterOtherCost(),
            CalcResultPartialObligations = new CalcResultPartialObligations(){
                PartialObligations = [],
            },
            CalcResultProjectedProducers = new CalcResultProjectedProducers(){
                H1ProjectedProducers = [],
                H2ProjectedProducers = []
            },
            CalcResultScaledupProducers = new CalcResultScaledupProducers(){
                ScaledupProducers = [],
            },
            CalcResultCancelledProducers = new CalcResultCancelledProducersResponse(),
            CalcResultRejectedProducers = []
        };

        var materials = await materialService.GetMaterials();
        var defaultParams = await parameterService.GetDefaultParameters(runContext);

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

        result.CalcResultCancelledProducers = await logger.LogDuration(
            () => cancelledProducersBuilder.ConstructAsync(runContext, materials),
            nameof(cancelledProducersBuilder));

        // ReSharper disable AccessToModifiedClosure - LogDuration always immediately invokes the delegate
        if (runContext.RunType == RunType.Billing)
        {
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
        }
        else {
            var producers = await reportedProducersService.GetProducers(runContext);

            if (runContext.RequiresModulation)
            {
                (producers, result.CalcResultProjectedProducers) = logger.LogDuration(
                    () => projectedProducersBuilder.Construct(runContext, materials, producers),
                    nameof(projectedProducersBuilder));

                await logger.LogDuration(
                    () => calcResultWriter.StoreProjectedH1Data(runContext.RunId, result.CalcResultProjectedProducers.H1ProjectedProducers, cancellationToken),
                    nameof(calcResultWriter.StoreProjectedH1Data));

                await logger.LogDuration(
                    () => calcResultWriter.StoreProjectedH2Data(runContext.RunId, result.CalcResultProjectedProducers.H2ProjectedProducers, cancellationToken),
                    nameof(calcResultWriter.StoreProjectedH2Data));
            }

            if (runContext.RequiresScaling)
            {
                (producers, result.CalcResultScaledupProducers) = await logger.LogDuration(
                    () => scaledUpProducersBuilder.ConstructAsync(runContext, materials, producers),
                    nameof(scaledUpProducersBuilder));

                await logger.LogDuration(
                    () => calcResultWriter.StoreScaledData(runContext.RunId, result.CalcResultScaledupProducers.ScaledupProducers, cancellationToken),
                    nameof(calcResultWriter.StoreScaledData));
            }

            (producers, result.CalcResultPartialObligations) = await logger.LogDuration(
                () => partialObligationsBuilder.ConstructAsync(runContext, materials, producers),
                nameof(partialObligationsBuilder));

            await logger.LogDuration(
                () => calcResultWriter.StorePartialData(runContext.RunId, result.CalcResultPartialObligations.PartialObligations, cancellationToken),
                nameof(calcResultWriter.StorePartialData));

            await logger.LogDuration(
                () => calcResultWriter.StoreProducerMaterialPackaging(producers, cancellationToken),
                nameof(calcResultWriter.StoreProducerMaterialPackaging));

            result.Smcw = await logger.LogDuration(
                () => selfManagedConsumerWasteService.Calculate(runContext, materials),
                nameof(selfManagedConsumerWasteService));

            await logger.LogDuration(
                () => calcResultWriter.StoreSmcw(runContext.RunId, result.Smcw, cancellationToken),
                nameof(calcResultWriter.StoreSmcw));

            result.CalcResultLaDisposalCostData = await logger.LogDuration(
                () => laDisposalCostsBuilder.ConstructAsync(runContext, materials, result.CalcResultLapcapData, result.CalcResultLateReportingTonnageData, result.Smcw),
                nameof(laDisposalCostsBuilder));

            result.CalcResultCommsCostReportDetail = await logger.LogDuration(
                () => commsCostsBuilder.ConstructAsync(runContext, materials, result.CalcResultOnePlusFourApportionment, result.CalcResultLateReportingTonnageData),
                nameof(commsCostsBuilder));

            if (runContext.RequiresModulation)
            {
                result.CalcResultModulation = await logger.LogDuration(
                    () => modulationBuilder.ConstructAsync(defaultParams, materials, result.CalcResultLaDisposalCostData, result.Smcw),
                    nameof(modulationBuilder));

                await logger.LogDuration(
                    () => calcResultWriter.StoreModulationResult(runContext.RunId, result.CalcResultModulation, cancellationToken),
                    nameof(calcResultWriter.StoreModulationResult));
            }

            result.ProducerFees = await logger.LogDuration(
                () => producerFeesBuilder.ConstructAsync(runContext, materials, result, result.Smcw),
                nameof(producerFeesBuilder));

            await logger.LogDuration(
                () => calcResultWriter.StoreProducerFees(runContext.RunId, result.ProducerFees, cancellationToken),
                nameof(calcResultWriter.StoreProducerFees));    

            result.CalcResultErrorReports = logger.LogDuration(
                () => errorReportBuilder.Construct(runContext),
                nameof(errorReportBuilder));
        }

        return result;
    }
}
