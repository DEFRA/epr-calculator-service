namespace EPR.Calculator.Service.Function.Services
{
    using System;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using EPR.Calculator.Service.Common;
    using EPR.Calculator.Service.Common.AzureSynapse;
    using EPR.Calculator.Service.Common.Utils;
    using EPR.Calculator.Service.Function.Interface;
    using Microsoft.Extensions.Logging;

    public class CalculatorRunService : ICalculatorRunService
    {
        private readonly IAzureSynapseRunner azureSynapseRunner;
        private readonly ILogger logger;
        private readonly PipelineClientFactory pipelineClientFactory;

        public CalculatorRunService(IAzureSynapseRunner azureSynapseRunner, ILogger<CalculatorRunService> logger)
        {
            this.logger = logger;
            this.azureSynapseRunner = azureSynapseRunner;
            this.pipelineClientFactory = new PipelineClientFactory();
        }

        /// <inheritdoc/>
        public async void StartProcess(CalculatorRunParameter calculatorRunParameter)
        {
            var pipelineFactory = new PipelineClientFactory();

            this.logger.LogInformation("Process started");
            bool isPomSuccessful = bool.Parse(Configuration.ExecuteRPDPipeline) ? false : true;

            if (bool.Parse(Configuration.ExecuteRPDPipeline))
            {
                var orgPipelineConfiguration = GetAzureSynapseConfiguration(
                  calculatorRunParameter,
                  Configuration.OrgDataPipelineName);

                var isOrgSuccessful = await this.azureSynapseRunner.Process(orgPipelineConfiguration);

                this.logger.LogInformation("Org status", isOrgSuccessful);

                if (isOrgSuccessful)
                {
                    var pomPipelineConfiguration = GetAzureSynapseConfiguration(
                        calculatorRunParameter,
                        Configuration.PomDataPipelineName);
                    isPomSuccessful = await this.azureSynapseRunner.Process(pomPipelineConfiguration);

                    this.logger.LogInformation("Pom status", isPomSuccessful);
                }
            }

            // Record success/failure to the database using the web API.
            using var client = this.pipelineClientFactory.GetStatusUpdateClient(Configuration.StatusEndpoint);
            var statusUpdateResponse = await client.PostAsync(
                    Configuration.StatusEndpoint,
                    GetStatusUpdateMessage(calculatorRunParameter.Id, isPomSuccessful));

            #if DEBUG
            Debug.WriteLine(statusUpdateResponse.Content.ReadAsStringAsync().Result);
            #endif

        }

        public static AzureSynapseRunnerParameters GetAzureSynapseConfiguration(
            CalculatorRunParameter args,
            string pipelineName)
            => new AzureSynapseRunnerParameters()
            {
                PipelineUrl = new Uri(Configuration.PipelineUrl),
                CheckInterval = int.Parse(Configuration.CheckInterval),
                MaxChecks = int.Parse(Configuration.MaxCheckCount),
                PipelineName = pipelineName,
                CalculatorRunId = args.Id,
                FinancialYear = Util.GetFinancialYearAsYYYY(args.FinancialYear),
                StatusUpdateEndpoint = Configuration.StatusEndpoint,
            };

        /// <summary>
        /// Build the JSON content of the status update API call.
        /// </summary>
        private static StringContent GetStatusUpdateMessage(int calculatorRunId, bool pipelineSucceeded)
            => new StringContent(
                JsonSerializer.Serialize(new
                {
                    runId = calculatorRunId,
                    updatedBy = "string",
                    isSuccessful = pipelineSucceeded,
                }),
                Encoding.UTF8,
                "application/json");
    }
}
