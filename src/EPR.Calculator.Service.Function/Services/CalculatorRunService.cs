// <copyright file="CalculatorRunService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace EPR.Calculator.Service.Function.Services
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using EPR.Calculator.Service.Common;
    using EPR.Calculator.Service.Common.AzureSynapse;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Interface;

    // need to implement test cases during integration
    [ExcludeFromCodeCoverage]
    public class CalculatorRunService : ICalculatorRunService
    {
        public void StartProcess(CalculatorRunParameter calculatorRunParameter, IAzureSynapseRunner azureSynapseRunner)
        {
            var pomPipelineConfiguration = this.GetAzureSynapseConfiguration(
                calculatorRunParameter,
                Configuration.PomDataPipelineName);
            azureSynapseRunner.Process(pomPipelineConfiguration);

            var orgPipelineConfiguration = this.GetAzureSynapseConfiguration(
                calculatorRunParameter,
                Configuration.OrgDataPipelineName);
            azureSynapseRunner.Process(orgPipelineConfiguration);
        }

        private AzureSynapseRunnerParameters GetAzureSynapseConfiguration(
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
                StatusUpdateEndpoint = new Uri(EnvironmentVariableKeys.StatusUpdateEndpoint),
            };
    }
}
