namespace EPR.Calculator.Service.Function.Services
{
    using System;
    using System.Diagnostics;
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

            using var client = this.pipelineClientFactory.GetStatusUpdateClient(Configuration.StatusEndpoint);
            var statusUpdateResponse = await client.PostAsync(
                    Configuration.StatusEndpoint,
                    GetStatusUpdateMessage(calculatorRunParameter.Id, isPomSuccessful));

#if DEBUG
            Debug.WriteLine(statusUpdateResponse.Content.ReadAsStringAsync().Result);
#endif
            this.logger.LogInformation("Status Response: {Response}", statusUpdateResponse);
            return statusUpdateResponse.IsSuccessStatusCode;
        }

        /// <summary>
        /// Gets the Azure Synapse configuration.
        /// </summary>
        /// <param name="args">The calculator run parameters.</param>
        /// <param name="pipelineName">The name of the pipeline.</param>
        /// <returns>The Azure Synapse runner parameters.</returns>
        private static AzureSynapseRunnerParameters GetAzureSynapseConfiguration(
            CalculatorRunParameter args,
            string pipelineName)
        {
            return new AzureSynapseRunnerParameters
            {
                PipelineUrl = new Uri(Configuration.PipelineUrl),
                CheckInterval = int.Parse(Configuration.CheckInterval),
                MaxCheckCount = int.Parse(Configuration.MaxCheckCount),
                PipelineName = pipelineName,
                CalculatorRunId = args.Id,
                FinancialYear = "2023",
                StatusUpdateEndpoint = Configuration.StatusEndpoint,
            };
        }

        /// <summary>
        /// Builds the JSON content of the status update API call.
        /// </summary>
        /// <param name="calculatorRunId">The calculator run ID.</param>
        /// <param name="pipelineSucceeded">Indicates whether the pipeline succeeded.</param>
        /// <returns>The JSON content for the status update.</returns>
        private static StringContent GetStatusUpdateMessage(int calculatorRunId, bool pipelineSucceeded)
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
    }
}