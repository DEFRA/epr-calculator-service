using EPR.Calculator.Service.Function.Features.Calculator.Contexts;
using EPR.Calculator.Service.Function.Services.Telemetry;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;
using Microsoft.ApplicationInsights.DataContracts;

namespace EPR.Calculator.Service.Function.UnitTests.Services.Telemetry;

[TestCategory(TestCategories.Core)]
[TestClass]
public class TelemetryEventsTests
{
    [TestMethod]
    public void Init_Should_return_correct_event_name()
    {
        TelemetryEvents.Init().Name.ShouldBe("PayCalRunInit");
    }

    [TestMethod]
    public void InitFailed_Should_return_correct_event_name()
    {
        TelemetryEvents.InitFailed("msg").Name.ShouldBe("PayCalRunInitFailed");
    }

    [DataRow("raw message body")]
    [DataRow("{}")]
    [TestMethod]
    public void InitFailed_Should_set_service_bus_message(string message)
    {
        TelemetryEvents.InitFailed(message).Properties["ServiceBusMessage"].ShouldBe(message);
    }

    [TestMethod]
    public void Started_Should_return_correct_event_name()
    {
        TelemetryEvents.Started(TestFixtures.Legacy.Create<CalculatorRunContext>()).Name.ShouldBe("PayCalRunStarted");
    }

    [TestMethod]
    public void Started_Should_include_run_context_properties()
    {
        var result = TelemetryEvents.Started(TestFixtures.Legacy.Create<CalculatorRunContext>());
        result.Properties["RunId"].ShouldBe("1");
        result.Properties["RunName"].ShouldStartWith("Test Run");
        result.Properties["RunType"].ShouldBe("Calculator");
    }

    [TestMethod]
    public void Completed_Should_return_correct_event_name()
    {
        TelemetryEvents.Completed(TestFixtures.Legacy.Create<CalculatorRunContext>(), TimeSpan.Zero).Name.ShouldBe("PayCalRunCompleted");
    }

    [TestMethod]
    public void Completed_Should_include_run_context_properties()
    {
        var result = TelemetryEvents.Completed(TestFixtures.Legacy.Create<CalculatorRunContext>(), TimeSpan.Zero);
        result.Properties["RunId"].ShouldBe("1");
        result.Properties["RunName"].ShouldStartWith("Test Run");
        result.Properties["RunType"].ShouldBe("Calculator");
    }

    [TestMethod]
    public void Completed_Should_include_elapsed_ms()
    {
        var elapsed = TimeSpan.FromMilliseconds(1500);
        TelemetryEvents.Completed(TestFixtures.Legacy.Create<CalculatorRunContext>(), elapsed).Properties["ElapsedMs"].ShouldBe("1500");
    }

    [TestMethod]
    public void Failed_Should_return_correct_event_name()
    {
        TelemetryEvents.Failed(TestFixtures.Legacy.Create<CalculatorRunContext>(), TimeSpan.Zero, "reason").Name.ShouldBe("PayCalRunFailed");
    }

    [TestMethod]
    public void Failed_Should_include_run_context_properties()
    {
        var result = TelemetryEvents.Failed(TestFixtures.Legacy.Create<CalculatorRunContext>(), TimeSpan.Zero, "reason");
        result.Properties["RunId"].ShouldBe("1");
        result.Properties["RunName"].ShouldStartWith("Test Run");
        result.Properties["RunType"].ShouldBe("Calculator");
    }

    [TestMethod]
    public void Failed_Should_include_elapsed_ms()
    {
        var elapsed = TimeSpan.FromMilliseconds(2000);
        TelemetryEvents.Failed(TestFixtures.Legacy.Create<CalculatorRunContext>(), elapsed, "reason").Properties["ElapsedMs"].ShouldBe("2000");
    }

    [DataRow("ProcessingFailed")]
    [DataRow("UnhandledException")]
    [TestMethod]
    public void Failed_Should_set_failure_reason(string reason)
    {
        TelemetryEvents.Failed(TestFixtures.Legacy.Create<CalculatorRunContext>(), TimeSpan.Zero, reason).Properties["FailureReason"].ShouldBe(reason);
    }

    [TestMethod]
    public void WarnException_Should_set_severity_level()
    {
        TelemetryEvents.WarnException(new Exception()).SeverityLevel.ShouldBe(SeverityLevel.Warning);
    }

    [TestMethod]
    public void WarnException_Should_set_exception()
    {
        var exception = new Exception();
        TelemetryEvents.WarnException(exception).Exception.ShouldBeSameAs(exception);
    }

    [TestMethod]
    public void WarnException_Should_include_run_context_properties_when_provided()
    {
        var result = TelemetryEvents.WarnException(new Exception(), TestFixtures.Legacy.Create<CalculatorRunContext>());
        result.Properties["RunId"].ShouldBe("1");
        result.Properties["RunName"].ShouldStartWith("Test Run");
        result.Properties["RunType"].ShouldBe("Calculator");
    }

    [TestMethod]
    public void WarnException_Should_not_include_run_context_properties_when_null()
    {
        TelemetryEvents.WarnException(new Exception()).Properties.ShouldNotContainKey("RunId");
    }

    [TestMethod]
    public void ErrorException_Should_set_severity_level()
    {
        TelemetryEvents.ErrorException(new Exception()).SeverityLevel.ShouldBe(SeverityLevel.Error);
    }

    [TestMethod]
    public void ErrorException_Should_set_exception()
    {
        var exception = new Exception();
        TelemetryEvents.ErrorException(exception).Exception.ShouldBeSameAs(exception);
    }

    [TestMethod]
    public void ErrorException_Should_include_run_context_properties_when_provided()
    {
        var result = TelemetryEvents.ErrorException(new Exception(), TestFixtures.Legacy.Create<CalculatorRunContext>());
        result.Properties["RunId"].ShouldBe("1");
        result.Properties["RunName"].ShouldStartWith("Test Run");
        result.Properties["RunType"].ShouldBe("Calculator");
    }

    [TestMethod]
    public void ErrorException_Should_not_include_run_context_properties_when_null()
    {
        TelemetryEvents.ErrorException(new Exception()).Properties.ShouldNotContainKey("RunId");
    }

    [TestMethod]
    public void CriticalException_Should_set_severity_level()
    {
        TelemetryEvents.CriticalException(new Exception()).SeverityLevel.ShouldBe(SeverityLevel.Critical);
    }

    [TestMethod]
    public void CriticalException_Should_set_exception()
    {
        var exception = new Exception();
        TelemetryEvents.CriticalException(exception).Exception.ShouldBeSameAs(exception);
    }

    [TestMethod]
    public void CriticalException_Should_include_run_context_properties_when_provided()
    {
        var result = TelemetryEvents.CriticalException(new Exception(), TestFixtures.Legacy.Create<CalculatorRunContext>());
        result.Properties["RunId"].ShouldBe("1");
        result.Properties["RunName"].ShouldStartWith("Test Run");
        result.Properties["RunType"].ShouldBe("Calculator");
    }

    [TestMethod]
    public void CriticalException_Should_not_include_run_context_properties_when_null()
    {
        TelemetryEvents.CriticalException(new Exception()).Properties.ShouldNotContainKey("RunId");
    }
}