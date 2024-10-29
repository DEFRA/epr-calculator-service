namespace EPR.Calculator.Service.Function.Services
{
    using System;
    using EPR.Calculator.Service.Common;
    using EPR.Calculator.Service.Common.AzureSynapse;
    using EPR.Calculator.Service.Function.Interface;

    public class CalculatorRunService : ICalculatorRunService
    {
        /// <inheritdoc/>
        public void StartProcess(CalculatorRunParameter calculatorRunParameter, IAzureSynapseRunner azureSynapseRunner)
        {
            var pomPipelineConfiguration = GetAzureSynapseConfiguration(
                calculatorRunParameter,
                Configuration.PomDataPipelineName);
            azureSynapseRunner.Process(pomPipelineConfiguration);

            var orgPipelineConfiguration = GetAzureSynapseConfiguration(
                calculatorRunParameter,
                Configuration.OrgDataPipelineName);
            azureSynapseRunner.Process(orgPipelineConfiguration);
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
                FinancialYear = FinancialYear.Parse(args.FinancialYear),
                StatusUpdateEndpoint = Configuration.StatusEndpoint,
            };
    }
}
