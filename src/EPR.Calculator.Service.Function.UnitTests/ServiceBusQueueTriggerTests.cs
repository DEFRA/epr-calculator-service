using EPR.Calculator.Service.Function.Features.Billing;
using EPR.Calculator.Service.Function.Features.Billing.Contexts;
using EPR.Calculator.Service.Function.Features.Calculator;
using EPR.Calculator.Service.Function.Features.Calculator.Contexts;
using EPR.Calculator.Service.Function.Messaging;
using EPR.Calculator.Service.Function.Services.Telemetry;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Utils;
using Microsoft.Extensions.Logging;

namespace EPR.Calculator.Service.Function.UnitTests;

[TestCategory(TestCategories.Core)]
[TestClass]
public class ServiceBusQueueTriggerTests
{
    private Mock<IBillingRunContextBuilder> _billingRunContextBuilder = null!;
    private Mock<IBillingRunProcessor> _billingRunProcessor = null!;
    private Mock<ICalculatorRunContextBuilder> _calculatorRunContextBuilder = null!;
    private Mock<ICalculatorRunProcessor> _calculatorRunProcessor = null!;
    private IFixture _fixture = null!;
    private Mock<ILogger<ServiceBusQueueTrigger>> _logger = null!;
    private Mock<IMessageTypeService> _messageTypeService = null!;
    private ServiceBusQueueTrigger _sut = null!;
    private Mock<ITelemetryClient> _telemetry = null!;

    [TestInitialize]
    public void Init()
    {
        _fixture = TestFixtures.New();
        _messageTypeService = _fixture.Freeze<Mock<IMessageTypeService>>();
        _telemetry = _fixture.Freeze<Mock<ITelemetryClient>>();
        _billingRunContextBuilder = _fixture.Freeze<Mock<IBillingRunContextBuilder>>();
        _billingRunProcessor = _fixture.Freeze<Mock<IBillingRunProcessor>>();
        _calculatorRunContextBuilder = _fixture.Freeze<Mock<ICalculatorRunContextBuilder>>();
        _calculatorRunProcessor = _fixture.Freeze<Mock<ICalculatorRunProcessor>>();
        _logger = _fixture.Freeze<Mock<ILogger<ServiceBusQueueTrigger>>>();

        _messageTypeService
            .Setup(svc => svc.DeserializeMessage(It.IsAny<string>()))
            .Returns(new CreateResultFileMessage
            {
                CalculatorRunId = 1,
                CreatedBy = "Test User"
            });

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
        var message = new CreateResultFileMessage { CalculatorRunId = 1, CreatedBy = "Test User" };

        _messageTypeService
            .Setup(svc => svc.DeserializeMessage(It.IsAny<string>()))
            .Returns(message);

        var runContext = _fixture.Create<CalculatorRunContext>();

        _calculatorRunContextBuilder
            .Setup(b => b.CreateContext(message, It.IsAny<CancellationToken>()))
            .ReturnsAsync(runContext);

        await Should.NotThrowAsync(() => _sut.Run("ignored", CancellationToken.None));
        _calculatorRunContextBuilder.Verify(b => b.CreateContext(message, It.IsAny<CancellationToken>()), Times.Once);
        _calculatorRunProcessor.Verify(p => p.Process(runContext, It.IsAny<CancellationToken>()), Times.Once);
        _telemetry.VerifyEventTracked("PayCalRunStarted", runContext);
        _telemetry.VerifyEventTracked("PayCalRunCompleted", runContext);
    }

