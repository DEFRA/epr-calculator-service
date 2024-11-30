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

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusQueueTrigger"/> class.
        /// </summary>
        /// <param name="calculatorRunService">Service to trigger the process for synapse pipeline</param>
        /// <param name="calculatorRunParameterMapper">Mapper class to map and get the parameter</param>
        public ServiceBusQueueTrigger(ICalculatorRunService calculatorRunService, ICalculatorRunParameterMapper calculatorRunParameterMapper)
        {
            this.calculatorRunService = calculatorRunService;
            this.calculatorRunParameterMapper = calculatorRunParameterMapper;
        }

        /// <summary>
        /// Triggering azure function <see cref="Run"/> to read the message from service bus.
        /// </summary>
        /// <param name="myQueueItem">Service bus message</param>
        /// <param name="log">Logger object for logging</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [FunctionName("EPRCalculatorRunServiceBusQueueTrigger")]
        public async Task<bool> Run([ServiceBusTrigger(queueName: "%ServiceBusQueueName%", Connection = "ServiceBusConnectionString")] string myQueueItem, ILogger log)
        {
            bool processStatus = false;
            log.LogInformation("Executing the funcation app started");
            if (string.IsNullOrEmpty(myQueueItem))
            {
                log.LogError($"Message is Null or empty");
                return processStatus;
            }

            try
            {
                var param = JsonConvert.DeserializeObject<CalculatorParameter>(myQueueItem);
                var calculatorRunParameter = this.calculatorRunParameterMapper.Map(param);
                processStatus = await this.calculatorRunService.StartProcess(calculatorRunParameter);
            }
            catch (JsonException jsonex)
            {
                log.LogError($"Incorrect format - {myQueueItem} - {jsonex.Message}");
            }
            catch (Exception ex)
            {
                log.LogError($"Error  - {myQueueItem} - {ex.Message}");
            }

            log.LogInformation("Azure function app execution finished");
            return processStatus;
        }
    }
}