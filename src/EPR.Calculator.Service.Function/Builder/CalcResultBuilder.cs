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
using EPR.Calculator.Service.Function.Builder.RejectedProducers;
using EPR.Calculator.Service.Function.Builder.ScaledupProducers;
using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Models;
using Microsoft.ApplicationInsights;

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
        private readonly ICalcResultPartialObligationBuilder calcResultPartialObligationBuilder;
        public readonly ICalcResultCancelledProducersBuilder calcResultCancelledProducersBuilder;
        public readonly ICalcResultRejectedProducersBuilder calcResultRejectedProducersBuilder;
        public readonly ICalcResultErrorReportBuilder calcResultErrorReportBuilder;
        private readonly TelemetryClient _telemetryClient;


        [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
        /// <summary>
        /// Initializes a new instance of the <see cref="CalcResultBuilder"/> class.
        /// </summary>
        public CalcResultBuilder(
            ICalcResultDetailBuilder calcResultDetailBuilder,
            ICalcResultLapcapDataBuilder lapcapBuilder,
            ICalcResultParameterOtherCostBuilder calcResultParameterOtherCostBuilder,
            ICalcResultOnePlusFourApportionmentBuilder calcResultOnePlusFourApportionmentBuilder,
            ICalcResultCommsCostBuilder commsCostReportBuilder,
            ICalcResultLateReportingBuilder lateReportingBuilder,
            ICalcRunLaDisposalCostBuilder calcRunLaDisposalCostBuilder,
            ICalcResultScaledupProducersBuilder calcResultScaledupProducersBuilder,
            ICalcResultPartialObligationBuilder calcResultPartialObligationBuilder,
            ICalcResultSummaryBuilder summaryBuilder,
            ICalcResultCancelledProducersBuilder calcResultCancelledProducersBuilder,
            ICalcResultRejectedProducersBuilder calcResultRejectedProducersBuilder,
            ICalcResultErrorReportBuilder calcResultErrorReportBuilder,
            TelemetryClient telemetryClient)
        {
            this.calcResultDetailBuilder = calcResultDetailBuilder;
            this.lapcapBuilder = lapcapBuilder;
            this.commsCostReportBuilder = commsCostReportBuilder;
            this.lateReportingBuilder = lateReportingBuilder;
            this.calcResultParameterOtherCostBuilder = calcResultParameterOtherCostBuilder;
            laDisposalCostBuilder = calcRunLaDisposalCostBuilder;
            lapcapplusFourApportionmentBuilder = calcResultOnePlusFourApportionmentBuilder;
            this.calcResultScaledupProducersBuilder = calcResultScaledupProducersBuilder;
            this.calcResultPartialObligationBuilder = calcResultPartialObligationBuilder;
            this.summaryBuilder = summaryBuilder;
            this.calcResultCancelledProducersBuilder = calcResultCancelledProducersBuilder;
            this.calcResultRejectedProducersBuilder = calcResultRejectedProducersBuilder;
            this.calcResultErrorReportBuilder = calcResultErrorReportBuilder;
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
            _telemetryClient.TrackTrace("BuilateReportingBuilderlder end...");

            _telemetryClient.TrackTrace("calcResultParameterOtherCostBuilder started...");
            result.CalcResultParameterOtherCost = await calcResultParameterOtherCostBuilder.ConstructAsync(resultsRequestDto);
            _telemetryClient.TrackTrace("calcResultParameterOtherCostBuilder end...");

            _telemetryClient.TrackTrace("lapcapplusFourApportionmentBuilder started...");
            result.CalcResultOnePlusFourApportionment = lapcapplusFourApportionmentBuilder.ConstructAsync(resultsRequestDto, result);
            _telemetryClient.TrackTrace("lapcapplusFourApportionmentBuilder end...");

            _telemetryClient.TrackTrace("CalcResultCancelledProducersBuilder started...");
            result.CalcResultCancelledProducers = await calcResultCancelledProducersBuilder.ConstructAsync(resultsRequestDto);
            _telemetryClient.TrackTrace("CalcResultCancelledProducersBuilder end...");

            _telemetryClient.TrackTrace("calcResultScaledupProducersBuilder started...");
            var scaledupProducersResult = await calcResultScaledupProducersBuilder.ConstructAsync(resultsRequestDto);
            result.CalcResultScaledupProducers = scaledupProducersResult;
            _telemetryClient.TrackTrace("calcResultScaledupProducersBuilder end...");

            _telemetryClient.TrackTrace("calcResultPartialObligationBuilder started...");
            result.CalcResultPartialObligations = await calcResultPartialObligationBuilder.ConstructAsync(resultsRequestDto, result.CalcResultScaledupProducers?.ScaledupProducers ?? new List<CalcResultScaledupProducer>());
            _telemetryClient.TrackTrace("calcResultPartialObligationBuilder end...");

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