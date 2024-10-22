using EPR.Calculator.Service.Common;
using EPR.Calculator.Service.Function;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Mapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Xml.Linq;

[assembly: FunctionsStartup(typeof(Startup))]
namespace EPR.Calculator.Service.Function
{
    public class ServiceBusQueueTrigger
    {
        private readonly ICalculatorRunService _calculatorRunService;

        public ServiceBusQueueTrigger(ICalculatorRunService calculatorRunService)
        {
                _calculatorRunService = calculatorRunService;

        }

        [FunctionName("EPRCalculatorRunServiceBusQueueTrigger")]
        public IActionResult Run([ServiceBusTrigger(queueName: "%ServiceBusQueueName%", Connection = "ServiceBusConnectionString")] string myQueueItem, ILogger log)
        {

            if (string.IsNullOrEmpty(myQueueItem)) { return new BadRequestResult(); }

            var calculatorRunParameter = CalculatorRunParameterMapper.Map(JsonConvert.DeserializeObject<CalculatorParameter>(myQueueItem));
            _calculatorRunService.startProcess(calculatorRunParameter);            

            return new OkObjectResult(myQueueItem);
        }
    }
}