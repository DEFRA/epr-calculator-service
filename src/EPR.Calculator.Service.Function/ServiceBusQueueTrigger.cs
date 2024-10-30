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
        private readonly ILogger logger;

        public ServiceBusQueueTrigger(ICalculatorRunService calculatorRunService,
        ICalculatorRunParameterMapper calculatorRunParameterMapper,
        IAzureSynapseRunner azureSynapseRunner,
        ILogger<ServiceBusQueueTrigger> logger)
        {
            this.calculatorRunService = calculatorRunService;
            this.calculatorRunParameterMapper = calculatorRunParameterMapper;
            this.azureSynapseRunner = azureSynapseRunner;
            this.logger = logger;
        }

        [FunctionName("EPRCalculatorRunServiceBusQueueTrigger")]
        public void Run([ServiceBusTrigger(queueName: "%ServiceBusQueueName%", Connection = "ServiceBusConnectionString")] string myQueueItem)
        {
            if (string.IsNullOrEmpty(myQueueItem))
            {
               this.logger.LogError($"Message is Null or empty");
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
                this.logger.LogError($"Incorrect format - {myQueueItem} - {jsonex.Message}");
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Error  - {myQueueItem} - {ex.Message}");
            }
        }
    }
}