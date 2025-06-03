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
    /// ServiceBusQueueTrigger to trigger the calculator run process and generate the result file.
    /// </summary>
    public class ServiceBusQueueTrigger
    {
        private readonly ICalculatorRunService calculatorRunService;
        private readonly ICalculatorRunParameterMapper calculatorRunParameterMapper;
        private readonly ICalculatorTelemetryLogger telemetryLogger;
        private readonly IRunNameService runNameService;
        private readonly IPrepareBillingService prepareBillingService;

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
            ICalculatorTelemetryLogger telemetryLogger,
            IPrepareBillingService prepareBillingService)
        {
            this.calculatorRunService = calculatorRunService;
            this.calculatorRunParameterMapper = calculatorRunParameterMapper;
            this.runNameService = runNameService;
            this.telemetryLogger = telemetryLogger;
            this.prepareBillingService = prepareBillingService;
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
                this.LogError("Message is null or empty", new ArgumentNullException(nameof(myQueueItem)));
                return;
            }

            try
            {
                BillingInstruction billingInstructionParameter;
                var calculatorParameter = JsonConvert.DeserializeObject<CalculatorParameter>(myQueueItem);
                if (calculatorParameter == null 
                    || 
                    calculatorParameter.CalculatorRunId <= 0)
                {
                    // this.LogError("Deserialized object is null", new JsonException($"Deserialized object is null"));
                    // throw new JsonException("Deserialized object is null");

                    billingInstructionParameter = JsonConvert.DeserializeObject<BillingInstruction>(myQueueItem);
                    if (billingInstructionParameter == null)
                    {
                        this.LogError("Deserialized object is null", new JsonException($"Deserialized object is null"));
                        throw new JsonException("Deserialized object is null");
                    }
                }
                


                string? runName = string.Empty;
                var calculatorRunParameter = this.calculatorRunParameterMapper.Map(calculatorParameter);
                try
                {
                    runName = await this.runNameService.GetRunNameAsync(calculatorRunParameter.Id);
                }
                catch (Exception ex)
                {
                    this.LogError("Run name not found", ex);
                }

                if (calculatorParameter != null && calculatorParameter.CalculatorRunId <= 0)
                {
                    bool processStatus = await this.calculatorRunService.StartProcess(calculatorRunParameter, runName);
                    this.telemetryLogger.LogInformation(new TrackMessage
                    {
                        RunId = calculatorRunParameter.Id,
                        RunName = runName,
                        Message = $"Process status: {processStatus}",
                    });
                }
                else
                {
                    var resultsRequestDto = new Dtos.CalcResultsRequestDto
                    {
                        RunId = calculatorParameter.CalculatorRunId
                    };
                    await this.prepareBillingService.PrepareBilling(resultsRequestDto);
                }
            }
            catch (JsonException jsonex)
            {
                this.LogError($"Incorrect format - {myQueueItem} - {jsonex.Message}", jsonex);
            }
            catch (Exception ex)
            {
                this.LogError($"Error - {myQueueItem} - {ex.Message}", ex);
            }

            this.telemetryLogger.LogInformation(new TrackMessage { Message = "Azure function app execution finished" });
        }

        private void LogError(string message, Exception exception)
        {
            var errorMessage = new ErrorMessage
            {
                Message = message,
                Exception = exception,
                RunId = null,
                RunName = string.Empty,
            };
            this.telemetryLogger.LogError(errorMessage);
        }
    }
}