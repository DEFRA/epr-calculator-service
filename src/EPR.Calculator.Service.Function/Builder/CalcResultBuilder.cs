namespace EPR.Calculator.Service.Function.Builder
{
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
    using EPR.Calculator.Service.Function.Builder.Summary;
    using EPR.Calculator.Service.Function.Dtos;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.ApplicationInsights;

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
        public readonly ICalcResultCancelledProducersBuilder calcResultCancelledProducersBuilder;
        public readonly ICalcResultRejectedProducersBuilder calcResultRejectedProducersBuilder;
        public readonly ICalcResultErrorReportBuilder calcResultErrorReportBuilder;
        private readonly TelemetryClient _telemetryClient;


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
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
            this.laDisposalCostBuilder = calcRunLaDisposalCostBuilder;
            this.lapcapplusFourApportionmentBuilder = calcResultOnePlusFourApportionmentBuilder;
            this.calcResultScaledupProducersBuilder = calcResultScaledupProducersBuilder;
            this.summaryBuilder = summaryBuilder;
            this.calcResultCancelledProducersBuilder = calcResultCancelledProducersBuilder;
            this.calcResultRejectedProducersBuilder = calcResultRejectedProducersBuilder;
            this.calcResultErrorReportBuilder = calcResultErrorReportBuilder;
            this._telemetryClient = telemetryClient;
        }

        public async Task<CalcResult> BuildAsync(CalcResultsRequestDto resultsRequestDto)
        {
            var result = new CalcResult
            {
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
                CalcResultScaledupProducers = new CalcResultScaledupProducers(),
                CalcResultCancelledProducers = new CalcResultCancelledProducersResponse(),
                CalcResultRejectedProducers = new List<CalcResultRejectedProducer>()
            };
            this._telemetryClient.TrackTrace("calcResultDetailBuilder started...");
            result.CalcResultDetail = await this.calcResultDetailBuilder.ConstructAsync(resultsRequestDto);
            this._telemetryClient.TrackTrace("calcResultDetailBuilder end...");

            this._telemetryClient.TrackTrace("lapcapBuilder started...");
            result.CalcResultLapcapData = await this.lapcapBuilder.ConstructAsync(resultsRequestDto);
            this._telemetryClient.TrackTrace("lapcapBuilder end...");

            this._telemetryClient.TrackTrace("lateReportingBuilder started...");
            result.CalcResultLateReportingTonnageData = await this.lateReportingBuilder.ConstructAsync(resultsRequestDto);
            this._telemetryClient.TrackTrace("BuilateReportingBuilderlder end...");

            this._telemetryClient.TrackTrace("calcResultParameterOtherCostBuilder started...");
            result.CalcResultParameterOtherCost = await this.calcResultParameterOtherCostBuilder.ConstructAsync(resultsRequestDto);
            this._telemetryClient.TrackTrace("calcResultParameterOtherCostBuilder end...");

            this._telemetryClient.TrackTrace("lapcapplusFourApportionmentBuilder started...");
            result.CalcResultOnePlusFourApportionment = this.lapcapplusFourApportionmentBuilder.ConstructAsync(resultsRequestDto, result);
            this._telemetryClient.TrackTrace("lapcapplusFourApportionmentBuilder end...");

            this._telemetryClient.TrackTrace("CalcResultCancelledProducersBuilder started...");
            result.CalcResultCancelledProducers = await this.calcResultCancelledProducersBuilder.ConstructAsync(resultsRequestDto, result.CalcResultDetail.FinancialYear);
            this._telemetryClient.TrackTrace("CalcResultCancelledProducersBuilder end...");

            this._telemetryClient.TrackTrace("calcResultScaledupProducersBuilder started...");
            result.CalcResultScaledupProducers = await this.calcResultScaledupProducersBuilder.ConstructAsync(resultsRequestDto);
            this._telemetryClient.TrackTrace("calcResultScaledupProducersBuilder end...");

            if (resultsRequestDto.IsBillingFile)
            {
                this._telemetryClient.TrackTrace("CalcResultRejectedProducersBuilder started...");
                result.CalcResultRejectedProducers = await this.calcResultRejectedProducersBuilder.ConstructAsync(resultsRequestDto);
                this._telemetryClient.TrackTrace("CalcResultRejectedProducersBuilder end...");
            }

            this._telemetryClient.TrackTrace("laDisposalCostBuilder started...");
            result.CalcResultLaDisposalCostData = await this.laDisposalCostBuilder.ConstructAsync(resultsRequestDto, result);
            this._telemetryClient.TrackTrace("laDisposalCostBuilder end...");

            this._telemetryClient.TrackTrace("commsCostReportBuilder started...");
            result.CalcResultCommsCostReportDetail = await this.commsCostReportBuilder.ConstructAsync(resultsRequestDto, result.CalcResultOnePlusFourApportionment, result);
            this._telemetryClient.TrackTrace("commsCostReportBuilder end...");

            this._telemetryClient.TrackTrace("summaryBuilder started...");
            result.CalcResultSummary = await this.summaryBuilder.ConstructAsync(resultsRequestDto, result);
            this._telemetryClient.TrackTrace("summaryBuilder end...");

            this._telemetryClient.TrackTrace("Error report builder started...");
            result.CalcResultErrorReports = await this.calcResultErrorReportBuilder.ConstructAsync(resultsRequestDto);
            this._telemetryClient.TrackTrace("Error report builder end...");

            return result;
        }
    }
}