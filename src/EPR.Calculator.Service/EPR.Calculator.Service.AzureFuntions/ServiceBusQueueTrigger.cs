using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace EPR.Calculator.Service.AzureFuntions
{
    public static class ServiceBusQueueTrigger
    {
        [FunctionName("ServiceBusQueueTrigger")]
        public static void Run([ServiceBusTrigger("defra.epr.calculator.run", Connection = "AzureWebJobServiceBus")] string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# Service Bus Queue trigger function proceed messages: {myQueueItem}");

        }
    }
}
