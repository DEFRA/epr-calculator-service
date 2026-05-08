using Microsoft.ApplicationInsights.DataContracts;

namespace EPR.Calculator.Service.Function.Services.Telemetry;

/// <summary>
///     Abstraction over <see cref="Microsoft.ApplicationInsights.TelemetryClient" />.
/// </summary>
public interface ITelemetryClient
{
    /// <summary>
    ///     Track a telemetry event. These are generally used for business purposes rather than metrics/logging.
    /// </summary>
    void TrackEvent(EventTelemetry telemetry);

    void TrackDuration(string metricId, TimeSpan duration);

    /// <summary>
    ///     Runs the supplied task and measures the duration it takes to complete.
    ///     The duration is recorded as a telemetry metric for the input <c>metricId</c>.
    /// </summary>
    Task TrackDuration(string metricId, Func<Task> func);

    /// <summary>
    ///     Runs the supplied task, returning its result, and measures the duration it takes to complete.
    ///     The duration is recorded as a telemetry metric for the input <c>metricId</c>.
    /// </summary>
    Task<T> TrackDuration<T>(string metricId, Func<Task<T>> func);

    /// <summary>
    ///     Runs the supplied task, returning its result, and measures the duration it takes to complete.
    ///     The duration is recorded as a telemetry metric for the input <c>metricId</c>.
    /// </summary>
    T TrackDuration<T>(string metricId, Func<T> func);
}
