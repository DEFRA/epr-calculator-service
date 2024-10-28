// <copyright file="CalculatorRunService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace EPR.Calculator.Service.Function.Services
{
    using System.Diagnostics.CodeAnalysis;
    using EPR.Calculator.Service.Common;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Interface;

    // need to implement test cases during integration
    [ExcludeFromCodeCoverage]
    public class CalculatorRunService : ICalculatorRunService
    {
        public void StartProcess(CalculatorRunParameter calculatorRunParameter)
        {
            var environmentConfiguration = this.GetConfiguration(Config.IsPom);
        }

        private CalculatorRunConfiguration GetConfiguration(bool isPomData)
        {
            return new CalculatorRunConfiguration()
            {
                PipelineUrl = Configuration.PipelineUrl,
                CheckInterval = Configuration.CheckInterval,
                MaxCheckCount = Configuration.MaxCheckCount,
                PipelineName = isPomData ? Configuration.GetPomDataPipelineName : Configuration.GetOrgDataPipelineName,
            };
        }
    }
}
