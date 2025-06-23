using System;
using System.Threading.Tasks;
using EPR.Calculator.Service.Common.Logging;
using EPR.Calculator.Service.Function;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;

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
        private readonly IMessageTypeService messageTypeService;
        private readonly IPrepareBillingFileService prepareBillingFileService;
        private readonly IClassificationService classificationService;

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
            this.telemetryLogger = telemetryLogger;
            this.messageTypeService = messageTypeService;
            this.prepareBillingFileService = prepareBillingFileService;
            this.classificationService = classificationService;
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

            MessageBase? resultMessageType = null;
            bool processStatus = false;

            try
            {
                resultMessageType = messageTypeService.DeserializeMessage(myQueueItem);

                var runName = await this.runNameService.GetRunNameAsync(resultMessageType.CalculatorRunId);

                if (resultMessageType is CreateBillingFileMessage billingmessage)
                {
                    var billingFileMessage = this.calculatorRunParameterMapper.Map(billingmessage);

                    processStatus = await this.prepareBillingFileService.PrepareBillingFileAsync(billingFileMessage.Id, runName!, billingFileMessage.ApprovedBy);
                }
                
                if (resultMessageType is CreateResultFileMessage resultmessage)
                {
                    var calculatorRunParameter = this.calculatorRunParameterMapper.Map(resultmessage);
                    this.telemetryLogger.LogInformation(new TrackMessage { Message = $"Process is going start with message type: {calculatorRunParameter.MessageType}" });

                    processStatus = await this.calculatorRunService.StartProcess(calculatorRunParameter, runName);
                }

                this.telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = resultMessageType.CalculatorRunId,
                    RunName = runName,
                    Message = $"Process status: {processStatus}",
                });

                if (!processStatus)
                {
                    // Set the run classification as ERROR
                    await this.classificationService.UpdateRunClassification(resultMessageType.CalculatorRunId, RunClassification.ERROR);
                }
            }
            catch (Exception exception)
            {
                if (resultMessageType != null)
                {
                    // Set the run classification as ERROR
                    await this.classificationService.UpdateRunClassification(resultMessageType.CalculatorRunId, RunClassification.ERROR);
                }

                this.LogError($"Error - {myQueueItem} - {exception.Message}", exception);
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