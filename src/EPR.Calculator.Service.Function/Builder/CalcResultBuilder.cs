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
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;
using Microsoft.ApplicationInsights;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.Misc;
using static EPR.Calculator.Service.Function.Builder.LaDisposalCost.CalcRunLaDisposalCostBuilder;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.Service.Function.Builder.Modulation;
using EPR.Calculator.API.Data;
using Microsoft.EntityFrameworkCore;
using EPR.Calculator.API.Data.Enums;

namespace EPR.Calculator.Service.Function.Builder
{
    class Parameter
    {
        public required string ReferenceId;
        public required decimal Value;
    }

    public class ProducerData
    {
        public required string MaterialName { get; set; }
        public required decimal TonnageRed { get; set; }
        public required decimal TonnageRedMedical { get; set; }
        public required decimal TonnageAmber { get; set; }
        public required decimal TonnageAmberMedical { get; set; }
        public required decimal TonnageGreen { get; set; }
        public required decimal TonnageGreenMedical { get; set; }
        public decimal Tonnage { get; set; }
    }


    public class CalcResultBuilder : ICalcResultBuilder
    {
        private readonly ApplicationDBContext dbContext;
        private readonly IParameterService parameterService;
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
        private readonly ICalcResultModulationBuilder modulationBuilder;
        private readonly TelemetryClient _telemetryClient;


        [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
        public CalcResultBuilder(
            ApplicationDBContext dbContext,
            IParameterService parameterService,
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
            ICalcResultModulationBuilder modulationBuilder,
            TelemetryClient telemetryClient)
        {
            this.dbContext = dbContext;
            this.parameterService = parameterService;
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
            this.modulationBuilder = modulationBuilder;
            _telemetryClient = telemetryClient;
        }

        public async Task<CalcResult> BuildAsync(CalcResultsRequestDto resultsRequestDto)
        {
            var result = new CalcResult
            {
                CalcResultDetail = await calcResultDetailBuilder.ConstructAsync(resultsRequestDto),
                CalcResultLapcapData = new CalcResultLapcapData
                {
                    CalcResultLapcapDataDetails = new List<CalcResultLapcapDataDetails>()
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

                ApplyModulation = resultsRequestDto.RelativeYear.Value >= 2026
            };

            // TODO pass to other builders that require default params
            var defaultParams =
                await parameterService.GetDefaultParameters(resultsRequestDto.RunId);

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

            if (result.ApplyModulation)
            {
                _telemetryClient.TrackTrace("calcResultProjectedProducerBuilder started...");
                var calcResultProjectedProducers = await calcResultProjectedProducersBuilder.ConstructAsync(resultsRequestDto);
                result.CalcResultProjectedProducers = calcResultProjectedProducers;
                _telemetryClient.TrackTrace("calcResultProjectedProducerBuilder end...");

                if(!resultsRequestDto.IsBillingFile)
                {
                    _telemetryClient.TrackTrace("Storing projected producers started...");
                    await projectedProducersService.StoreProjectedProducers(
                        resultsRequestDto.RunId,
                        calcResultProjectedProducers.H2ProjectedProducers?.ToList() ?? new List<CalcResultH2ProjectedProducer>(),
                        calcResultProjectedProducers.H1ProjectedProducers?.ToList() ?? new List<CalcResultH1ProjectedProducer>()
                    );
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

            _telemetryClient.TrackTrace("modulationBuilder started...");
            result.CalcResultModulation = await modulationBuilder.ConstructAsync(result.CalcResultLaDisposalCostData, defaultParams);
            _telemetryClient.TrackTrace("modulationBuilder end...");

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
