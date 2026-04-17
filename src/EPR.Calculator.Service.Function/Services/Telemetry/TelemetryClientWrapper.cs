using System.Diagnostics.CodeAnalysis;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace EPR.Calculator.Service.Function.Services.Telemetry;

/// <summary>
///     Abstraction over <see cref="Microsoft.ApplicationInsights.TelemetryClient" />
///     to allow telemetry to be verified in unit tests.
/// </summary>
public interface ITelemetryClient
{
    void Track(EventTelemetry telemetry);
    void Track(ExceptionTelemetry telemetry);
}

[ExcludeFromCodeCoverage]
public class TelemetryClientWrapper(TelemetryClient client) : ITelemetryClient
{
    public void Track(EventTelemetry telemetry)
    {
        client.TrackEvent(telemetry);
    }

    public void Track(ExceptionTelemetry telemetry)
    {
        client.TrackException(telemetry);
    }
}