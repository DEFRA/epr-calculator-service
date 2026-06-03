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
    Task<CalcResult> BuildAsync(RunContext runContext);
}

[SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
public class CalcResultBuilder(
    IParameterService parameterService,
    ICalcResultLapcapDataBuilder lapcapData,
    ICalcResultLateReportingBuilder lateReportingTonnage,
    ICalcResultParameterOtherCostBuilder otherCosts,
    ICalcResultOnePlusFourApportionmentBuilder onePlusFourApportionment,
    ICalcResultCancelledProducersBuilder cancelledProducers,
    IReportedProducerService reportedProducers,
    ICalcResultProjectedProducersBuilder projectedProducers,
    ICalcResultScaledupProducersBuilder scaledUpProducers,
    ICalcResultDetailBuilder calcResultDetailBuilder,
    ICalcResultCommsCostBuilder commsCosts,
    ICalcRunLaDisposalCostBuilder laDisposalCosts,
    ICalcResultPartialObligationBuilder partialObligations,
    ICalcResultSummaryBuilder summary,
    ICalcResultRejectedProducersBuilder rejectedProducers,
    ICalcResultErrorReportBuilder errorReport,
    IProjectedProducersService projectedProducersService,
    ISelfManagedConsumerWasteService selfManagedConsumerWaste,
    ICalcResultModulationBuilder modulation,
    ICalcResultService calcResultService,
    IMaterialService materialService,
    ITelemetryClient telemetryClient,
    ILogger<CalcResultBuilder> logger)
    : ICalcResultBuilder
{
    public Task<CalcResult> BuildAsync(RunContext runContext) =>
        telemetryClient.TrackDuration($"{runContext.RunType}RunResultBuilder", () => BuildResult(runContext));

    private async Task<CalcResult> BuildResult(RunContext runContext)
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
                PartialObligations = ImmutableList<CalcResultPartialObligation>.Empty,
            },
            CalcResultProjectedProducers = new CalcResultProjectedProducers(){
                H1ProjectedProducers = ImmutableList<CalcResultH1ProjectedProducer>.Empty,
                H2ProjectedProducers = ImmutableList<CalcResultH2ProjectedProducer>.Empty
            },
            CalcResultScaledupProducers = new CalcResultScaledupProducers(){
                ScaledupProducers = ImmutableList<CalcResultScaledupProducer>.Empty,
            },
            CalcResultCancelledProducers = new CalcResultCancelledProducersResponse(),
            CalcResultRejectedProducers = new List<CalcResultRejectedProducer>()
        };

        var materials = await materialService.GetMaterials();
        var defaultParams = await parameterService.GetDefaultParameters(runContext);

        result.CalcResultLapcapData = await logger.LogDuration(() =>
                lapcapData.ConstructAsync(runContext, materials),
            nameof(lapcapData));

        result.CalcResultLateReportingTonnageData = await logger.LogDuration(() =>
                lateReportingTonnage.ConstructAsync(runContext, materials),
            nameof(lateReportingTonnage));

        result.CalcResultParameterOtherCost = await logger.LogDuration(() =>
                otherCosts.ConstructAsync(runContext),
            nameof(otherCosts));

        result.CalcResultOnePlusFourApportionment = logger.LogDuration(() =>
                onePlusFourApportionment.Construct(result),
            nameof(onePlusFourApportionment));

        result.CalcResultCancelledProducers = await logger.LogDuration(() =>
                cancelledProducers.ConstructAsync(runContext, materials),
            nameof(cancelledProducers));

        // ReSharper disable AccessToModifiedClosure - LogDuration always immediately invokes the delegate
        if (runContext.RunType == RunType.Billing)
        {
            if (runContext.RequiresModulation)
            {
                result.CalcResultProjectedProducers.H1ProjectedProducers = (await logger.LogDuration(() =>
                    calcResultService.ReadH1ProjectedData(runContext.RunId),
                nameof(calcResultService.ReadH1ProjectedData))).ToImmutableList();

                result.CalcResultProjectedProducers.H2ProjectedProducers = (await logger.LogDuration(() =>
                    calcResultService.ReadH2ProjectedData(runContext.RunId),
                nameof(calcResultService.ReadH2ProjectedData))).ToImmutableList();
            }

            if (runContext.RequiresScaling)
            {
                result.CalcResultScaledupProducers.ScaledupProducers = (await logger.LogDuration(() =>
                    calcResultService.ReadScaledData(runContext.RunId),
                nameof(calcResultService.ReadScaledData))).ToImmutableList();
            }

            result.CalcResultPartialObligations.PartialObligations = (await logger.LogDuration(() =>
                    calcResultService.ReadPartialData(runContext.RunId),
                nameof(calcResultService.ReadPartialData))).ToImmutableList();

            result.CalcResultRejectedProducers = await logger.LogDuration(() =>
                    rejectedProducers.ConstructAsync(runContext),
                nameof(rejectedProducers));
        }
        else {
            var producers = await reportedProducers.GetProducers(runContext);
            
            if (runContext.RequiresModulation)
            {
                (producers, result.CalcResultProjectedProducers) = logger.LogDuration(() =>
                        projectedProducers.Construct(runContext, materials, producers),
                    nameof(projectedProducers));
                
                await logger.LogDuration(() =>
                    calcResultService.StoreProjectedH1Data(runContext.RunId, result.CalcResultProjectedProducers.H1ProjectedProducers),
                nameof(calcResultService.StoreProjectedH1Data));

                await logger.LogDuration(() =>
                    calcResultService.StoreProjectedH2Data(runContext.RunId, result.CalcResultProjectedProducers.H2ProjectedProducers),
                nameof(calcResultService.StoreProjectedH2Data));
            }

            if (runContext.RequiresScaling)
            {
                (producers, result.CalcResultScaledupProducers) = await logger.LogDuration(() =>
                        scaledUpProducers.ConstructAsync(runContext, materials, producers),
                    nameof(scaledUpProducers));

                await logger.LogDuration(() =>
                    calcResultService.StoreScaledData(runContext.RunId, result.CalcResultScaledupProducers.ScaledupProducers),
                nameof(calcResultService.StoreScaledData));
            }

            (producers, result.CalcResultPartialObligations) = await logger.LogDuration(() =>
                    partialObligations.ConstructAsync(runContext, materials, producers),
                nameof(partialObligations));

            await logger.LogDuration(() =>
                    calcResultService.StorePartialData(runContext.RunId, result.CalcResultPartialObligations.PartialObligations),
                nameof(calcResultService.StorePartialData));

            await logger.LogDuration(() =>
                    projectedProducersService.StoreProjectedProducers(producers),
                nameof(projectedProducersService.StoreProjectedProducers));
        }
        
        result.Smcw = await logger.LogDuration(() =>
                selfManagedConsumerWaste.Calculate(runContext, materials),
            nameof(selfManagedConsumerWaste));

        result.CalcResultLaDisposalCostData = await logger.LogDuration(() =>
                laDisposalCosts.ConstructAsync(runContext, materials, result.CalcResultLapcapData, result.CalcResultLateReportingTonnageData, result.Smcw),
            nameof(laDisposalCosts));

        result.CalcResultCommsCostReportDetail = await logger.LogDuration(() =>
                commsCosts.ConstructAsync(runContext, materials, result.CalcResultOnePlusFourApportionment, result.CalcResultLateReportingTonnageData),
            nameof(commsCosts));

        if (runContext.RequiresModulation)
        {
            result.CalcResultModulation = await logger.LogDuration(() =>
                    modulation.ConstructAsync(defaultParams, materials, result.CalcResultLaDisposalCostData, result.Smcw),
                nameof(modulation));
        }

        result.CalcResultSummary = await logger.LogDuration(() =>
                summary.ConstructAsync(runContext, materials, result, result.Smcw),
            nameof(summary));

        result.CalcResultErrorReports = logger.LogDuration(() =>
                errorReport.Construct(runContext),
            nameof(errorReport));

        return result;
    }
}
