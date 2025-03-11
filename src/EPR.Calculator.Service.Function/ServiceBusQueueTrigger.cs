// <copyright file="ServiceBusQueueTrigger.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using EPR.Calculator.Service.Common.Logging;
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
        private readonly ICalculatorTelemetryLogger telemetryLogger;
        private readonly IRunNameService runNameService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusQueueTrigger"/> class.
        /// </summary>
        /// <param name="calculatorRunService">Service to trigger the process for synapse pipeline.</param>
        /// <param name="calculatorRunParameterMapper">Mapper class to map and get the parameter.</param>
        /// <param name="runNameService">Service to fetch the run name from the database.</param>
        /// <param name="telemetryLogger">Service to fetch the telemetry log.</param>
        public ServiceBusQueueTrigger(
            ICalculatorRunService calculatorRunService,
            ICalculatorRunParameterMapper calculatorRunParameterMapper,
            IRunNameService runNameService,
            ICalculatorTelemetryLogger telemetryLogger)
        {
            this.calculatorRunService = calculatorRunService;
            this.calculatorRunParameterMapper = calculatorRunParameterMapper;
            this.runNameService = runNameService;
            this.telemetryLogger = telemetryLogger;
        }

        /// <summary>
        /// Triggering Azure function <see cref="Run"/> to read the message from Service Bus.
        /// </summary>
        /// <param name="myQueueItem">Service Bus message.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [FunctionName("EPRCalculatorRunServiceBusQueueTrigger")]
        public async Task Run([ServiceBusTrigger(queueName: "%ServiceBusQueueName%", Connection = "ServiceBusConnectionString")] string myQueueItem)
        {
            this.telemetryLogger.LogInformation(new TrackMessage { Message = "Executing the function app started" });

            if (string.IsNullOrEmpty(myQueueItem))
            {
                this.telemetryLogger.LogError(new ErrorMessage { Message = "Message is null or empty", Exception = new ArgumentNullException() });
                return;
            }

            try
            {
                var param = JsonConvert.DeserializeObject<CalculatorParameter>(myQueueItem);
                var calculatorRunParameter = this.calculatorRunParameterMapper.Map(param);

                // Fetch the run name using the run ID
                var runName = await this.runNameService.GetRunNameAsync(calculatorRunParameter.Id);

                bool processStatus = await this.calculatorRunService.StartProcess(calculatorRunParameter, runName);
                this.telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = calculatorRunParameter.Id,
                    RunName = runName,
                    Message = $"Process status: {processStatus}",
                });
            }
            catch (JsonException jsonex)
            {
                this.telemetryLogger.LogError(new ErrorMessage { Message = $"Incorrect format - {myQueueItem} - {jsonex.Message}", Exception = jsonex });
            }
            catch (Exception ex)
            {
                this.telemetryLogger.LogError(new ErrorMessage { Message = $"Error - {myQueueItem} - {ex.Message}", Exception = ex });
            }

            this.telemetryLogger.LogInformation(new TrackMessage { Message = "Azure function app execution finished" });
        }
    }
}