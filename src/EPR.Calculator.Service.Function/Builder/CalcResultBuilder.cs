using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.Service.Function.Builder.CancelledProducers;
using EPR.Calculator.Service.Function.Builder.CommsCost;
using EPR.Calculator.Service.Function.Builder.Detail;
using EPR.Calculator.Service.Function.Builder.ErrorReport;
using EPR.Calculator.Service.Function.Builder.LaDisposalCost;
using EPR.Calculator.Service.Function.Builder.Lapcap;
using EPR.Calculator.Service.Function.Builder.LateReportingTonnages;
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
using Microsoft.Extensions.Logging;

namespace EPR.Calculator.Service.Function.Builder;

[SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
public class CalcResultBuilder(
    ICalcResultDetailBuilder calcResultDetailBuilder,
    ICalcResultLapcapDataBuilder lapcapBuilder,
    ICalcResultParameterOtherCostBuilder calcResultParameterOtherCostBuilder,
    ICalcResultOnePlusFourApportionmentBuilder lapcapplusFourApportionmentBuilder,
    ICalcResultCommsCostBuilder commsCostReportBuilder,
    ICalcResultLateReportingBuilder lateReportingBuilder,
    ICalcRunLaDisposalCostBuilder laDisposalCostBuilder,
    ICalcResultScaledupProducersBuilder calcResultScaledupProducersBuilder,
    ICalcResultPartialObligationBuilder calcResultPartialObligationBuilder,
    ICalcResultSummaryBuilder summaryBuilder,
    ICalcResultProjectedProducersBuilder calcResultProjectedProducersBuilder,
    ICalcResultCancelledProducersBuilder calcResultCancelledProducersBuilder,
    ICalcResultRejectedProducersBuilder calcResultRejectedProducersBuilder,
    ICalcResultErrorReportBuilder calcResultErrorReportBuilder,
    IProjectedProducersService projectedProducers,
    ILogger<CalcResultBuilder> logger)
    : ICalcResultBuilder
{
    public async Task<CalcResult> BuildAsync(RunContext runContext)
    {
        var result = new CalcResult
        {
            CalcResultDetail = await calcResultDetailBuilder.ConstructAsync(runContext),
            CalcResultLapcapData =
                new CalcResultLapcapData
                {
                    CalcResultLapcapDataDetails = new List<CalcResultLapcapDataDetails>()
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
            CalcResultRejectedProducers = new List<CalcResultRejectedProducer>(),

            #pragma warning disable S1135 // Sonar TODO comment
            CalcResultModulation = runContext.RelativeYear.Value >= 2026 ? "" : null // TODO add modulation class here for CSV section - not part of this ticket
            #pragma warning restore S1135
        };

        logger.LogDebug("lapcapBuilder started");
        result.CalcResultLapcapData = await lapcapBuilder.ConstructAsync(runContext);
        logger.LogDebug("lapcapBuilder finished");

        logger.LogDebug("lateReportingBuilder started");
        result.CalcResultLateReportingTonnageData = await lateReportingBuilder.ConstructAsync(runContext);
        logger.LogDebug("lateReportingBuilder finished");

        logger.LogDebug("calcResultParameterOtherCostBuilder started");
        result.CalcResultParameterOtherCost = await calcResultParameterOtherCostBuilder.ConstructAsync(runContext);
        logger.LogDebug("calcResultParameterOtherCostBuilder finished");

        logger.LogDebug("lapcapplusFourApportionmentBuilder started");
        result.CalcResultOnePlusFourApportionment = lapcapplusFourApportionmentBuilder.ConstructAsync(runContext, result);
        logger.LogDebug("lapcapplusFourApportionmentBuilder finished");

        logger.LogDebug("CalcResultCancelledProducersBuilder started");
        result.CalcResultCancelledProducers = await calcResultCancelledProducersBuilder.ConstructAsync(runContext);
        logger.LogDebug("CalcResultCancelledProducersBuilder finished");

        if (result.CalcResultModulation is not null)
        {
            logger.LogDebug("calcResultProjectedProducerBuilder started");
            var calcResultProjectedProducers = await calcResultProjectedProducersBuilder.ConstructAsync(runContext);
            result.CalcResultProjectedProducers = calcResultProjectedProducers;
            logger.LogDebug("calcResultProjectedProducerBuilder finished");
            
            if (runContext.RunType == RunType.Calculator)
            {
                logger.LogDebug("Storing projected producers started..");
                await projectedProducers.StoreProjectedProducers(runContext.RunId, calcResultProjectedProducers.H2ProjectedProducers?.ToList() ?? new List<CalcResultH2ProjectedProducer>());
                logger.LogDebug("Storing projected producers finished");
            }
        }
        else
        {
            logger.LogDebug("calcResultScaledupProducersBuilder started...");
            var scaledupProducersResult = await calcResultScaledupProducersBuilder.ConstructAsync(runContext);
            result.CalcResultScaledupProducers = scaledupProducersResult;
            logger.LogDebug("calcResultScaledupProducersBuilder finished");
        }

        logger.LogDebug("calcResultPartialObligationBuilder started");
        result.CalcResultPartialObligations = await calcResultPartialObligationBuilder.ConstructAsync(runContext, result.CalcResultScaledupProducers.ScaledupProducers ?? new List<CalcResultScaledupProducer>());
        logger.LogDebug("calcResultPartialObligationBuilder finished");

        if (runContext.RunType == RunType.Billing)
        {
            logger.LogDebug("CalcResultRejectedProducersBuilder started");
            result.CalcResultRejectedProducers = await calcResultRejectedProducersBuilder.ConstructAsync(runContext);
            logger.LogDebug("CalcResultRejectedProducersBuilder finished");
        }

        logger.LogDebug("laDisposalCostBuilder started");
        result.CalcResultLaDisposalCostData = await laDisposalCostBuilder.ConstructAsync(runContext, result);
        logger.LogDebug("laDisposalCostBuilder finished");

        logger.LogDebug("commsCostReportBuilder started");
        result.CalcResultCommsCostReportDetail = await commsCostReportBuilder.ConstructAsync(runContext, result.CalcResultOnePlusFourApportionment, result);
        logger.LogDebug("commsCostReportBuilder finished");

        logger.LogDebug("summaryBuilder started");
        result.CalcResultSummary = await summaryBuilder.ConstructAsync(runContext, result);
        logger.LogDebug("summaryBuilder finished");

        logger.LogDebug("Error report builder started");
        result.CalcResultErrorReports = calcResultErrorReportBuilder.ConstructAsync(runContext);
        logger.LogDebug("Error report builder finished");

        return result;
    }
}