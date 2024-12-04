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
            return new AzureSynapseRunnerParameters
            {
                PipelineUrl = new Uri(Configuration.PipelineUrl),
                CheckInterval = int.Parse(Configuration.CheckInterval),
                MaxCheckCount = int.Parse(Configuration.MaxCheckCount),
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
        /// <returns>The JSON content for the status update.</returns>
        public static StringContent GetStatusUpdateMessage(int calculatorRunId, bool pipelineSucceeded)
        {
            var statusUpdate = new
            {
                runId = calculatorRunId,
                updatedBy = "string",
                isSuccessful = pipelineSucceeded,
            };

            return new StringContent(
                JsonSerializer.Serialize(statusUpdate),
                Encoding.UTF8,
                "application/json");
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
                "application/json");
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
            bool isPomSuccessful = false;
            bool runRpdPipeline = bool.Parse(Configuration.ExecuteRPDPipeline);
            var response = new HttpResponseMessage(HttpStatusCode.Continue);
            bool isSuccess = response.IsSuccessStatusCode;

            if (runRpdPipeline)
            {
                var orgPipelineConfiguration = GetAzureSynapseConfiguration(
                    calculatorRunParameter,
                    Configuration.OrgDataPipelineName);

                var isOrgSuccessful = await this.azureSynapseRunner.Process(orgPipelineConfiguration);

                this.logger.LogInformation("Org status: {Status}", Convert.ToString(isOrgSuccessful));

                if (isOrgSuccessful)
                {
                    var pomPipelineConfiguration = GetAzureSynapseConfiguration(
                        calculatorRunParameter,
                        Configuration.PomDataPipelineName);
                    isPomSuccessful = await this.azureSynapseRunner.Process(pomPipelineConfiguration);

                    this.logger.LogInformation("Pom status: {Status}", Convert.ToString(isPomSuccessful));
                }
            }
            else
            {
                isPomSuccessful = true;
            }

            this.logger.LogInformation("Pom status: {Status}", Convert.ToString(isPomSuccessful));

            using var client = this.pipelineClientFactory.GetHttpClient(Configuration.StatusEndpoint);

            this.logger.LogInformation("HTTP Client: {client}", client);

            if (isPomSuccessful)
            {
                this.logger.LogInformation("StatusEndPoint: {StatusEndPoint}", Configuration.StatusEndpoint);
                var statusUpdateResponse = await client.PostAsync(
                    Configuration.StatusEndpoint,
                    GetStatusUpdateMessage(calculatorRunParameter.Id, isPomSuccessful));
                this.logger.LogInformation("Status Response: {Response}", statusUpdateResponse);

                if (statusUpdateResponse != null && statusUpdateResponse.IsSuccessStatusCode)
                {
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
                    GetStatusUpdateMessage(calculatorRunParameter.Id, isPomSuccessful));
                this.logger.LogInformation("Status Response: {Response}", statusUpdateResponse);
            }

            return isSuccess;
        }
    }
}