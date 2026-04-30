using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Services.Telemetry;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;

namespace EPR.Calculator.Service.Function.UnitTests.TestHelpers.Utils;

public static class Verification
{
    public static void VerifyLogContains<T>(this Mock<ILogger<T>> logger, LogLevel level, string fragment)
    {
        logger.Verify(l => l.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, _) => o.ToString()!.Contains(fragment, StringComparison.OrdinalIgnoreCase)),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce(),
            $"Log message should contain '{fragment}'.");
    }

    public static void VerifyEventTracked(this Mock<ITelemetryClient> telemetry, string eventName)
    {
        telemetry.Verify(t => t.Track(
            It.Is<EventTelemetry>(e =>
                e.Name == eventName
                && e.Properties.Count == 0)));
    }

    public static void VerifyEventTracked(this Mock<ITelemetryClient> telemetry, string eventName, RunContext runContext)
    {
        var expectedProperties = runContext.Summary.Select(p => (p.Key, p.Value?.ToString() ?? "")).ToArray();
        telemetry.VerifyEventTracked(eventName, expectedProperties);
    }

    public static void VerifyEventTracked(this Mock<ITelemetryClient> telemetry, string eventName, IList<(string Key, string Value)> expectedProperties)
    {
        telemetry.Verify(t => t.Track(
            It.Is<EventTelemetry>(e =>
                e.Name == eventName
                && expectedProperties.All(p =>
                    e.Properties.ContainsKey(p.Key)
                    && e.Properties[p.Key] == p.Value))));
    }

    public static void VerifyExceptionTracked(this Mock<ITelemetryClient> telemetry, Exception exception)
    {
        telemetry.Verify(t => t.Track(
            It.Is<ExceptionTelemetry>(e => e.Exception == exception)));
    }
}