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
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using Microsoft.ApplicationInsights;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Service.Function.Builder
{
    public class CalcResultBuilder : ICalcResultBuilder
    {
        private readonly IParameterService parameterService;
        private readonly IMaterialService materialService;
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
        public readonly ISelfManagedConsumerWasteService selfManagedConsumerWasteService;
        private readonly ICalcResultModulationBuilder modulationBuilder;
        public readonly IReportedProducerService reportedProducerService;
        private readonly TelemetryClient telemetryClient;


        [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
        public CalcResultBuilder(
            IParameterService parameterService,
            IMaterialService materialService,
            ICalcResultDetailBuilder calcResultDetailBuilder,
            ICalcResultLapcapDataBuilder lapcapBuilder,
            ICalcResultParameterOtherCostBuilder calcResultParameterOtherCostBuilder,
            ICalcResultOnePlusFourApportionmentBuilder calcResultOnePlusFourApportionmentBuilder,
            ICalcResultCommsCostBuilder commsCostReportBuilder,
            ICalcResultLateReportingBuilder lateReportingBuilder,
            ICalcRunLaDisposalCostBuilder calcRunLaDisposalCostBuilder,
            ICalcResultScaledupProducersBuilder calcResultScaledupProducersBuilder,
            ICalcResultPartialObligationBuilder calcResultPartialObligationBuilder,
            ICalcResultProjectedProducersBuilder calcResultProjectedProducersBuilder,
            ICalcResultSummaryBuilder summaryBuilder,
            ICalcResultCancelledProducersBuilder calcResultCancelledProducersBuilder,
            ICalcResultRejectedProducersBuilder calcResultRejectedProducersBuilder,
            ICalcResultErrorReportBuilder calcResultErrorReportBuilder,
            IProjectedProducersService projectedProducersService,
            ISelfManagedConsumerWasteService selfManagedConsumerWasteService,
            ICalcResultModulationBuilder modulationBuilder,
            IReportedProducerService reportedProducerService,
            TelemetryClient telemetryClient)
        {
            this.parameterService = parameterService;
            this.materialService = materialService;
            this.calcResultDetailBuilder = calcResultDetailBuilder;
            this.lapcapBuilder = lapcapBuilder;
            this.commsCostReportBuilder = commsCostReportBuilder;
            this.lateReportingBuilder = lateReportingBuilder;
            this.calcResultParameterOtherCostBuilder = calcResultParameterOtherCostBuilder;
            this.laDisposalCostBuilder = calcRunLaDisposalCostBuilder;
            this.lapcapplusFourApportionmentBuilder = calcResultOnePlusFourApportionmentBuilder;
            this.calcResultScaledupProducersBuilder = calcResultScaledupProducersBuilder;
            this.calcResultProjectedProducersBuilder = calcResultProjectedProducersBuilder;
            this.calcResultPartialObligationBuilder = calcResultPartialObligationBuilder;
            this.summaryBuilder = summaryBuilder;
            this.calcResultCancelledProducersBuilder = calcResultCancelledProducersBuilder;
            this.calcResultRejectedProducersBuilder = calcResultRejectedProducersBuilder;
            this.calcResultErrorReportBuilder = calcResultErrorReportBuilder;
            this.projectedProducersService = projectedProducersService;
            this.selfManagedConsumerWasteService = selfManagedConsumerWasteService;
            this.modulationBuilder = modulationBuilder;
            this.reportedProducerService = reportedProducerService;
            this.telemetryClient = telemetryClient;
        }

        public async Task<CalcResult> BuildAsync(CalcResultsRequestDto resultsRequestDto)
        {
            var result = new CalcResult
            {
                ApplyModulation = resultsRequestDto.RelativeYear.Value >= 2026,
                CalcResultDetail = await calcResultDetailBuilder.ConstructAsync(resultsRequestDto),
                CalcResultLapcapData = new CalcResultLapcapData
                {
                    CalcResultLapcapDataDetails = new List<CalcResultLapcapDataDetail>(),
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

            var defaultParams =
                await parameterService.GetDefaultParameters(resultsRequestDto.RunId);

            telemetryClient.TrackTrace("materialService started...");
            var materialDetails = await materialService.GetMaterials();
            telemetryClient.TrackTrace("materialService started...");

            telemetryClient.TrackTrace("lapcapBuilder started...");
            result.CalcResultLapcapData = await lapcapBuilder.ConstructAsync(materialDetails, resultsRequestDto);
            telemetryClient.TrackTrace("lapcapBuilder end...");

            telemetryClient.TrackTrace("lateReportingBuilder started...");
            result.CalcResultLateReportingTonnageData = await lateReportingBuilder.ConstructAsync(resultsRequestDto);
            telemetryClient.TrackTrace("lateReportingBuilder end...");

            telemetryClient.TrackTrace("calcResultParameterOtherCostBuilder started...");
            result.CalcResultParameterOtherCost = await calcResultParameterOtherCostBuilder.ConstructAsync(resultsRequestDto);
            telemetryClient.TrackTrace("calcResultParameterOtherCostBuilder end...");

            telemetryClient.TrackTrace("lapcapplusFourApportionmentBuilder started...");
            result.CalcResultOnePlusFourApportionment = lapcapplusFourApportionmentBuilder.ConstructAsync(resultsRequestDto, result);
            telemetryClient.TrackTrace("lapcapplusFourApportionmentBuilder end...");

            telemetryClient.TrackTrace("calcResultCancelledProducersBuilder started...");
            result.CalcResultCancelledProducers = await calcResultCancelledProducersBuilder.ConstructAsync(materialDetails, resultsRequestDto);
            telemetryClient.TrackTrace("calcResultCancelledProducersBuilder end...");

            List<ProducerDetail> producers1 = await reportedProducerService.GetProducers(resultsRequestDto.RunId);

            List<ProducerDetail> producers2;
            if (result.ApplyModulation)
            {
                telemetryClient.TrackTrace("calcResultProjectedProducerBuilder started...");
                var calcResultProjectedProducers = await calcResultProjectedProducersBuilder.ConstructAsync(materialDetails, producers1, resultsRequestDto);
                producers2 = calcResultProjectedProducers.Item1;
                result.CalcResultProjectedProducers = calcResultProjectedProducers.Item2;
                telemetryClient.TrackTrace("calcResultProjectedProducerBuilder end...");
            } else
            {
                producers2 = producers1;
            }

            List<ProducerDetail> producers3;
            if (resultsRequestDto.RelativeYear.Value == 2025)
            {
                telemetryClient.TrackTrace("calcResultScaledupProducersBuilder started...");
                var scaledupProducersResult = await calcResultScaledupProducersBuilder.ConstructAsync(materialDetails, producers2, resultsRequestDto);
                result.CalcResultScaledupProducers = scaledupProducersResult.Item2;
                producers3 = scaledupProducersResult.Item1;
                telemetryClient.TrackTrace("calcResultScaledupProducersBuilder end...");
            } else
            {
                producers3 = producers2;
            }

            telemetryClient.TrackTrace("calcResultPartialObligationBuilder started...");
            var partialObligationsResult = await calcResultPartialObligationBuilder.ConstructAsync(materialDetails, producers3, resultsRequestDto, result.ApplyModulation);
            result.CalcResultPartialObligations = partialObligationsResult.Item2;
            List<ProducerDetail> producers4 = partialObligationsResult.Item1;
            telemetryClient.TrackTrace("calcResultPartialObligationBuilder end...");

            if (!resultsRequestDto.IsBillingFile)
            {
                telemetryClient.TrackTrace("Storing projected producers started...");
                await projectedProducersService.StoreProjectedProducers(
                    resultsRequestDto.RunId,
                    producers4
                );
                telemetryClient.TrackTrace("Storing projected producers ended...");
            }

            if (resultsRequestDto.IsBillingFile)
            {
                telemetryClient.TrackTrace("CalcResultRejectedProducersBuilder started...");
                result.CalcResultRejectedProducers = await calcResultRejectedProducersBuilder.ConstructAsync(resultsRequestDto);
                telemetryClient.TrackTrace("CalcResultRejectedProducersBuilder end...");
            }

            telemetryClient.TrackTrace("selfManagedConsumerWasteService started...");
            var smcw = await selfManagedConsumerWasteService.Calculate(resultsRequestDto, materialDetails, result.ApplyModulation);
            result.Smcw = smcw;
            telemetryClient.TrackTrace("selfManagedConsumerWasteService ended...");

            telemetryClient.TrackTrace("laDisposalCostBuilder started...");
            result.CalcResultLaDisposalCostData = await laDisposalCostBuilder.ConstructAsync(resultsRequestDto, materialDetails, result.CalcResultLapcapData, result.CalcResultLateReportingTonnageData, smcw, result.ApplyModulation);
            telemetryClient.TrackTrace("laDisposalCostBuilder end...");

            telemetryClient.TrackTrace("commsCostReportBuilder started...");
            result.CalcResultCommsCostReportDetail = await commsCostReportBuilder.ConstructAsync(materialDetails, resultsRequestDto, result.CalcResultOnePlusFourApportionment, result.CalcResultLateReportingTonnageData);
            telemetryClient.TrackTrace("commsCostReportBuilder end...");

            if (result.ApplyModulation)
            {
                telemetryClient.TrackTrace("modulationBuilder started...");
                result.CalcResultModulation = await modulationBuilder.ConstructAsync(defaultParams, materialDetails, result.CalcResultLaDisposalCostData, smcw);
                telemetryClient.TrackTrace("modulationBuilder end...");
            }

            telemetryClient.TrackTrace("summaryBuilder started...");
            result.CalcResultSummary = await summaryBuilder.ConstructAsync(materialDetails, resultsRequestDto.RunId, resultsRequestDto.RelativeYear, resultsRequestDto.IsBillingFile, result, smcw);
            telemetryClient.TrackTrace("summaryBuilder end...");

            telemetryClient.TrackTrace("Error report builder started...");
            result.CalcResultErrorReports = calcResultErrorReportBuilder.ConstructAsync(resultsRequestDto);
            telemetryClient.TrackTrace("Error report builder end...");

            return result;
        }
    }
}
