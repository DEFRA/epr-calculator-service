using Microsoft.ApplicationInsights.DataContracts;

namespace EPR.Calculator.Service.Function.Telemetry
{
    public static class TelemetryExtensions
    {
        public static TTelemetry WithProperty<TTelemetry>(this TTelemetry telemetry, string key, string value)
            where TTelemetry : ISupportProperties
        {
            telemetry.Properties[key] = value;
            return telemetry;
        }

        public static TTelemetry WithRunContext<TTelemetry>(this TTelemetry telemetry, RunParams runParams)
            where TTelemetry : ISupportProperties
        {
            return telemetry
                .WithProperty("RunId", runParams.Id.ToString())
                .WithProperty("RunType", runParams.Type)
                .WithProperty("RunName", runParams.Name);
        }

        public static TTelemetry WithElapsed<TTelemetry>(this TTelemetry telemetry, TimeSpan elapsed)
            where TTelemetry : ISupportProperties
        {
            return telemetry
                .WithProperty("ElapsedMs", elapsed.TotalMilliseconds.ToString("F0"));
        }
    }
}