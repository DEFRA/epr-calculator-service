namespace EPR.Calculator.Service.Function.Services
{
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using EPR.Calculator.Service.Common;
    using EPR.Calculator.Service.Common.AzureSynapse;
    using EPR.Calculator.Service.Common.Utils;
    using EPR.Calculator.Service.Function.Dtos;
    using EPR.Calculator.Service.Function.Enums;
    using EPR.Calculator.Service.Function.Interface;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Implementing calculator run service methods.
    /// </summary>
    public class CalculatorRunService : ICalculatorRunService
    {
        private const string JsonMediaType = "application/json";
        private readonly IAzureSynapseRunner azureSynapseRunner;
        private readonly ILogger logger;
        private readonly IPipelineClientFactory pipelineClientFactory;
        private readonly ITransposePomAndOrgDataService transposePomAndOrgDataService;
        private readonly IConfigurationService configuration;
        private readonly IPrepareCalcService prepareCalcService;
        private readonly IRpdStatusService statusService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculatorRunService"/> class.
        /// </summary>
        /// <param name="azureSynapseRunner">The Azure Synapse runner.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="pipelineClientFactory">The pipeline client factory.</param>
        public CalculatorRunService(
            IAzureSynapseRunner azureSynapseRunner,
            ILogger<CalculatorRunService> logger,
            IPipelineClientFactory pipelineClientFactory,
            ITransposePomAndOrgDataService transposePomAndOrgDataService,
            IConfigurationService configuration,
            IPrepareCalcService prepareCalcService,
            IRpdStatusService statusService)
        {
            this.logger = logger;
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

        /// <summary>
        /// Starts the calculator process.
        /// </summary>
        /// <param name="calculatorRunParameter">The parameters required to run the calculator.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a boolean indicating success or failure.
        /// </returns>
        public async Task<bool> StartProcess(CalculatorRunParameter calculatorRunParameter)
        {
            this.logger.LogInformation("Process started");
            bool isPomSuccessful = await this.RunPipelines(calculatorRunParameter, this.configuration.ExecuteRPDPipeline);

            bool isSuccess;
            try
            {
                isSuccess = await this.UpdateStatusAndPrepareResult(calculatorRunParameter, isPomSuccessful);
            }
            catch (TaskCanceledException)
            {
                return false;
            }

            return isSuccess;
        }

        /// <summary>
        /// Executes the organization and POM pipelines based on the provided parameters.
        /// </summary>
        /// <param name="calculatorRunParameter">The parameters required to run the calculator.</param>
        /// <param name="runRpdPipeline">A boolean indicating whether the RPD pipeline should be executed.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a boolean indicating the success of the POM pipeline.
        /// </returns>
        private async Task<bool> RunPipelines(CalculatorRunParameter calculatorRunParameter, bool runRpdPipeline)
        {
            bool isPomSuccessful = false;

            if (runRpdPipeline)
            {
                var orgPipelineConfiguration = this.GetAzureSynapseConfiguration(
                    calculatorRunParameter,
                    this.configuration.OrgDataPipelineName);

                var isOrgSuccessful = await this.azureSynapseRunner.Process(orgPipelineConfiguration);
                this.logger.LogInformation("Org status: {Status}", isOrgSuccessful);

                if (isOrgSuccessful)
                {
                    var pomPipelineConfiguration = this.GetAzureSynapseConfiguration(
                        calculatorRunParameter,
                        this.configuration.PomDataPipelineName);
                    isPomSuccessful = await this.azureSynapseRunner.Process(pomPipelineConfiguration);
                    this.logger.LogInformation("Pom status: {Status}", isPomSuccessful);
                }
            }
            else
            {
                isPomSuccessful = true;
            }

            this.logger.LogInformation("Pom status: {Status}", isPomSuccessful);
            return isPomSuccessful;
        }

        /// <summary>
        /// Updates the status and prepares the result based on the success of the POM pipeline.
        /// </summary>
        /// <param name="calculatorRunParameter">The parameters required to run the calculator.</param>
        /// <param name="isPomSuccessful">A boolean indicating whether the POM pipeline was successful.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a boolean indicating the success of the status update and result preparation.
        /// </returns>
        private async Task<bool> UpdateStatusAndPrepareResult(CalculatorRunParameter calculatorRunParameter, bool isPomSuccessful)
        {
            bool isSuccess = false;

            if (isPomSuccessful)
            {
                this.logger.LogInformation("StatusEndPoint: {StatusEndPoint}", this.configuration.StatusEndpoint);
                var statusUpdateResponse = await this.statusService.UpdateRpdStatus(
                        calculatorRunParameter.Id,
                        calculatorRunParameter.User,
                        isPomSuccessful,
                        new CancellationTokenSource(this.configuration.RpdStatusTimeout).Token);
                this.logger.LogInformation("Status UpdateRpdStatus: {Response}", statusUpdateResponse);

                if (statusUpdateResponse == RunClassification.RUNNING)
                {
                    var isTransposeSuccess = await this.transposePomAndOrgDataService.
                        TransposeBeforeCalcResults(
                            new CalcResultsRequestDto { RunId = calculatorRunParameter.Id },
                            new CancellationTokenSource(this.configuration.TransposeTimeout).Token);

                    this.logger.LogInformation("transposeResultResponse: {isSuccess}", isTransposeSuccess);

                    if (isTransposeSuccess)
                    {
                        isSuccess = await this.prepareCalcService.PrepareCalcResults(
                            new CalcResultsRequestDto { RunId = calculatorRunParameter.Id },
                            new CancellationTokenSource(this.configuration.PrepareCalcResultsTimeout).Token);

                        this.logger.LogInformation("prepareCalcResultResponse: {isSuccess}", isSuccess);
                    }
                }

                this.logger.LogInformation("PrepareCalcResultEndPoint: {PrepareCalcResultEndPoint}", this.configuration.PrepareCalcResultEndPoint);
                this.logger.LogInformation("CalculatorRunParameter ID: {CalculatorRunParameterId}", calculatorRunParameter.Id);
                this.logger.LogInformation("GetPrepareCalcResultMessage: {GetPrepareCalcResultMessageId}", GetCalcResultMessage(calculatorRunParameter.Id));
            }
            else
            {
                this.logger.LogInformation("StatusEndPoint: {StatusEndPoint}", this.configuration.StatusEndpoint);
                var statusUpdateResponse = await this.statusService.UpdateRpdStatus(
                    calculatorRunParameter.Id,
                    calculatorRunParameter.User,
                    isPomSuccessful,
                    new CancellationTokenSource(this.configuration.RpdStatusTimeout).Token);
                this.logger.LogInformation("Status Response: {Response}", statusUpdateResponse);
            }

            return isSuccess;
        }
    }
}