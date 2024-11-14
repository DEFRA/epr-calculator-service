namespace EPR.Calculator.Service.Function.Services
{
    using System;
    using EPR.Calculator.Service.Common;
    using EPR.Calculator.Service.Common.AzureSynapse;
    using EPR.Calculator.Service.Common.Utils;
    using EPR.Calculator.Service.Function.Interface;
    using Microsoft.Extensions.Logging;

    public class CalculatorRunService : ICalculatorRunService
    {
        private readonly IAzureSynapseRunner azureSynapseRunner;
        private readonly ILogger logger;

        public CalculatorRunService(IAzureSynapseRunner azureSynapseRunner, ILogger<CalculatorRunService> logger)
        {
            this.logger = logger;
            this.azureSynapseRunner = azureSynapseRunner;
        }

        /// <inheritdoc/>
        public async void StartProcess(CalculatorRunParameter calculatorRunParameter)
        {
            var pipelineFactory = new PipelineClientFactory();

            this.logger.LogInformation("Process started");

            //var pomPipelineConfiguration = GetAzureSynapseConfiguration(
            //    calculatorRunParameter,
            //    Configuration.PomDataPipelineName);
            //  var isPomSuccessful = await azureSynapseRunner.Process(pomPipelineConfiguration);

            //this.logger.LogInformation("Pom status", isPomSuccessful);

            //var orgPipelineConfiguration = GetAzureSynapseConfiguration(
            //    calculatorRunParameter,
            //    Configuration.OrgDataPipelineName);

            // var isOrgSuccessful = await azureSynapseRunner.Process(orgPipelineConfiguration);

            //  this.logger.LogInformation("Org status", isOrgSuccessful);

            // Record success/failure to the database using the web API.
            using var client = pipelineFactory.GetStatusUpdateClient(Configuration.StatusEndpoint);
            var statusUpdateResponse = await client.PostAsync(
                    Configuration.StatusEndpoint,
                    AzureSynapseRunner.GetStatusUpdateMessage(calculatorRunParameter.Id, true));

        }

        private static AzureSynapseRunnerParameters GetAzureSynapseConfiguration(
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
    }
}
