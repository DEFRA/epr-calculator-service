using System.Collections.Generic;
using System.Threading.Tasks;
using EPR.Calculator.Service.Function.Builder.CancelledProducers;
using EPR.Calculator.Service.Function.Builder.CommsCost;
using EPR.Calculator.Service.Function.Builder.Detail;
using EPR.Calculator.Service.Function.Builder.ErrorReport;
using EPR.Calculator.Service.Function.Builder.LaDisposalCost;
using EPR.Calculator.Service.Function.Builder.Lapcap;
using EPR.Calculator.Service.Function.Builder.LateReportingTonnages;
using EPR.Calculator.Service.Function.Builder.OnePlusFourApportionment;
using EPR.Calculator.Service.Function.Builder.ParametersOther;
using EPR.Calculator.Service.Function.Builder.RejectedProducers;
using EPR.Calculator.Service.Function.Builder.ScaledupProducers;
using EPR.Calculator.Service.Function.Builder.PartialObligations;
using EPR.Calculator.Service.Function.Builder.ProjectedProducers;
using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Models;
using Microsoft.ApplicationInsights;
using EPR.Calculator.Service.Function.Services;
using System.Linq;
using EPR.Calculator.Service.Function.Misc;
using System.Diagnostics.CodeAnalysis;
using static EPR.Calculator.Service.Function.Builder.LaDisposalCost.CalcRunLaDisposalCostBuilder;
using EPR.Calculator.API.Data;
using Microsoft.EntityFrameworkCore;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.Builder
{
    public class CalcResultBuilder : ICalcResultBuilder
    {
        private readonly ICalcResultParameterOtherCostBuilder calcResultParameterOtherCostBuilder;
        private readonly ICalcResultDetailBuilder calcResultDetailBuilder;
        private readonly ICalcResultLapcapDataBuilder lapcapBuilder;
        private readonly ICalcResultSummaryBuilder summaryBuilder;
        private readonly ICalcResultOnePlusFourApportionmentBuilder lapcapplusFourApportionmentBuilder;
        private readonly ICalcResultCommsCostBuilder commsCostReportBuilder;
        private readonly ICalcResultLateReportingBuilder lateReportingBuilder;
        private readonly ICalcRunLaDisposalCostBuilder laDisposalCostBuilder;
        private readonly ICalcResultScaledupProducersBuilder calcResultScaledupProducersBuilder;
        private readonly ICalcResultProjectedProducersBuilder calcResultProjectedProducersBuilder;
        private readonly ICalcResultPartialObligationBuilder calcResultPartialObligationBuilder;
        public readonly ICalcResultCancelledProducersBuilder calcResultCancelledProducersBuilder;
        public readonly ICalcResultRejectedProducersBuilder calcResultRejectedProducersBuilder;
        public readonly ICalcResultErrorReportBuilder calcResultErrorReportBuilder;
        public readonly IProjectedProducersService projectedProducersService;
        public readonly ILevelledProducerService levelledProducerService;
        private readonly TelemetryClient _telemetryClient;


        [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
        public CalcResultBuilder(
            ICalcResultDetailBuilder calcResultDetail,
            ICalcResultLapcapDataBuilder lapcap,
            ICalcResultParameterOtherCostBuilder calcResultParameterOtherCost,
            ICalcResultOnePlusFourApportionmentBuilder calcResultOnePlusFourApportionment,
            ICalcResultCommsCostBuilder commsCostReport,
            ICalcResultLateReportingBuilder lateReporting,
            ICalcRunLaDisposalCostBuilder calcRunLaDisposalCost,
            ICalcResultScaledupProducersBuilder calcResultScaledupProducers,
            ICalcResultPartialObligationBuilder calcResultPartialObligation,
            ICalcResultProjectedProducersBuilder calcResultProjectedProducers,
            ICalcResultSummaryBuilder summary,
            ICalcResultCancelledProducersBuilder calcResultCancelledProducers,
            ICalcResultRejectedProducersBuilder calcResultRejectedProducers,
            ICalcResultErrorReportBuilder calcResultErrorReport,
            IProjectedProducersService projectedProducers,
            ILevelledProducerService levelledProducerService,
            TelemetryClient telemetryClient)
        {
            calcResultDetailBuilder = calcResultDetail;
            lapcapBuilder = lapcap;
            commsCostReportBuilder = commsCostReport;
            lateReportingBuilder = lateReporting;
            calcResultParameterOtherCostBuilder = calcResultParameterOtherCost;
            laDisposalCostBuilder = calcRunLaDisposalCost;
            lapcapplusFourApportionmentBuilder = calcResultOnePlusFourApportionment;
            calcResultScaledupProducersBuilder = calcResultScaledupProducers;
            calcResultProjectedProducersBuilder = calcResultProjectedProducers;
            calcResultPartialObligationBuilder = calcResultPartialObligation;
            summaryBuilder = summary;
            calcResultCancelledProducersBuilder = calcResultCancelledProducers;
            calcResultRejectedProducersBuilder = calcResultRejectedProducers;
            calcResultErrorReportBuilder = calcResultErrorReport;
            projectedProducersService = projectedProducers;
            this.levelledProducerService = levelledProducerService;
            _telemetryClient = telemetryClient;
        }

        public async Task<CalcResult> BuildAsync(CalcResultsRequestDto resultsRequestDto)
        {
            var result = new CalcResult
            {
                CalcResultDetail =  await calcResultDetailBuilder.ConstructAsync(resultsRequestDto),
                CalcResultLapcapData =
                new CalcResultLapcapData
                {
                    CalcResultLapcapDataDetails = new List<CalcResultLapcapDataDetails>(),
                },
                CalcResultLateReportingTonnageData = new CalcResultLateReportingTonnage
                {
                    CalcResultLateReportingTonnageDetails = new List<CalcResultLateReportingTonnageDetail>(),
                },
                CalcResultParameterOtherCost = new CalcResultParameterOtherCost
                {
                    Name = string.Empty,
                },
                CalcResultPartialObligations = new CalcResultPartialObligations(),
                CalcResultProjectedProducers = new CalcResultProjectedProducers(),
                CalcResultScaledupProducers = new CalcResultScaledupProducers(),
                CalcResultCancelledProducers = new CalcResultCancelledProducersResponse(),
                CalcResultRejectedProducers = new List<CalcResultRejectedProducer>(),

                #pragma warning disable S1135 // Sonar TODO comment
                CalcResultModulation = resultsRequestDto.RelativeYear.Value >= 2026 ? "" : null // TODO add modulation class here for CSV section - not part of this ticket
                #pragma warning restore S1135
            };

            _telemetryClient.TrackTrace("lapcapBuilder started...");
            result.CalcResultLapcapData = await lapcapBuilder.ConstructAsync(resultsRequestDto);
            _telemetryClient.TrackTrace("lapcapBuilder end...");

            _telemetryClient.TrackTrace("lateReportingBuilder started...");
            result.CalcResultLateReportingTonnageData = await lateReportingBuilder.ConstructAsync(resultsRequestDto);
            _telemetryClient.TrackTrace("lateReportingBuilder end...");

            _telemetryClient.TrackTrace("calcResultParameterOtherCostBuilder started...");
            result.CalcResultParameterOtherCost = await calcResultParameterOtherCostBuilder.ConstructAsync(resultsRequestDto);
            _telemetryClient.TrackTrace("calcResultParameterOtherCostBuilder end...");

            _telemetryClient.TrackTrace("lapcapplusFourApportionmentBuilder started...");
            result.CalcResultOnePlusFourApportionment = lapcapplusFourApportionmentBuilder.ConstructAsync(resultsRequestDto, result);
            _telemetryClient.TrackTrace("lapcapplusFourApportionmentBuilder end...");

            _telemetryClient.TrackTrace("CalcResultCancelledProducersBuilder started...");
            result.CalcResultCancelledProducers = await calcResultCancelledProducersBuilder.ConstructAsync(resultsRequestDto);
            _telemetryClient.TrackTrace("CalcResultCancelledProducersBuilder end...");

            var materials = new List<MaterialDetail>();// TODO lookup from MaterialService
            List<ProducerReportedMaterialsForSubmissionPeriod> producers1 = await levelledProducerService.GetProducers(resultsRequestDto.RunId, materials);

            List<ProducerReportedMaterialsForSubmissionPeriod> producers2 = null;
            if (result.CalcResultModulation is not null)
            {
                _telemetryClient.TrackTrace("calcResultProjectedProducerBuilder started...");
                var calcResultProjectedProducers = await calcResultProjectedProducersBuilder.ConstructAsync(resultsRequestDto, producers1);
                producers2 = calcResultProjectedProducers.Item1;
                result.CalcResultProjectedProducers = calcResultProjectedProducers.Item2;
                _telemetryClient.TrackTrace("calcResultProjectedProducerBuilder end...");
            } else
            {
                producers2 = producers1;
            }

            List<ProducerReportedMaterialsForSubmissionPeriod> producers3 = null;
            if (resultsRequestDto.RelativeYear.Value == 2025)
            {
                _telemetryClient.TrackTrace("calcResultScaledupProducersBuilder started...");
                var scaledupProducersResult = await calcResultScaledupProducersBuilder.ConstructAsync(resultsRequestDto, producers2);
                result.CalcResultScaledupProducers = scaledupProducersResult.Item2;
                producers3 = scaledupProducersResult.Item1;
                _telemetryClient.TrackTrace("calcResultScaledupProducersBuilder end...");
            } else
            {
                producers3 = producers2;
            }

            _telemetryClient.TrackTrace("calcResultPartialObligationBuilder started...");
            var partialObligationsResult = await calcResultPartialObligationBuilder.ConstructAsync(resultsRequestDto, producers3);// result.CalcResultScaledupProducers?.ScaledupProducers ?? new List<CalcResultScaledupProducer>());
            result.CalcResultPartialObligations = partialObligationsResult.Item2;
            IEnumerable<ProducerReportedMaterialsForSubmissionPeriod> producers4 = partialObligationsResult.Item1;
            _telemetryClient.TrackTrace("calcResultPartialObligationBuilder end...");

            if (!resultsRequestDto.IsBillingFile)
            {
                _telemetryClient.TrackTrace("Storing projected producers started...");
                //await projectedProducersService.StoreProjectedProducers(
                //    resultsRequestDto.RunId,
                //    producers4
                //);
                _telemetryClient.TrackTrace("Storing projected producers ended...");
            }

            if (resultsRequestDto.IsBillingFile)
            {
                _telemetryClient.TrackTrace("CalcResultRejectedProducersBuilder started...");
                result.CalcResultRejectedProducers = await calcResultRejectedProducersBuilder.ConstructAsync(resultsRequestDto);
                _telemetryClient.TrackTrace("CalcResultRejectedProducersBuilder end...");
            }

            _telemetryClient.TrackTrace("laDisposalCostBuilder started...");
            result.CalcResultLaDisposalCostData = await laDisposalCostBuilder.ConstructAsync(resultsRequestDto, result);
            _telemetryClient.TrackTrace("laDisposalCostBuilder end...");

            _telemetryClient.TrackTrace("commsCostReportBuilder started...");
            result.CalcResultCommsCostReportDetail = await commsCostReportBuilder.ConstructAsync(resultsRequestDto, result.CalcResultOnePlusFourApportionment, result);
            _telemetryClient.TrackTrace("commsCostReportBuilder end...");

            _telemetryClient.TrackTrace("summaryBuilder started...");
            result.CalcResultSummary = await summaryBuilder.ConstructAsync(resultsRequestDto.RunId, resultsRequestDto.RelativeYear, resultsRequestDto.IsBillingFile, result);
            _telemetryClient.TrackTrace("summaryBuilder end...");

            _telemetryClient.TrackTrace("Error report builder started...");
            result.CalcResultErrorReports = calcResultErrorReportBuilder.ConstructAsync(resultsRequestDto);
            _telemetryClient.TrackTrace("Error report builder end...");

            return result;
        }
    }
}
