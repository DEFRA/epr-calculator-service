using EPR.Calculator.Service.Common;
using EPR.Calculator.Service.Function.Interface;
using System;
using EPR.Calculator.Service.Function.Constants;


namespace EPR.Calculator.Service.Function.Services
{
    public class CalculatorRunService : ICalculatorRunService
    {
             
        public void startProcess(CalculatorRunParameter calculatorRunParameter)
        {
           var environmentConfiguration =   GetConfiguration(true);
        }


        private CalculatorRunConfiguration GetConfiguration(bool isPomData)
        {
            return new CalculatorRunConfiguration()
            {
                PipelineUrl = Configuration.PipelineUrl,
                CheckInterval = Configuration.CheckInterval,
                MaxCheckCount = Configuration.MaxCheckCount,
                PipelineName = isPomData ? Configuration.GetPomDataPipelineName : Configuration.GetOrgDataPipelineName
            };

        }
    }
}
