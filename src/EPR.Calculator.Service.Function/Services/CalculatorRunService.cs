﻿namespace EPR.Calculator.Service.Function.Services
{
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using EPR.Calculator.Service.Common;
    using EPR.Calculator.Service.Common.AzureSynapse;
    using EPR.Calculator.Service.Common.Logging;
    using EPR.Calculator.Service.Common.Utils;
    using EPR.Calculator.Service.Function.Dtos;
    using EPR.Calculator.Service.Function.Enums;
    using EPR.Calculator.Service.Function.Interface;

    /// <summary>
    /// Implementing calculator run service methods.
    /// </summary>
    public class CalculatorRunService : ICalculatorRunService
    {
        private const string JsonMediaType = "application/json";
        private readonly IAzureSynapseRunner azureSynapseRunner;
        private readonly IPipelineClientFactory pipelineClientFactory;
        private readonly ITransposePomAndOrgDataService transposePomAndOrgDataService;
        private readonly IConfigurationService configuration;
        private readonly IPrepareCalcService prepareCalcService;
        private readonly IRpdStatusService statusService;
        private readonly ICalculatorTelemetryLogger telemetryLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculatorRunService"/> class.
        /// </summary>
        /// <param name="azureSynapseRunner">The Azure Synapse runner.</param>
        /// <param name="telemetryLogger">The logger instance.</param>
        /// <param name="pipelineClientFactory">The pipeline client factory.</param>
        /// <param name="transposePomAndOrgDataService">The service for transposing POM and organization data.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="prepareCalcService">The prepare calculator service.</param>
        /// <param name="statusService">The status service.</param>
        public CalculatorRunService(
            IAzureSynapseRunner azureSynapseRunner,
            ICalculatorTelemetryLogger telemetryLogger,
            IPipelineClientFactory pipelineClientFactory,
            ITransposePomAndOrgDataService transposePomAndOrgDataService,
            IConfigurationService configuration,
            IPrepareCalcService prepareCalcService,
            IRpdStatusService statusService)
        {
            this.telemetryLogger = telemetryLogger;
            this.azureSynapseRunner = azureSynapseRunner;
            this.pipelineClientFactory = pipelineClientFactory;
            this.transposePomAndOrgDataService = transposePomAndOrgDataService;
            this.configuration = configuration;
            this.prepareCalcService = prepareCalcService;
            this.statusService = statusService;
        }

        /// <summary>
        /// Builds the JSON content of the status update API call.
        /// </summary>
        /// <param name="calculatorRunId">The calculator run ID.</param>
        /// <param name="pipelineSucceeded">Indicates whether the pipeline succeeded.</param>
        /// <param name="user">Requested by user.</param>
        /// <returns>The JSON content for the status update.</returns>
        public static StringContent GetStatusUpdateMessage(int calculatorRunId, bool pipelineSucceeded, string user)
        {
            var statusUpdate = new
            {
                runId = calculatorRunId,
                updatedBy = user,
                isSuccessful = pipelineSucceeded,
            };

            return new StringContent(
                JsonSerializer.Serialize(statusUpdate),
                Encoding.UTF8,
                JsonMediaType);
        }

        /// <summary>
        /// Creates a JSON message containing the calculator run ID for preparing calculation results.
        /// </summary>
        /// <param name="calculatorRunId">The ID of the calculator run.</param>
        /// <returns>A <see cref="StringContent"/> object containing the JSON message.</returns>
        public static StringContent GetCalcResultMessage(int calculatorRunId)
        {
            var calcResultsRequest = new
            {
                runId = calculatorRunId,
            };

            return new StringContent(
                JsonSerializer.Serialize(calcResultsRequest),
                Encoding.UTF8,
                JsonMediaType);
        }

        /// <summary>
        /// Gets the Azure Synapse configuration.
        /// </summary>
        /// <param name="args">The calculator run parameters.</param>
        /// <param name="pipelineName">The name of the pipeline.</param>
        /// <returns>The Azure Synapse runner parameters.</returns>
        public AzureSynapseRunnerParameters GetAzureSynapseConfiguration(
            CalculatorRunParameter args,
            string pipelineName)
        {
            var financialYear = args.FinancialYear;

            return new AzureSynapseRunnerParameters
            {
                PipelineUrl = new Uri(this.configuration.PipelineUrl),
                CheckInterval = this.configuration.CheckInterval,
                MaxCheckCount = this.configuration.MaxCheckCount,
                PipelineName = pipelineName,
                CalculatorRunId = args.Id,
                CalendarYear = Util.GetCalendarYearFromFinancialYear(financialYear),
            };
        }

        public async Task<bool> StartProcess(CalculatorRunParameter calculatorRunParameter, string runName)
        {
            this.telemetryLogger.LogInformation(new TrackMessage
            {
                RunId = calculatorRunParameter.Id,
                RunName = runName,
                Message = "Process started",
            });

            bool runRpdPipeline = this.configuration.ExecuteRPDPipeline;
            bool isPomSuccessful = await this.RunPipelines(calculatorRunParameter, runRpdPipeline, runName);

            using var client = this.pipelineClientFactory.GetHttpClient(this.configuration.StatusEndpoint);
            this.telemetryLogger.LogInformation(new TrackMessage
            {
                RunId = calculatorRunParameter.Id,
                RunName = runName,
                Message = $"HTTP Client: {client}",
            });

            bool isSuccess;
            try
            {
                isSuccess = await this.UpdateStatusAndPrepareResult(calculatorRunParameter, isPomSuccessful, client, runName);
            }
            catch (TaskCanceledException ex)
            {
                this.LogError(calculatorRunParameter.Id, runName, "StartProcess - Task was canceled", ex);
                return false;
            }
            catch (Exception ex)
            {
                this.LogError(calculatorRunParameter.Id, runName, "StartProcess - An error occurred", ex);
                return false;
            }

            return isSuccess;
        }

        private async Task<bool> RunPipelines(CalculatorRunParameter calculatorRunParameter, bool runRpdPipeline, string runName)
        {
            bool isPomSuccessful = false;

            if (runRpdPipeline)
            {
                var orgPipelineConfiguration = this.GetAzureSynapseConfiguration(
                    calculatorRunParameter,
                    this.configuration.OrgDataPipelineName);

                var isOrgSuccessful = await this.azureSynapseRunner.Process(orgPipelineConfiguration);
                this.LogInformation(calculatorRunParameter.Id, runName, $"RunPipelines - Org status: {isOrgSuccessful}");

                if (isOrgSuccessful)
                {
                    var pomPipelineConfiguration = this.GetAzureSynapseConfiguration(
                        calculatorRunParameter,
                        this.configuration.PomDataPipelineName);
                    isPomSuccessful = await this.azureSynapseRunner.Process(pomPipelineConfiguration);
                    this.LogInformation(calculatorRunParameter.Id, runName, $"RunPipelines - POM status: {isPomSuccessful}");
                }
            }
            else
            {
                isPomSuccessful = true;
            }

            this.LogInformation(calculatorRunParameter.Id, runName, $"RunPipelines - Final POM status: {isPomSuccessful}");
            return isPomSuccessful;
        }

        private async Task<bool> UpdateStatusAndPrepareResult(CalculatorRunParameter calculatorRunParameter, bool isPomSuccessful, HttpClient client, string runName)
        {
            bool isSuccess = false;

            if (isPomSuccessful)
            {
                await LogAndUpdateStatus(calculatorRunParameter, runName, isPomSuccessful);

                var statusUpdateResponse = await this.statusService.UpdateRpdStatus(
                    calculatorRunParameter.Id,
                    runName,
                    calculatorRunParameter.User,
                    isPomSuccessful,
                    new CancellationTokenSource(this.configuration.RpdStatusTimeout).Token);

                this.LogInformation(calculatorRunParameter.Id, runName, $"UpdateStatusAndPrepareResult - Status UpdateRpdStatus: {statusUpdateResponse}");

                if (statusUpdateResponse == RunClassification.RUNNING)
                {
                    var isTransposeSuccess = await this.transposePomAndOrgDataService.TransposeBeforeCalcResults(
                        new CalcResultsRequestDto { RunId = calculatorRunParameter.Id },
                        runName,
                        new CancellationTokenSource(this.configuration.TransposeTimeout).Token);

                    this.LogInformation(calculatorRunParameter.Id, runName, $"UpdateStatusAndPrepareResult - transposeResultResponse: {isTransposeSuccess}");

                    if (isTransposeSuccess)
                    {
                        isSuccess = await this.prepareCalcService.PrepareCalcResults(
                            new CalcResultsRequestDto { RunId = calculatorRunParameter.Id },
                            runName,
                            new CancellationTokenSource(this.configuration.PrepareCalcResultsTimeout).Token);

                        this.LogInformation(calculatorRunParameter.Id, runName, $"UpdateStatusAndPrepareResult - prepareCalcResultResponse: {isSuccess}");
                    }
                }

                this.LogInformation(calculatorRunParameter.Id, runName, $"UpdateStatusAndPrepareResult - StatusEndPoint: {this.configuration.PrepareCalcResultEndPoint}");
                this.LogInformation(calculatorRunParameter.Id, runName, $"UpdateStatusAndPrepareResult - CalculatorRunParameter ID: {calculatorRunParameter.Id}");
                this.LogInformation(calculatorRunParameter.Id, runName, $"UpdateStatusAndPrepareResult - GetPrepareCalcResultMessage: {GetCalcResultMessage(calculatorRunParameter.Id)}");
            }
            else
            {
                await LogAndUpdateStatus(calculatorRunParameter, runName, isPomSuccessful);
            }

            return isSuccess;
        }

        private async Task LogAndUpdateStatus(CalculatorRunParameter calculatorRunParameter, string runName, bool isPomSuccessful)
        {
            this.LogInformation(calculatorRunParameter.Id, runName, $"UpdateStatusAndPrepareResult - StatusEndPoint: {this.configuration.StatusEndpoint}");
            var statusUpdateResponse = await this.statusService.UpdateRpdStatus(
                calculatorRunParameter.Id,
                runName,
                calculatorRunParameter.User,
                isPomSuccessful,
                new CancellationTokenSource(this.configuration.RpdStatusTimeout).Token);
            this.LogInformation(calculatorRunParameter.Id, runName, $"UpdateStatusAndPrepareResult - Status Response: {statusUpdateResponse}");
        }

        private void LogInformation(int runId, string runName, string message)
        {
            this.telemetryLogger.LogInformation(new TrackMessage
            {
                RunId = runId,
                RunName = runName,
                Message = message,
            });
        }

        private void LogError(int runId, string runName, string message, Exception ex)
        {
            this.telemetryLogger.LogError(new ErrorMessage
            {
                RunId = runId,
                RunName = runName,
                Message = message,
                Exception = ex,
            });
        }
    }
}