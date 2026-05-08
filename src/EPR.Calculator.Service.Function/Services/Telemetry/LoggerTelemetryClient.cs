using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.ApplicationInsights.DataContracts;

namespace EPR.Calculator.Service.Function.Services.Telemetry;

/// <summary>
///     Telemetry client that relies on logger calls. For local development only.
/// </summary>
[ExcludeFromCodeCoverage]
public class LoggerTelemetryClient(ILogger<LoggerTelemetryClient> logger)
    : ITelemetryClient
{
    /// <inheritdoc />
    public void TrackEvent(EventTelemetry telemetry)
    {
        if (telemetry.Properties.Count > 0)
        {
            logger.LogInformation("Tracked event: {EventName}. {Properties}",
                telemetry.Name, string.Join(", ", telemetry.Properties.Select(kvp => $"{kvp.Key}: {kvp.Value}")));
        }
        else
        {
            logger.LogInformation("Tracked event: {EventName}",
                telemetry.Name);
        }
    }

    /// <inheritdoc />
    public void TrackDuration(string metricId, TimeSpan duration)
    {
        logger.LogDebug("{MetricId} Duration: {Duration}", metricId, duration);
    }

    /// <inheritdoc />
    public async Task TrackDuration(string metricId, Func<Task> func)
    {
        var success = false;
        var sw = Stopwatch.StartNew();

        try
        {
            logger.LogTrace("{MetricId}: Starting...", metricId);
            await func();
            success = true;
        }
        finally
        {
            sw.Stop();

            logger.LogDebug("{MetricId}: {Status}. Duration: {Duration}",
                metricId, success ? "Completed" : "FAILED", sw.Elapsed);
        }
    }

    /// <inheritdoc />
    public async Task<T> TrackDuration<T>(string metricId, Func<Task<T>> func)
    {
        var success = false;
        var sw = Stopwatch.StartNew();

        try
        {
            logger.LogTrace("{MetricId}: Starting...", metricId);
            var result = await func();
            success = true;
            return result;
        }
        finally
        {
            sw.Stop();

            logger.LogDebug("{MetricId}: {Status}. Duration: {Duration}",
                metricId, success ? "Completed" : "FAILED", sw.Elapsed);
        }
    }

    /// <inheritdoc />
    public T TrackDuration<T>(string metricId, Func<T> func)
    {
        var success = false;
        var sw = Stopwatch.StartNew();

        try
        {
            logger.LogTrace("{MetricId}: Starting...", metricId);
            var result = func();
            success = true;
            return result;
        }
        finally
        {
            sw.Stop();

            logger.LogDebug("{MetricId}: {Status}. Duration: {Duration}",
                metricId, success ? "Completed" : "FAILED", sw.Elapsed);
        }
    }
}
