using System;
using System.Threading.Tasks;
using EPR.Calculator.Service.Common;
using EPR.Calculator.Service.Common.Logging;
using EPR.Calculator.Service.Function;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
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
        private readonly IClassificationService classificationService;
        private readonly IMessageTypeService messageTypeService;
        private readonly IPrepareBillingFileService prepareBillingFileService;

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
            IMessageTypeService messageTypeService,
            IPrepareBillingFileService prepareBillingFileService,
            IClassificationService classificationService)
        {
            this.calculatorRunService = calculatorRunService;
            this.calculatorRunParameterMapper = calculatorRunParameterMapper;
            this.runNameService = runNameService;
            this.classificationService = classificationService;
            this.telemetryLogger = telemetryLogger;
            this.messageTypeService = messageTypeService;
            this.prepareBillingFileService = prepareBillingFileService;
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

            CalculatorRunParameter? calculatorRunParameter = null;

            try
            {
                var resultMessageType = messageTypeService.DeserializeMessage(myQueueItem);
                string runName = string.Empty;

                try
                {
                    runName = await this.runNameService.GetRunNameAsync(resultMessageType.CalculatorRunId);
                }
                catch (Exception ex)
                {
                    this.LogError("Run name not found", ex);
                }

                if (resultMessageType is CreateBillingFileMessage billingmessage)
                {
                    var calculatorRunParameter = this.calculatorRunParameterMapper.Map(billingmessage);
                    await this.prepareBillingFileService.PrepareBillingFileAsync(calculatorRunParameter.Id, runName);
                }
                else if (resultMessageType is CreateResultFileMessage resultmessage)
                {                    
                    var calculatorRunParameter = this.calculatorRunParameterMapper.Map(resultmessage);
                    
                    bool processStatus = await this.calculatorRunService.StartProcess(calculatorRunParameter, runName);
                    this.telemetryLogger.LogInformation(new TrackMessage
                    {
                        RunId = calculatorRunParameter.Id,
                        RunName = runName,
                        Message = $"Process status: {processStatus}",
                    });
                }
            }
            catch (Exception exception)
            {
                if (calculatorRunParameter != null)
                {
                    // Set the run classification as ERROR
                    await this.classificationService.UpdateRunClassification(calculatorRunParameter.Id, RunClassification.ERROR);
                }

                this.LogError($"Error - {myQueueItem} - {exception.Message}", exception);
            }

            this.telemetryLogger.LogInformation(new TrackMessage { Message = "Azure function app execution finished" });
        }

        private CalculatorRunParameter GetCalculatorRunParameter(string message)
        {
            var param = JsonConvert.DeserializeObject<CalculatorParameter>(message);
            if (param == null)
            {
                throw new JsonException("Deserializing service bus message object resulted in null");
            }

            return this.calculatorRunParameterMapper.Map(param);
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