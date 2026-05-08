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
using EPR.Calculator.Service.Function.Builder.RejectedProducers;
using EPR.Calculator.Service.Function.Builder.ScaledupProducers;
using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.Services.Telemetry;

namespace EPR.Calculator.Service.Function.Builder;

public interface ICalcResultBuilder
{
    Task<CalcResult> BuildAsync(RunContext runContext);
}

[SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
public class CalcResultBuilder(
    IParameterService parameterService,
    IMaterialService materialService,
    ICalcResultDetailBuilder resultDetails,
    ICalcResultLapcapDataBuilder lapcapData,
    ICalcResultParameterOtherCostBuilder otherCosts,
    ICalcResultOnePlusFourApportionmentBuilder onePlusFourApportionment,
    ICalcResultCommsCostBuilder commsCosts,
    ICalcResultLateReportingBuilder lateReporting,
    ICalcRunLaDisposalCostBuilder laDisposalCosts,
    ICalcResultScaledupProducersBuilder scaledupProducers,
    ICalcResultPartialObligationBuilder partialObligations,
    ICalcResultSummaryBuilder summaryBuilder,
    ICalcResultProjectedProducersBuilder projectedProducers,
    ICalcResultCancelledProducersBuilder cancelledProducers,
    ICalcResultRejectedProducersBuilder rejectedProducers,
    ICalcResultErrorReportBuilder errorReports,
    ICalcResultModulationBuilder modulations,
    IProjectedProducersService projectedProducersSvc,
    ISelfManagedConsumerWasteService selfManagedConsumerWasteSvc,
    IReportedProducerService reportedProducersSvc,
    ITelemetryClient telemetry)
    : ICalcResultBuilder
{
    public async Task<CalcResult> BuildAsync(RunContext runContext)
    {
        var defaultParams = await parameterService.GetDefaultParameters(runContext.RunId);
        var materials = await materialService.GetMaterials();

        var result = new CalcResult
        {
            CalcResultDetail = await resultDetails.ConstructAsync(runContext),
            CalcResultLapcapData = new CalcResultLapcapData
            {
                CalcResultLapcapDataDetails = new List<CalcResultLapcapDataDetail>()
            },
            CalcResultLateReportingTonnageData = new CalcResultLateReportingTonnage
            {
                CalcResultLateReportingTonnageDetails = new List<CalcResultLateReportingTonnageDetail>()
            },
            CalcResultParameterOtherCost = new CalcResultParameterOtherCost
            {
                Name = string.Empty
            },
            CalcResultPartialObligations = new CalcResultPartialObligations(),
            CalcResultProjectedProducers = new CalcResultProjectedProducers(),
            CalcResultScaledupProducers = new CalcResultScaledupProducers(),
            CalcResultCancelledProducers = new CalcResultCancelledProducersResponse(),
            CalcResultRejectedProducers = new List<CalcResultRejectedProducer>()
        };

        result.CalcResultLapcapData = await telemetry.TrackDuration("ResultBuilder.Lapcap", () =>
            lapcapData.ConstructAsync(runContext, materials));

        result.CalcResultLateReportingTonnageData = await telemetry.TrackDuration("ResultBuilder.LateReporting", () =>
            lateReporting.ConstructAsync(runContext));

        result.CalcResultParameterOtherCost = await telemetry.TrackDuration("ResultBuilder.OtherCost", () =>
            otherCosts.ConstructAsync(runContext));

        result.CalcResultOnePlusFourApportionment = telemetry.TrackDuration("ResultBuilder.OnePlusFourApportionment", () =>
            onePlusFourApportionment.Construct(result.CalcResultLapcapData, result.CalcResultParameterOtherCost));

        result.CalcResultCancelledProducers = await telemetry.TrackDuration("ResultBuilder.CancelledProducers", () =>
            cancelledProducers.ConstructAsync(runContext));

        var producers1 = await reportedProducersSvc.GetProducers(runContext.RunId);

        List<ProducerDetail> producers2;
        if (runContext.RequiresModulation)
        {
            var calcResultProjectedProducers = telemetry.TrackDuration("ResultBuilder.ProjectedProducers", () =>
                projectedProducers.Construct(runContext, materials, producers1));

            producers2 = calcResultProjectedProducers.Item1;
            result.CalcResultProjectedProducers = calcResultProjectedProducers.Item2;
        }
        else
            producers2 = producers1;

        List<ProducerDetail> producers3;

        if (runContext.RequiresScaledUpProducers)
        {
            var scaledupProducersResult = await telemetry.TrackDuration("ResultBuilder.ScaledUpProducers", () =>
                scaledupProducers.ConstructAsync(runContext, materials, producers2));

            result.CalcResultScaledupProducers = scaledupProducersResult.Item2;
            producers3 = scaledupProducersResult.Item1;
        }
        else
            producers3 = producers2;

        var producers4 = await telemetry.TrackDuration("ResultBuilder.PartialObligations", async () =>
        {
            var partialObligationsResult = await partialObligations.ConstructAsync(runContext, materials, producers3);
            result.CalcResultPartialObligations = partialObligationsResult.Item2;
            return partialObligationsResult.Item1;
        });

        if (runContext.RunType == RunType.Calculator)
        {
            await telemetry.TrackDuration("ResultBuilder.StoreProjectedProducers", () =>
                projectedProducersSvc.StoreProjectedProducers(runContext, producers4));
        }

        if (runContext.RunType == RunType.Billing)
        {
            result.CalcResultRejectedProducers = await telemetry.TrackDuration("ResultBuilder.RejectedProducers", () =>
                rejectedProducers.ConstructAsync(runContext));
        }

        result.Smcw = await telemetry.TrackDuration("ResultBuilder.SelfManagedConsumerWaste", () =>
            selfManagedConsumerWasteSvc.Calculate(runContext, materials));

        result.CalcResultLaDisposalCostData = await telemetry.TrackDuration("ResultBuilder.LaDisposalCost", () =>
            laDisposalCosts.ConstructAsync(runContext, materials, result.CalcResultLapcapData, result.CalcResultLateReportingTonnageData, result.Smcw));

        result.CalcResultCommsCostReportDetail = await telemetry.TrackDuration("ResultBuilder.CommsCost", () =>
            commsCosts.ConstructAsync(runContext, materials, result.CalcResultOnePlusFourApportionment, result.CalcResultLateReportingTonnageData));

        if (runContext.RequiresModulation)
        {
            result.CalcResultModulation = await telemetry.TrackDuration("ResultBuilder.Modulation", () =>
                modulations.ConstructAsync(defaultParams, materials, result.CalcResultLaDisposalCostData, result.Smcw));
        }

        result.CalcResultSummary = await telemetry.TrackDuration("ResultBuilder.Summary", () =>
            summaryBuilder.ConstructAsync(runContext, materials, result, result.Smcw));

        result.CalcResultErrorReports = await telemetry.TrackDuration("ResultBuilder.ErrorReports", () =>
            errorReports.ConstructAsync(runContext));

        return result;
    }
}
