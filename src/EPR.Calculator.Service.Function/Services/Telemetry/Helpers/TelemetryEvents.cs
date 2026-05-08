using EPR.Calculator.Service.Function.Features.Common;
using Microsoft.ApplicationInsights.DataContracts;

namespace EPR.Calculator.Service.Function.Services.Telemetry.Helpers;

public static class TelemetryEvents
{
    public static EventTelemetry RunInit()
    {
        return new EventTelemetry("RunInit");
    }

    public static EventTelemetry RunInitFailed(string serviceBusMessage)
    {
        return new EventTelemetry("RunInitFailed")
            .WithProperty("ServiceBusMessage", serviceBusMessage);
    }

    public static EventTelemetry RunStarted(RunContext runContext)
    {
        return new EventTelemetry("RunStarted")
            .WithRunContext(runContext);
    }

    public static EventTelemetry RunCompleted(RunContext runContext, TimeSpan duration)
    {
        return new EventTelemetry("RunCompleted")
            .WithRunContext(runContext)
            .WithDuration(duration);
    }

    public static EventTelemetry RunFailed(RunContext runContext, TimeSpan duration, string reason)
    {
        return new EventTelemetry("RunFailed")
            .WithRunContext(runContext)
            .WithDuration(duration)
            .WithProperty("FailureReason", reason);
    }
}
