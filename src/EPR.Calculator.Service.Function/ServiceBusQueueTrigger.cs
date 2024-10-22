using EPR.Calculator.Service.Function;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Mapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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
            _calculatorRunService.StartProcess(calculatorRunParameter);            

            return new OkObjectResult(myQueueItem);
        }
    }
}