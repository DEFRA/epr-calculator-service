namespace EPR.Calculator.Service.Function.Services
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using EPR.Calculator.Service.Common;
    using EPR.Calculator.Service.Common.AzureSynapse;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculatorRunService"/> class.
        /// </summary>
        /// <param name="azureSynapseRunner">The Azure Synapse runner.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="pipelineClientFactory">The pipeline client factory.</param>
        public CalculatorRunService(IAzureSynapseRunner azureSynapseRunner, ILogger<CalculatorRunService> logger, IPipelineClientFactory pipelineClientFactory)
        {
            this.logger = logger;
            this.azureSynapseRunner = azureSynapseRunner;
            this.pipelineClientFactory = pipelineClientFactory;
        }

        /// <summary>
        /// Gets the Azure Synapse configuration.
        /// </summary>
        /// <param name="args">The calculator run parameters.</param>
        /// <param name="pipelineName">The name of the pipeline.</param>
        /// <returns>The Azure Synapse runner parameters.</returns>
        public static AzureSynapseRunnerParameters GetAzureSynapseConfiguration(
            CalculatorRunParameter args,
            string pipelineName)
        {
            var financialYear = args.FinancialYear;
            int.TryParse(Configuration.CheckInterval, out int checkInterval);

            int.TryParse(Configuration.MaxCheckCount, out int maxCheckCount);

            return new AzureSynapseRunnerParameters
            {
                PipelineUrl = new Uri(Configuration.PipelineUrl),
                CheckInterval = checkInterval,
                MaxCheckCount = maxCheckCount,
                PipelineName = pipelineName,
                CalculatorRunId = args.Id,
                FinancialYear = Common.Utils.Util.GetCalendarYearFromFinancialYear(financialYear),
            };
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
        public static StringContent GetPrepareCalcResultMessage(int calculatorRunId)
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
        /// Starts the calculator process.
        /// </summary>
        /// <param name="calculatorRunParameter">The parameters required to run the calculator.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a boolean indicating success or failure.
        /// </returns>
        public async Task<bool> StartProcess(CalculatorRunParameter calculatorRunParameter)
        {
            this.logger.LogInformation("Process started");
            bool.TryParse(Configuration.ExecuteRPDPipeline, out bool runRpdPipeline);

            bool isPomSuccessful = await this.RunPipelines(calculatorRunParameter, runRpdPipeline);

            using var client = this.pipelineClientFactory.GetHttpClient(Configuration.StatusEndpoint);
            this.logger.LogInformation("HTTP Client: {client}", client);

            bool isSuccess;
            try
            {
                isSuccess = await this.UpdateStatusAndPrepareResult(calculatorRunParameter, isPomSuccessful, client);
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
                var orgPipelineConfiguration = GetAzureSynapseConfiguration(
                    calculatorRunParameter,
                    Configuration.OrgDataPipelineName);

                var isOrgSuccessful = await this.azureSynapseRunner.Process(orgPipelineConfiguration);
                this.logger.LogInformation("Org status: {Status}", isOrgSuccessful);

                if (isOrgSuccessful)
                {
                    var pomPipelineConfiguration = GetAzureSynapseConfiguration(
                        calculatorRunParameter,
                        Configuration.PomDataPipelineName);
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
        /// <param name="client">The HTTP client used to send status updates and prepare result requests.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a boolean indicating the success of the status update and result preparation.
        /// </returns>
        private async Task<bool> UpdateStatusAndPrepareResult(CalculatorRunParameter calculatorRunParameter, bool isPomSuccessful, HttpClient client)
        {
            bool isSuccess = false;

            if (isPomSuccessful)
            {
                this.logger.LogInformation("StatusEndPoint: {StatusEndPoint}", Configuration.StatusEndpoint);
                var statusUpdateResponse = await client.PostAsync(
                    Configuration.StatusEndpoint,
                    GetStatusUpdateMessage(calculatorRunParameter.Id, isPomSuccessful, calculatorRunParameter.User));
                this.logger.LogInformation("Status Response: {Response}", statusUpdateResponse);

                if (statusUpdateResponse != null && statusUpdateResponse.IsSuccessStatusCode)
                {
                    client.Timeout = Configuration.CalculatorRunTimeout;
                    var prepareCalcResultResponse = await client.PostAsync(
                        Configuration.PrepareCalcResultEndPoint,
                        GetPrepareCalcResultMessage(calculatorRunParameter.Id));
                    isSuccess = prepareCalcResultResponse.IsSuccessStatusCode;
                    this.logger.LogInformation("prepareCalcResultResponse: {isSuccess}", prepareCalcResultResponse.IsSuccessStatusCode);
                }

                this.logger.LogInformation("PrepareCalcResultEndPoint: {PrepareCalcResultEndPoint}", Configuration.PrepareCalcResultEndPoint);
                this.logger.LogInformation("CalculatorRunParameter ID: {CalculatorRunParameterId}", calculatorRunParameter.Id);
                this.logger.LogInformation("GetPrepareCalcResultMessage: {GetPrepareCalcResultMessageId}", GetPrepareCalcResultMessage(calculatorRunParameter.Id));
            }
            else
            {
                this.logger.LogInformation("StatusEndPoint: {StatusEndPoint}", Configuration.StatusEndpoint);
                var statusUpdateResponse = await client.PostAsync(
                    Configuration.StatusEndpoint,
                    GetStatusUpdateMessage(calculatorRunParameter.Id, isPomSuccessful, calculatorRunParameter.User));
                this.logger.LogInformation("Status Response: {Response}", statusUpdateResponse);
            }

            return isSuccess;
        }
    }
}