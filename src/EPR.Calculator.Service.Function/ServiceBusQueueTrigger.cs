// <copyright file="ServiceBusQueueTrigger.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using EPR.Calculator.Service.Common.AzureSynapse;
using EPR.Calculator.Service.Function;
using EPR.Calculator.Service.Function.Interface;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

[assembly: FunctionsStartup(typeof(Startup))]

namespace EPR.Calculator.Service.Function
{
    public class ServiceBusQueueTrigger
    {

        private readonly ICalculatorRunService calculatorRunService;
        private readonly ICalculatorRunParameterMapper calculatorRunParameterMapper;
        private readonly IAzureSynapseRunner azureSynapseRunner;

        public ServiceBusQueueTrigger(ICalculatorRunService calculatorRunService,
        ICalculatorRunParameterMapper calculatorRunParameterMapper,
        IAzureSynapseRunner azureSynapseRunner)
        {
            this.calculatorRunService = calculatorRunService;
            this.calculatorRunParameterMapper = calculatorRunParameterMapper;
            this.azureSynapseRunner = azureSynapseRunner;
        }

        [FunctionName("EPRCalculatorRunServiceBusQueueTrigger")]
        public void Run([ServiceBusTrigger(queueName: "%ServiceBusQueueName%", Connection = "ServiceBusConnectionString")] string myQueueItem, ILogger log)
        {
            log.LogInformation("Executing the funcation app started");
            if (string.IsNullOrEmpty(myQueueItem))
            {
                log.LogError($"Message is Null or empty");
                return;
            }

            try
            {
                var param = JsonConvert.DeserializeObject<CalculatorParameter>(myQueueItem);
                var calculatorRunParameter = this.calculatorRunParameterMapper.Map(param);
                this.calculatorRunService.StartProcess(calculatorRunParameter);
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
        }
    }
}