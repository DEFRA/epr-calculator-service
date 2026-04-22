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
        public readonly IMaterialService _materialService;
        public readonly IProjectedProducersService _projectedProducersService;
        public readonly ISelfManagedConsumerWasteService _selfManagedConsumerWasteService;
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
            IMaterialService materialService,
            IProjectedProducersService projectedProducersService,
            ISelfManagedConsumerWasteService selfManagedConsumerWasteService,
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
            _materialService = materialService;
            _projectedProducersService = projectedProducersService;
            _selfManagedConsumerWasteService = selfManagedConsumerWasteService;
            _telemetryClient = telemetryClient;
        }

        public async Task<CalcResult> BuildAsync(CalcResultsRequestDto resultsRequestDto)
        {
            var result = new CalcResult
            {
                ShowModulations = resultsRequestDto.RelativeYear.Value >= 2026,
                CalcResultDetail =  await calcResultDetailBuilder.ConstructAsync(resultsRequestDto),
                CalcResultLapcapData =
                new CalcResultLapcapData
                {
                    CalcResultLapcapDataDetail = new List<CalcResultLapcapDataDetail>(),
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
            };

            _telemetryClient.TrackTrace("materialService started...");
            var materialDetails = await _materialService.GetMaterials();
            _telemetryClient.TrackTrace("materialService started...");

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

            _telemetryClient.TrackTrace("calcResultCancelledProducersBuilder started...");
            result.CalcResultCancelledProducers = await calcResultCancelledProducersBuilder.ConstructAsync(resultsRequestDto);
            _telemetryClient.TrackTrace("calcResultCancelledProducersBuilder end...");

            if (result.ShowModulations)
            {
                    _telemetryClient.TrackTrace("calcResultProjectedProducerBuilder started...");
                var calcResultProjectedProducers = await calcResultProjectedProducersBuilder.ConstructAsync(resultsRequestDto);
                result.CalcResultProjectedProducers = calcResultProjectedProducers;
                _telemetryClient.TrackTrace("calcResultProjectedProducerBuilder end...");

                if(!resultsRequestDto.IsBillingFile)
                {
                    _telemetryClient.TrackTrace("Storing projected producers started...");
                    await _projectedProducersService.StoreProjectedProducers(resultsRequestDto.RunId, calcResultProjectedProducers.H2ProjectedProducers?.ToList() ?? new List<CalcResultH2ProjectedProducer>());
                    _telemetryClient.TrackTrace("Storing projected producers ended...");
                }
            } else
            {
                _telemetryClient.TrackTrace("calcResultScaledupProducersBuilder started...");
                var scaledupProducersResult = await calcResultScaledupProducersBuilder.ConstructAsync(resultsRequestDto);
                result.CalcResultScaledupProducers = scaledupProducersResult;
                _telemetryClient.TrackTrace("calcResultScaledupProducersBuilder end...");
            }

            _telemetryClient.TrackTrace("calcResultPartialObligationBuilder started...");
            result.CalcResultPartialObligations = await calcResultPartialObligationBuilder.ConstructAsync(resultsRequestDto, result.CalcResultScaledupProducers?.ScaledupProducers ?? new List<CalcResultScaledupProducer>());
            _telemetryClient.TrackTrace("calcResultPartialObligationBuilder end...");

            var scaledUpProducers = result.CalcResultScaledupProducers?.ScaledupProducers ?? [];
            var partialObligations = result.CalcResultPartialObligations?.PartialObligations ?? [];

            _telemetryClient.TrackTrace("selfManagedConsumerWasteService started...");
            var smcw = await _selfManagedConsumerWasteService.Calculate(resultsRequestDto, materialDetails, scaledUpProducers, partialObligations, result.ShowModulations);
            _telemetryClient.TrackTrace("selfManagedConsumerWasteService ended...");

            if (resultsRequestDto.IsBillingFile)
            {
                _telemetryClient.TrackTrace("CalcResultRejectedProducersBuilder started...");
                result.CalcResultRejectedProducers = await calcResultRejectedProducersBuilder.ConstructAsync(resultsRequestDto);
                _telemetryClient.TrackTrace("CalcResultRejectedProducersBuilder end...");
            }

            _telemetryClient.TrackTrace("laDisposalCostBuilder started...");
            result.CalcResultLaDisposalCostData = await laDisposalCostBuilder.ConstructAsync(resultsRequestDto, result, smcw);
            _telemetryClient.TrackTrace("laDisposalCostBuilder end...");

            _telemetryClient.TrackTrace("commsCostReportBuilder started...");
            result.CalcResultCommsCostReportDetail = await commsCostReportBuilder.ConstructAsync(resultsRequestDto, result.CalcResultOnePlusFourApportionment, result);
            _telemetryClient.TrackTrace("commsCostReportBuilder end...");

            _telemetryClient.TrackTrace("summaryBuilder started...");
            result.CalcResultSummary = await summaryBuilder.ConstructAsync(resultsRequestDto.RunId, resultsRequestDto.RelativeYear, resultsRequestDto.IsBillingFile, result, smcw);
            _telemetryClient.TrackTrace("summaryBuilder end...");

            _telemetryClient.TrackTrace("Error report builder started...");
            result.CalcResultErrorReports = calcResultErrorReportBuilder.ConstructAsync(resultsRequestDto);
            _telemetryClient.TrackTrace("Error report builder end...");

            return result;
        }
    }
}