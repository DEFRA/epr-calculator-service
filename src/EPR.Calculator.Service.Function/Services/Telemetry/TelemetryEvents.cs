using EPR.Calculator.Service.Function.Features.Common;
using Microsoft.ApplicationInsights.DataContracts;

namespace EPR.Calculator.Service.Function.Services.Telemetry;

public static class TelemetryEvents
{
    public static EventTelemetry Init()
    {
        return new EventTelemetry("PayCalRunInit");
    }

    public static EventTelemetry InitFailed(string serviceBusMessage)
    {
        return new EventTelemetry("PayCalRunInitFailed")
            .WithProperty("ServiceBusMessage", serviceBusMessage);
    }

    public static EventTelemetry Started(RunContext runContext)
    {
        return new EventTelemetry("PayCalRunStarted")
            .WithRunContext(runContext);
    }

    public static EventTelemetry Completed(RunContext runContext, TimeSpan elapsed)
    {
        return new EventTelemetry("PayCalRunCompleted")
            .WithRunContext(runContext)
            .WithElapsed(elapsed);
    }

    public static EventTelemetry Failed(RunContext runContext, TimeSpan elapsed, string reason)
    {
        return new EventTelemetry("PayCalRunFailed")
            .WithRunContext(runContext)
            .WithElapsed(elapsed)
            .WithProperty("FailureReason", reason);
    }

    public static ExceptionTelemetry WarnException(Exception exception, RunContext? runContext = null)
    {
        var telemetry = new ExceptionTelemetry(exception)
        {
            SeverityLevel = SeverityLevel.Warning
        };

        if (runContext != null)
        {
            telemetry.WithRunContext(runContext);
        }

        return telemetry;
    }

    public static ExceptionTelemetry ErrorException(Exception exception, RunContext? runContext = null)
    {
        var telemetry = new ExceptionTelemetry(exception)
        {
            SeverityLevel = SeverityLevel.Error
        };

        if (runContext != null)
        {
            telemetry.WithRunContext(runContext);
        }

        return telemetry;
    }

    public static ExceptionTelemetry CriticalException(Exception exception, RunContext? runContext = null)
    {
        var telemetry = new ExceptionTelemetry(exception)
        {
            SeverityLevel = SeverityLevel.Critical
        };

        if (runContext != null)
        {
            telemetry.WithRunContext(runContext);
        }

        return telemetry;
    }
}