using System.Diagnostics;
using EPR.Calculator.Service.Function;
using EPR.Calculator.Service.Function.Exceptions;
using EPR.Calculator.Service.Function.Features.Billing;
using EPR.Calculator.Service.Function.Features.Billing.Contexts;
using EPR.Calculator.Service.Function.Features.Calculator;
using EPR.Calculator.Service.Function.Features.Calculator.Contexts;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Services.Telemetry;
using EPR.Calculator.Service.Function.Services.Telemetry.Helpers;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;

[assembly: FunctionsStartup(typeof(Startup))]

namespace EPR.Calculator.Service.Function;

public class ServiceBusQueueTrigger(
    ICalculatorRunContextBuilder calculatorRunContextBuilder,
    ICalculatorRunProcessor calculatorRunProcessor,
    IBillingRunContextBuilder billingRunContextBuilder,
    IBillingRunProcessor billingRunProcessor,
    ITelemetryClient telemetry,
    ILogger<ServiceBusQueueTrigger> logger)
{
    /// <summary>
    ///     This is the main entry point for the PayCal service. It triggers the calculator/billing run processes.
    /// </summary>
    [FunctionName("EPRCalculatorRunServiceBusQueueTrigger")]
    public async Task Run(
        [ServiceBusTrigger("%ServiceBusQueueName%", Connection = "ServiceBusConnectionString")]
        ServiceBusMessage message,
        CancellationToken cancellationToken)
    {
        try
        {
            telemetry.TrackEvent(TelemetryEvents.RunInit());
            var sw = Stopwatch.StartNew();

            RunContext runContext = message.RunType switch
            {
                RunType.Calculator => await calculatorRunContextBuilder.CreateContext(message.RunId, message.User, cancellationToken),
                RunType.Billing => await billingRunContextBuilder.CreateContext(message.RunId, message.User, cancellationToken),
                _ => throw new RunContextException(RunType.Unknown, message.RunId, $"Invalid message type: {message.RunType}")
            };

            await ProcessRun(runContext, sw, cancellationToken);
        }
        catch (RunContextException ex)
        {
            logger.LogError(ex, "Run initialization failed for: '{Message}'", message);
            telemetry.TrackEvent(TelemetryEvents.RunInitFailed(message.ToString()));
        }
        catch (Exception ex)
        {
            // Exceptions that occur during run context initialization/processing should already be handled.
            // So if we're here, it's likely due to service misconfiguration.
            logger.LogCritical(ex, "Run failed (unhandled exception)");
        }
    }

    private async Task ProcessRun(RunContext runContext, Stopwatch sw, CancellationToken ct)
    {
        using (logger.BeginScope(runContext.Summary))
        {
            telemetry.TrackEvent(TelemetryEvents.RunStarted(runContext));

            var processingTask = runContext switch
            {
                BillingRunContext billingRunContext => billingRunProcessor.Process(billingRunContext, ct),
                CalculatorRunContext calculatorRunContext => calculatorRunProcessor.Process(calculatorRunContext, ct),
                _ => throw new ArgumentException("Invalid runContext type: " + runContext.GetType().Name)
            };

            var success = await processingTask;
            sw.Stop();

            telemetry.TrackEvent(success
                ? TelemetryEvents.RunCompleted(runContext, sw.Elapsed)
                : TelemetryEvents.RunFailed(runContext, sw.Elapsed, "ProcessingFailed"));

            telemetry.TrackDuration($"{runContext.RunType}Run", sw.Elapsed);
        }
    }

    public sealed record ServiceBusMessage
    {
        public required RunType RunType { get; init; }
        public required int RunId { get; init; }
        public required string? User { get; init; }
    }
}
