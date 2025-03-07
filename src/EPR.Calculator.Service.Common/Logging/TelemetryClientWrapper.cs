namespace EPR.Calculator.Service.Common.Logging
{
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DataContracts;

    /// <summary>
    /// Wrapper class for TelemetryClient to track traces and exceptions.
    /// </summary>
    public class TelemetryClientWrapper : ITelemetryClientWrapper
    {
        private readonly TelemetryClient telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryClientWrapper"/> class.
        /// </summary>
        /// <param name="telemetryClient">The telemetry client instance.</param>
        public TelemetryClientWrapper(TelemetryClient telemetryClient)
        {
            this.telemetryClient = telemetryClient;
        }

        /// <summary>
        /// Tracks a trace telemetry.
        /// </summary>
        /// <param name="telemetry">The trace telemetry to track.</param>
        public void TrackTrace(TraceTelemetry telemetry)
        {
            this.telemetryClient.TrackTrace(telemetry);
        }

        /// <summary>
        /// Tracks an exception telemetry.
        /// </summary>
        /// <param name="telemetry">The exception telemetry to track.</param>
        public void TrackException(ExceptionTelemetry telemetry)
        {
            this.telemetryClient.TrackException(telemetry);
        }
    }
}