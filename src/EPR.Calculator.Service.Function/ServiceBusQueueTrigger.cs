// <copyright file="ServiceBusQueueTrigger.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using EPR.Calculator.Service.Function;
using EPR.Calculator.Service.Function.Interface;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

[assembly: FunctionsStartup(typeof(Startup))]

namespace EPR.Calculator.Service.Function
{
    /// <summary>
    /// ServiceBusQueueTrigger to trigger the calculator run process and generating the result file.
    /// </summary>
    public class ServiceBusQueueTrigger
    {
        private readonly ICalculatorRunService calculatorRunService;
        private readonly ICalculatorRunParameterMapper calculatorRunParameterMapper;
        private readonly IRunNameService runNameService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusQueueTrigger"/> class.
        /// </summary>
        /// <param name="calculatorRunService">Service to trigger the process for synapse pipeline.</param>
        /// <param name="calculatorRunParameterMapper">Mapper class to map and get the parameter.</param>
        /// <param name="runNameService">Service to fetch the run name from the database.</param>
        public ServiceBusQueueTrigger(
            ICalculatorRunService calculatorRunService,
            ICalculatorRunParameterMapper calculatorRunParameterMapper,
            IRunNameService runNameService)
        {
            this.calculatorRunService = calculatorRunService;
            this.calculatorRunParameterMapper = calculatorRunParameterMapper;
            this.runNameService = runNameService;
        }

        /// <summary>
        /// Triggering Azure function <see cref="Run"/> to read the message from Service Bus.
        /// </summary>
        /// <param name="myQueueItem">Service Bus message.</param>
        /// <param name="log">Logger object for logging.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [FunctionName("EPRCalculatorRunServiceBusQueueTrigger")]
        public async Task Run([ServiceBusTrigger(queueName: "%ServiceBusQueueName%", Connection = "ServiceBusConnectionString")] string myQueueItem, ILogger log)
        {
            log.LogInformation("Executing the function app started");
            if (string.IsNullOrEmpty(myQueueItem))
            {
                log.LogError("Message is null or empty");
                return;
            }

            try
            {
                var param = JsonConvert.DeserializeObject<CalculatorParameter>(myQueueItem);
                var calculatorRunParameter = this.calculatorRunParameterMapper.Map(param);

                // Fetch the run name using the run ID
                var runName = await this.runNameService.GetRunNameAsync(calculatorRunParameter.Id);

                bool processStatus = await this.calculatorRunService.StartProcess(calculatorRunParameter, runName);
                log.LogInformation($"Process status: {processStatus}");
            }
            catch (JsonException jsonex)
            {
                log.LogError($"Incorrect format - {myQueueItem} - {jsonex.Message}");
            }
            catch (Exception ex)
            {
                log.LogError($"Error - {myQueueItem} - {ex.Message}");
            }

            log.LogInformation("Azure function app execution finished");
        }
    }
}