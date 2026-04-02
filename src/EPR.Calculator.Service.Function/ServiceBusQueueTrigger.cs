using System.Diagnostics;
using EPR.Calculator.Service.Function;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Exceptions;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.Telemetry;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

[assembly: FunctionsStartup(typeof(Startup))]

namespace EPR.Calculator.Service.Function
{
    /// <summary>
    ///     This is the main entry point for the PayCal service. It triggers the calculator/billing run processes to generate
    ///     the appropriate files.
    /// </summary>
    public class ServiceBusQueueTrigger(
        ICalculatorRunService calculatorRunService,
        IRunNameService runNameService,
        ILogger<ServiceBusQueueTrigger> logger,
        IMessageTypeService messageTypeService,
        IPrepareBillingFileService prepareBillingFileService,
        IClassificationService classificationService,
        ITelemetryClient telemetryClient)
    {
        private const string QueueName = "%ServiceBusQueueName%";
        private const string Connection = "ServiceBusConnectionString";

        [FunctionName("EPRCalculatorRunServiceBusQueueTrigger")]
        public async Task Run([ServiceBusTrigger(QueueName, Connection = Connection)] string message)
        {
            var (runParams, runFunc) = await InitialiseRun(message);
            await DoRun(runParams, runFunc);
        }

        private async Task<(RunParams runParams, Func<Task<bool>> runMethod)> InitialiseRun(string message)
        {
            try
            {
                telemetryClient.TrackEvent(RunEvents.Init());

                var deserializedMessage = messageTypeService.DeserializeMessage(message);
                var runName = await runNameService.GetRunNameAsync(deserializedMessage.CalculatorRunId);

                switch (deserializedMessage)
                {
                    case CreateResultFileMessage createResultFileMessage:
                    {
                        var runParams = new CalculatorRunParams
                        {
                            Id = createResultFileMessage.CalculatorRunId,
                            Name = runName,
                            User = createResultFileMessage.CreatedBy,
                            RelativeYear = createResultFileMessage.RelativeYear
                        };

                        return (runParams, () => calculatorRunService.PrepareResultsFileAsync(runParams));
                    }
                    case CreateBillingFileMessage createBillingFileMessage:
                    {
                        var runParams = new BillingRunParams
                        {
                            Id = createBillingFileMessage.CalculatorRunId,
                            Name = runName,
                            ApprovedBy = createBillingFileMessage.ApprovedBy
                        };

                        return (runParams, () => prepareBillingFileService.PrepareBillingFileAsync(runParams));
                    }
                }

                throw new ArgumentException("Invalid message type: " + deserializedMessage.GetType().Name);
            }
            catch (Exception ex)
            {
                telemetryClient.TrackEvent(RunEvents.InitFailed(message));
                throw new RunInitialiseException(message, ex);
            }
        }

        private async Task DoRun(RunParams runParams, Func<Task<bool>> runFunc)
        {
            using (logger.BeginScope(runParams))
            {
                telemetryClient.TrackEvent(RunEvents.Started(runParams));

                var success = false;
                Exception? runException = null;

                var sw = Stopwatch.StartNew();

                try
                {
                    success = await runFunc();
                }
                catch (Exception ex)
                {
                    runException = ex;
                }
                finally
                {
                    sw.Stop();

                    if (success)
                    {
                        telemetryClient.TrackEvent(RunEvents.Completed(runParams, sw.Elapsed));
                    }
                    else
                    {
                        await HandleRunFailure(runParams, sw.Elapsed, runException);
                    }
                }
            }
        }

        private async Task HandleRunFailure(RunParams runParams, TimeSpan elapsed, Exception? runException = null)
        {
            telemetryClient.TrackEvent(RunEvents.Failed(runParams, elapsed, runException));

            if (runException is not null)
            {
                logger.LogError(runException, "Run failed (unhandled exception). Elapsed:{Elapsed}",
                    elapsed.ToString("g"));

                telemetryClient.TrackException(new ExceptionTelemetry(runException).WithRunContext(runParams));
            }
            else
            {
                logger.LogError("Run failed (probably invalid data/state). Elapsed:{Elapsed}",
                    elapsed.ToString("g"));
            }

            try
            {
                await classificationService.UpdateRunClassification(runParams.Id, RunClassification.ERROR);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unable to classify run as ERROR(5) after run failure");
            }
        }
    }
}