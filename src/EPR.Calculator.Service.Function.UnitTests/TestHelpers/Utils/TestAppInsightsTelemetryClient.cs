using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Services.Telemetry;
using Microsoft.ApplicationInsights.DataContracts;

namespace EPR.Calculator.Service.Function.UnitTests.TestHelpers.Utils;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class TestAppInsightsTelemetryClient : ITelemetryClient
{
    private readonly List<EventTelemetry> TrackedEvents = [];

    public virtual void TrackEvent(EventTelemetry telemetry)
    {
        TrackedEvents.Add(telemetry);
    }

    public virtual void TrackDuration(string metricId, TimeSpan duration)
    {
    }

    public virtual async Task TrackDuration(string metricId, Func<Task> func)
    {
        await func();
    }

    public virtual async Task<T> TrackDuration<T>(string metricId, Func<Task<T>> func)
    {
        return await func();
    }

    public virtual T TrackDuration<T>(string metricId, Func<T> func)
    {
        return func();
    }

    public void VerifyEventTracked(string eventName)
    {
        TrackedEvents.ShouldContain(e =>
            e.Name == eventName
            && e.Properties.Count == 0);
    }

    public void VerifyEventTracked(string eventName, RunContext runContext)
    {
        var expectedProperties = runContext.Summary.Select(p => (p.Key, p.Value?.ToString() ?? "")).ToArray();
        VerifyEventTracked(eventName, expectedProperties);
    }

    public void VerifyEventTracked(string eventName, IList<(string Key, string Value)> expectedProperties)
    {
        TrackedEvents.ShouldContain(e => e.Name == eventName, $"Tacked event '{eventName}' not found");

        TrackedEvents.ShouldContain(e =>
            e.Name == eventName
            && expectedProperties.All(p =>
                e.Properties.ContainsKey(p.Key)
                && e.Properties[p.Key] == p.Value),
            $"Tacked event '{eventName}' was found, but event properties did not match [{string.Join(", ", expectedProperties.Select(p => $"{p.Key}: {p.Value}"))}].");
    }
}
