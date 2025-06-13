namespace EPR.Calculator.Service.Function.Builder
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EPR.Calculator.Service.Function.Builder.CancelledProducers;
    using EPR.Calculator.Service.Function.Builder.CommsCost;
    using EPR.Calculator.Service.Function.Builder.Detail;
    using EPR.Calculator.Service.Function.Builder.LaDisposalCost;
    using EPR.Calculator.Service.Function.Builder.Lapcap;
    using EPR.Calculator.Service.Function.Builder.LateReportingTonnages;
    using EPR.Calculator.Service.Function.Builder.OnePlusFourApportionment;
    using EPR.Calculator.Service.Function.Builder.ParametersOther;
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
        public readonly ICalcResultCancelledProducersBuilder CalcResultCancelledProducersBuilder;
        private readonly TelemetryClient _telemetryClient;

#pragma warning restore S107 // Constructor has 9 parameters, which is greater than the 7 authorized
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
            this.CalcResultCancelledProducersBuilder = calcResultCancelledProducersBuilder;
            this._telemetryClient = telemetryClient;
        }
#pragma warning restore S107 // Constructor has 9 parameters, which is greater than the 7 authorized

        public async Task<CalcResult> Build(CalcResultsRequestDto resultsRequestDto)
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
            };
            this._telemetryClient.TrackTrace("calcResultDetailBuilder started...");
            result.CalcResultDetail = await this.calcResultDetailBuilder.Construct(resultsRequestDto);
            this._telemetryClient.TrackTrace("calcResultDetailBuilder end...");

            this._telemetryClient.TrackTrace("lapcapBuilder started...");
            result.CalcResultLapcapData = await this.lapcapBuilder.Construct(resultsRequestDto);
            this._telemetryClient.TrackTrace("lapcapBuilder end...");

            this._telemetryClient.TrackTrace("lateReportingBuilder started...");
            result.CalcResultLateReportingTonnageData = await this.lateReportingBuilder.Construct(resultsRequestDto);
            this._telemetryClient.TrackTrace("BuilateReportingBuilderlder end...");

            this._telemetryClient.TrackTrace("calcResultParameterOtherCostBuilder started...");
            result.CalcResultParameterOtherCost = await this.calcResultParameterOtherCostBuilder.Construct(resultsRequestDto);
            this._telemetryClient.TrackTrace("calcResultParameterOtherCostBuilder end...");

            this._telemetryClient.TrackTrace("lapcapplusFourApportionmentBuilder started...");
            result.CalcResultOnePlusFourApportionment = this.lapcapplusFourApportionmentBuilder.Construct(resultsRequestDto, result);
            this._telemetryClient.TrackTrace("lapcapplusFourApportionmentBuilder end...");

            this._telemetryClient.TrackTrace("CalcResultCancelledProducersBuilder started...");
            result.CalcResultCancelledProducers = await this.CalcResultCancelledProducersBuilder.Construct(resultsRequestDto);
            this._telemetryClient.TrackTrace("CalcResultCancelledProducersBuilder end...");

            this._telemetryClient.TrackTrace("calcResultScaledupProducersBuilder started...");
            result.CalcResultScaledupProducers = await this.calcResultScaledupProducersBuilder.Construct(resultsRequestDto);
            this._telemetryClient.TrackTrace("calcResultScaledupProducersBuilder end...");

            this._telemetryClient.TrackTrace("laDisposalCostBuilder started...");
            result.CalcResultLaDisposalCostData = await this.laDisposalCostBuilder.Construct(resultsRequestDto, result);
            this._telemetryClient.TrackTrace("laDisposalCostBuilder end...");

            this._telemetryClient.TrackTrace("summaryBuilder started...");
            result.CalcResultCommsCostReportDetail = await this.commsCostReportBuilder.Construct(
                resultsRequestDto, result.CalcResultOnePlusFourApportionment, result);
            result.CalcResultLaDisposalCostData = await this.laDisposalCostBuilder.Construct(resultsRequestDto, result);
            result.CalcResultSummary = await this.summaryBuilder.Construct(resultsRequestDto, result);
            this._telemetryClient.TrackTrace("summaryBuilder end...");


            return result;
        }
    }
}