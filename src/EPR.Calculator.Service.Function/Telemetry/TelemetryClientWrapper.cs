using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace EPR.Calculator.Service.Function.Telemetry
{
    /// <summary>
    ///     Abstraction over <see cref="Microsoft.ApplicationInsights.TelemetryClient"/>
    ///     to allow telemetry to be verified in unit tests.
    /// </summary>
    public interface ITelemetryClient
    {
        void TrackEvent(EventTelemetry telemetry);
        void TrackException(ExceptionTelemetry telemetry);
    }

    public class TelemetryClientWrapper(TelemetryClient client) : ITelemetryClient
    {
        public void TrackEvent(EventTelemetry telemetry) => client.TrackEvent(telemetry);
        public void TrackException(ExceptionTelemetry telemetry) => client.TrackException(telemetry);
    }
}