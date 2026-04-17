using System.Diagnostics;
using EPR.Calculator.Service.Function;
using EPR.Calculator.Service.Function.Features.Billing;
using EPR.Calculator.Service.Function.Features.Billing.Contexts;
using EPR.Calculator.Service.Function.Features.Calculator;
using EPR.Calculator.Service.Function.Features.Calculator.Contexts;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Messaging;
using EPR.Calculator.Service.Function.Services.Telemetry;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

[assembly: FunctionsStartup(typeof(Startup))]

namespace EPR.Calculator.Service.Function;

public class ServiceBusQueueTrigger(
    IMessageTypeService messageTypeService,
    ICalculatorRunContextBuilder calculatorRunContextBuilder,
    ICalculatorRunProcessor calculatorRunProcessor,
    IBillingRunContextBuilder billingRunContextBuilder,
    IBillingRunProcessor billingRunProcessor,
    ITelemetryClient telemetryClient,
    ILogger<ServiceBusQueueTrigger> logger)
{
    /// <summary>
    ///     This is the main entry point for the PayCal service. It triggers the calculator/billing run processes.
    /// </summary>
    [FunctionName("EPRCalculatorRunServiceBusQueueTrigger")]
    public async Task Run(
        [ServiceBusTrigger("%ServiceBusQueueName%", Connection = "ServiceBusConnectionString")]
        string message,
        CancellationToken cancellationToken)
    {
        var runContext = await InitializeRun(message, cancellationToken);
        await ProcessRun(runContext, cancellationToken);
    }

    private async Task<RunContext> InitializeRun(string message, CancellationToken ct)
    {
        try
        {
            telemetryClient.Track(TelemetryEvents.Init());

            var deserializedMessage = messageTypeService.DeserializeMessage(message);

            return deserializedMessage switch
            {
                CreateResultFileMessage createResultFileMessage => await calculatorRunContextBuilder.CreateContext(createResultFileMessage, ct),
                CreateBillingFileMessage createBillingFileMessage => await billingRunContextBuilder.CreateContext(createBillingFileMessage, ct),
                _ => throw new ArgumentException("Invalid message type: " + deserializedMessage.GetType().Name)
            };
        }
        catch (Exception ex)
        {
            telemetryClient.Track(TelemetryEvents.InitFailed(message));
            telemetryClient.Track(TelemetryEvents.ErrorException(ex));

            // If initialization fails, it may just be due to a transient data issue.
            // Rethrow so this can be re-queued on the service bus and retried.
            throw;
        }
    }

    private async Task ProcessRun(RunContext runContext, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        telemetryClient.Track(TelemetryEvents.Started(runContext));

        using (logger.BeginScope(runContext.Summary))
        {
            try
            {
                var processingTask = runContext switch
                {
                    BillingRunContext billingRunContext => billingRunProcessor.Process(billingRunContext, ct),
                    CalculatorRunContext calculatorRunContext => calculatorRunProcessor.Process(calculatorRunContext, ct),
                    _ => throw new ArgumentException("Invalid runContext type: " + runContext.GetType().Name)
                };

                var success = await processingTask;
                sw.Stop();

                telemetryClient.Track(success
                    ? TelemetryEvents.Completed(runContext, sw.Elapsed)
                    : TelemetryEvents.Failed(runContext, sw.Elapsed, "ProcessingFailed"));
            }
            catch (Exception ex)
            {
                // Exceptions that occur during processing are expected to be handled within each processor.
                // So if we're here, it's likely due to service misconfiguration.
                sw.Stop();
                logger.LogCritical(ex, "Run failed (unhandled exception). Elapsed:{Elapsed}", sw.Elapsed);
                telemetryClient.Track(TelemetryEvents.CriticalException(ex, runContext));
                telemetryClient.Track(TelemetryEvents.Failed(runContext, sw.Elapsed, "UnhandledException"));
            }
        }
    }
}