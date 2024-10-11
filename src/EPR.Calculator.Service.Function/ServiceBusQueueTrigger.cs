using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace EPR.Calculator.Service.Function
{
    public static class ServiceBusQueueTrigger
    {
        [FunctionName("ServiceBusQueueTrigger")]
        public static void Run([ServiceBusTrigger(queueName: "defra.epr.calculator.run.dev11", Connection = "ServiceBusConnectionString")] string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# Service Bus Queue trigger function proceed messages: {myQueueItem}");

        }
    }
}