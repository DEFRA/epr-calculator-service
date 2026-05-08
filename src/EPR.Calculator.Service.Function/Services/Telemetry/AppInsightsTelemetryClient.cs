using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace EPR.Calculator.Service.Function.Services.Telemetry;

[ExcludeFromCodeCoverage]
public class AppInsightsTelemetryClient(TelemetryClient client)
    : ITelemetryClient
{
    /// <inheritdoc />
    public void TrackEvent(EventTelemetry telemetry)
    {
        client.TrackEvent(telemetry);
    }

    /// <inheritdoc />
    public void TrackDuration(string metricId, TimeSpan duration)
    {
        // For consistency
        if (!metricId.EndsWith("DurationMs"))
            metricId += "DurationMs";

        client.GetMetric(metricId).TrackValue(duration);
    }

    /// <inheritdoc />
    public async Task TrackDuration(string metricId, Func<Task> func)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            await func();
        }
        finally
        {
            sw.Stop();
            TrackDuration(metricId, sw.Elapsed);
        }
    }

    /// <inheritdoc />
    public async Task<T> TrackDuration<T>(string metricId, Func<Task<T>> func)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            return await func();
        }
        finally
        {
            sw.Stop();
            TrackDuration(metricId, sw.Elapsed);
        }
    }

    /// <inheritdoc />
    public T TrackDuration<T>(string metricId, Func<T> func)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            return func();
        }
        finally
        {
            sw.Stop();
            TrackDuration(metricId, sw.Elapsed);
        }
    }
}
