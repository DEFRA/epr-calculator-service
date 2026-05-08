using EPR.Calculator.Service.Function.Features.Calculator.Contexts;
using EPR.Calculator.Service.Function.Services.Telemetry.Helpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;

namespace EPR.Calculator.Service.Function.UnitTests.Services.Telemetry;

[TestCategory(TestCategories.Core)]
[TestClass]
public class TelemetryEventsTests
{
    private CalculatorRunContext _runContext = null!;

    [TestInitialize]
    public void Init()
    {
        _runContext = new CalculatorRunContext { RunId = 1, RelativeYear = 2025, RunName = "Test Run", User = "Test User", ProcessingStartedAt = DateTime.Parse("2026-01-01")};
    }

    [TestMethod]
    public void Init_Should_return_correct_event_name()
    {
        TelemetryEvents.RunInit().Name.ShouldBe("RunInit");
    }

    [TestMethod]
    public void InitFailed_Should_return_correct_event_name()
    {
        TelemetryEvents.RunInitFailed("msg").Name.ShouldBe("RunInitFailed");
    }

    [DataRow("raw message body")]
    [DataRow("{}")]
    [TestMethod]
    public void InitFailed_Should_set_service_bus_message(string message)
    {
        TelemetryEvents.RunInitFailed(message).Properties["ServiceBusMessage"].ShouldBe(message);
    }

    [TestMethod]
    public void Started_Should_return_correct_event_name()
    {
        TelemetryEvents.RunStarted(_runContext).Name.ShouldBe("RunStarted");
    }

    [TestMethod]
    public void Started_Should_include_run_context_properties()
    {
        var result = TelemetryEvents.RunStarted(_runContext);
        result.Properties["RunId"].ShouldBe("1");
        result.Properties["RunName"].ShouldStartWith("Test Run");
        result.Properties["RunType"].ShouldBe("Calculator");
    }

    [TestMethod]
    public void Completed_Should_return_correct_event_name()
    {
        TelemetryEvents.RunCompleted(_runContext, TimeSpan.Zero).Name.ShouldBe("RunCompleted");
    }

    [TestMethod]
    public void Completed_Should_include_run_context_properties()
    {
        var result = TelemetryEvents.RunCompleted(_runContext, TimeSpan.Zero);
        result.Properties["RunId"].ShouldBe("1");
        result.Properties["RunName"].ShouldStartWith("Test Run");
        result.Properties["RunType"].ShouldBe("Calculator");
    }

    [TestMethod]
    public void Completed_Should_include_duration()
    {
        var elapsed = TimeSpan.FromMilliseconds(1500);
        TelemetryEvents.RunCompleted(_runContext, elapsed).Properties["DurationMs"].ShouldBe("1500");
    }

    [TestMethod]
    public void Failed_Should_return_correct_event_name()
    {
        TelemetryEvents.RunFailed(_runContext, TimeSpan.Zero, "reason").Name.ShouldBe("RunFailed");
    }

    [TestMethod]
    public void Failed_Should_include_run_context_properties()
    {
        var result = TelemetryEvents.RunFailed(_runContext, TimeSpan.Zero, "reason");
        result.Properties["RunId"].ShouldBe("1");
        result.Properties["RunName"].ShouldStartWith("Test Run");
        result.Properties["RunType"].ShouldBe("Calculator");
    }

    [TestMethod]
    public void Failed_Should_include_duration()
    {
        var elapsed = TimeSpan.FromMilliseconds(2000);
        TelemetryEvents.RunFailed(_runContext, elapsed, "reason").Properties["DurationMs"].ShouldBe("2000");
    }

    [DataRow("ProcessingFailed")]
    [DataRow("UnhandledException")]
    [TestMethod]
    public void Failed_Should_set_failure_reason(string reason)
    {
        TelemetryEvents.RunFailed(_runContext, TimeSpan.Zero, reason).Properties["FailureReason"].ShouldBe(reason);
    }
}
