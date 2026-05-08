using EPR.Calculator.Service.Function.Exceptions;
using EPR.Calculator.Service.Function.Features.Billing;
using EPR.Calculator.Service.Function.Features.Billing.Contexts;
using EPR.Calculator.Service.Function.Features.Calculator;
using EPR.Calculator.Service.Function.Features.Calculator.Contexts;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Utils;
using Microsoft.Extensions.Logging;

namespace EPR.Calculator.Service.Function.UnitTests;

[TestCategory(TestCategories.Core)]
[TestClass]
public class ServiceBusQueueTriggerTests
{
    private readonly ServiceBusQueueTrigger.ServiceBusMessage _defaultMessage = new() { RunId = 1, User = "Test", RunType = RunType.Calculator };
    private Mock<IBillingRunContextBuilder> _billingRunContextBuilder = null!;
    private Mock<IBillingRunProcessor> _billingRunProcessor = null!;
    private Mock<ICalculatorRunContextBuilder> _calculatorRunContextBuilder = null!;
    private Mock<ICalculatorRunProcessor> _calculatorRunProcessor = null!;
    private IFixture _fixture = null!;
    private Mock<ILogger<ServiceBusQueueTrigger>> _logger = null!;
    private ServiceBusQueueTrigger _sut = null!;
    private TestAppInsightsTelemetryClient _appInsightsTelemetry = null!;

    [TestInitialize]
    public void Init()
    {
        _fixture = TestFixtures.New();
        _appInsightsTelemetry = _fixture.Freeze<TestAppInsightsTelemetryClient>();
        _billingRunContextBuilder = _fixture.Freeze<Mock<IBillingRunContextBuilder>>();
        _billingRunProcessor = _fixture.Freeze<Mock<IBillingRunProcessor>>();
        _calculatorRunContextBuilder = _fixture.Freeze<Mock<ICalculatorRunContextBuilder>>();
        _calculatorRunProcessor = _fixture.Freeze<Mock<ICalculatorRunProcessor>>();
        _logger = _fixture.Freeze<Mock<ILogger<ServiceBusQueueTrigger>>>();

        _calculatorRunProcessor
            .Setup(p => p.Process(It.IsAny<CalculatorRunContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _billingRunProcessor
            .Setup(p => p.Process(It.IsAny<BillingRunContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _sut = _fixture.Create<ServiceBusQueueTrigger>();
    }

    [TestMethod]
    public async Task Should_handle_valid_calculator_message()
    {
        var message = _defaultMessage;
        var runContext = _fixture.Create<CalculatorRunContext>();

        _calculatorRunContextBuilder
            .Setup(b => b.CreateContext(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(runContext);

        await Should.NotThrowAsync(() => _sut.Run(message, CancellationToken.None));
        _calculatorRunContextBuilder.Verify(b => b.CreateContext(message.RunId, message.User, It.IsAny<CancellationToken>()), Times.Once);
        _calculatorRunProcessor.Verify(p => p.Process(runContext, It.IsAny<CancellationToken>()), Times.Once);
        _appInsightsTelemetry.VerifyEventTracked("RunStarted", runContext);
        _appInsightsTelemetry.VerifyEventTracked("RunCompleted", runContext);
    }

    [TestMethod]
    public async Task Should_handle_valid_billing_message()
    {
        var message = _defaultMessage with { RunType = RunType.Billing };
        var runContext = _fixture.Create<BillingRunContext>();

        _billingRunContextBuilder
            .Setup(b => b.CreateContext(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(runContext);

        await Should.NotThrowAsync(() => _sut.Run(message, CancellationToken.None));
        _billingRunContextBuilder.Verify(b => b.CreateContext(message.RunId, message.User, It.IsAny<CancellationToken>()), Times.Once);
        _billingRunProcessor.Verify(p => p.Process(runContext, It.IsAny<CancellationToken>()), Times.Once);
        _appInsightsTelemetry.VerifyEventTracked("RunStarted", runContext);
        _appInsightsTelemetry.VerifyEventTracked("RunCompleted", runContext);
    }

    [TestMethod]
    public async Task Should_track_event_RunInit()
    {
        var message = _defaultMessage;
        await _sut.Run(message, CancellationToken.None);
        _appInsightsTelemetry.VerifyEventTracked("RunInit");
    }

    [TestMethod]
    public async Task Should_handle_invalid_message_type_as_RunInitFailed()
    {
        var message = _defaultMessage with { RunType = RunType.Unknown };

        await Should.NotThrowAsync(() => _sut.Run(message, CancellationToken.None));
        _appInsightsTelemetry.VerifyEventTracked("RunInitFailed", [("ServiceBusMessage", message.ToString())]);
    }

    [TestMethod]
    public async Task Should_handle_context_exception_as_RunInitFailed()
    {
        var message = _defaultMessage;
        var exception = new RunContextException(RunType.Calculator, message.RunId, "Context build failed");

        _calculatorRunContextBuilder
            .Setup(b => b.CreateContext(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        await Should.NotThrowAsync(() => _sut.Run(message, CancellationToken.None));

        _appInsightsTelemetry.VerifyEventTracked("RunInitFailed", [("ServiceBusMessage", message.ToString())]);
    }

    [TestMethod]
    public async Task Should_handle_processor_false()
    {
        var message = _defaultMessage;
        var runContext = _fixture.Create<CalculatorRunContext>();

        _calculatorRunContextBuilder
            .Setup(b => b.CreateContext(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(runContext);

        _calculatorRunProcessor
            .Setup(p => p.Process(It.IsAny<CalculatorRunContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await Should.NotThrowAsync(() => _sut.Run(message, CancellationToken.None));

        _appInsightsTelemetry.VerifyEventTracked("RunFailed", runContext);
    }

    [TestMethod]
    public async Task Should_handle_processor_exception()
    {
        var message = _defaultMessage;
        var exception = new Exception("Unexpected failure");

        _calculatorRunProcessor
            .Setup(p => p.Process(It.IsAny<CalculatorRunContext>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        await Should.NotThrowAsync(() => _sut.Run(message, CancellationToken.None));

        _logger.VerifyLogContains(LogLevel.Critical, "unhandled");
    }
}
