namespace EPR.Calculator.Service.Common.Logging
{
    using Microsoft.ApplicationInsights.DataContracts;

    /// <summary>
    /// Interface for wrapping telemetry client operations.
    /// </summary>
    public interface ITelemetryClientWrapper
    {
        /// <summary>
        /// Tracks a trace telemetry event.
        /// </summary>
        /// <param name="telemetry">The trace telemetry to track.</param>
        void TrackTrace(TraceTelemetry telemetry);

        /// <summary>
        /// Tracks an exception telemetry event.
        /// </summary>
        /// <param name="telemetry">The exception telemetry to track.</param>
        void TrackException(ExceptionTelemetry telemetry);
    }
}