    [TestMethod]
    public async Task Should_handle_valid_billing_message()
    {
        var message = new CreateBillingFileMessage { CalculatorRunId = 1, ApprovedBy = "Test User" };

        _messageTypeService
            .Setup(svc => svc.DeserializeMessage(It.IsAny<string>()))
            .Returns(message);

        var runContext = _fixture.Create<BillingRunContext>();

        _billingRunContextBuilder
            .Setup(b => b.CreateContext(message, It.IsAny<CancellationToken>()))
            .ReturnsAsync(runContext);

        await Should.NotThrowAsync(() => _sut.Run("ignored", CancellationToken.None));
        _billingRunContextBuilder.Verify(b => b.CreateContext(message, It.IsAny<CancellationToken>()), Times.Once);
        _billingRunProcessor.Verify(p => p.Process(runContext, It.IsAny<CancellationToken>()), Times.Once);
        _telemetry.VerifyEventTracked("PayCalRunStarted", runContext);
        _telemetry.VerifyEventTracked("PayCalRunCompleted", runContext);
    }

    [TestMethod]
    public async Task Should_track_event_RunInit()
    {
        await _sut.Run("ignored", CancellationToken.None);
        _telemetry.VerifyEventTracked("PayCalRunInit");
    }

    [TestMethod]
    public async Task Should_handle_deserialize_exception_as_RunInitFailed()
    {
        _messageTypeService
            .Setup(svc => svc.DeserializeMessage(It.IsAny<string>()))
            .Throws(new Exception("Deserialization failed"));

        var thrown = await Should.ThrowAsync<Exception>(() => _sut.Run("{foo}", CancellationToken.None));

        _telemetry.VerifyExceptionTracked(thrown);
        _telemetry.VerifyEventTracked("PayCalRunInitFailed", [("ServiceBusMessage", "{foo}")]);
    }

    [TestMethod]
    public async Task Should_handle_invalid_message_type_as_RunInitFailed()
    {
        _messageTypeService
            .Setup(svc => svc.DeserializeMessage(It.IsAny<string>()))
            .Returns(new InvalidMessageType{CalculatorRunId = 99});

        var thrown = await Should.ThrowAsync<ArgumentException>(() => _sut.Run("{foo}", CancellationToken.None));

        _telemetry.VerifyExceptionTracked(thrown);
        _telemetry.VerifyEventTracked("PayCalRunInitFailed", [("ServiceBusMessage", "{foo}")]);
    }

    [TestMethod]
    public async Task Should_handle_context_exception_as_RunInitFailed()
    {
        var exception = new Exception("Context build failed");

        _messageTypeService
            .Setup(svc => svc.DeserializeMessage(It.IsAny<string>()))
            .Returns(new CreateResultFileMessage
            {
                CalculatorRunId = 1,
                CreatedBy = "Test User"
            });

        _calculatorRunContextBuilder
            .Setup(b => b.CreateContext(It.IsAny<CreateResultFileMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        await Should.ThrowAsync<Exception>(() => _sut.Run("{foo}", CancellationToken.None));

        _telemetry.VerifyExceptionTracked(exception);
        _telemetry.VerifyEventTracked("PayCalRunInitFailed", [("ServiceBusMessage", "{foo}")]);
    }

    [TestMethod]
    public async Task Should_handle_processor_false()
    {
        var runContext = _fixture.Create<CalculatorRunContext>();

        _calculatorRunContextBuilder
            .Setup(b => b.CreateContext(It.IsAny<CreateResultFileMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(runContext);

        _calculatorRunProcessor
            .Setup(p => p.Process(It.IsAny<CalculatorRunContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await Should.NotThrowAsync(() => _sut.Run("ignored", CancellationToken.None));

        _telemetry.VerifyEventTracked("PayCalRunFailed", runContext);
    }

    [TestMethod]
    public async Task Should_handle_processor_exception()
    {
        var exception = new Exception("Unexpected failure");

        _calculatorRunProcessor
            .Setup(p => p.Process(It.IsAny<CalculatorRunContext>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        await Should.NotThrowAsync(() => _sut.Run("ignored", CancellationToken.None));

        _telemetry.VerifyExceptionTracked(exception);
        _logger.VerifyLogContains(LogLevel.Critical, "unhandled");
    }

    private record InvalidMessageType : MessageBase;
}