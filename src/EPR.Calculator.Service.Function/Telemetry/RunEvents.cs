using Microsoft.ApplicationInsights.DataContracts;

namespace EPR.Calculator.Service.Function.Telemetry
{
    public static class RunEvents
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

        public static EventTelemetry Started(RunParams runParams)
        {
            return new EventTelemetry("PayCalRunStarted")
                .WithRunContext(runParams);
        }

        public static EventTelemetry Completed(RunParams runParams, TimeSpan elapsed)
        {
            return new EventTelemetry("PayCalRunCompleted")
                .WithRunContext(runParams)
                .WithElapsed(elapsed);
        }

        public static EventTelemetry Failed(RunParams runParams, TimeSpan elapsed, Exception? exception)
        {
            return new EventTelemetry("PayCalRunFailed")
                .WithRunContext(runParams)
                .WithElapsed(elapsed)
                .WithProperty("FailureReason", exception is not null ? "UnhandledException" : "ProcessingFailed");
        }
    }
}