using EPR.Calculator.Service.Function.Builder;
using EPR.Calculator.Service.Function.Features.Billing;
using EPR.Calculator.Service.Function.Features.Billing.Contexts;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Services.Telemetry;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Utils;
using Microsoft.Extensions.Logging;

namespace EPR.Calculator.Service.Function.UnitTests.Features.Billing;

[TestCategory(TestCategories.BillingRuns)]
[TestClass]
public class BillingRunProcessorTests
{
    private Mock<ICalcResultBuilder> _builder = null!;
    private IFixture _fixture = null!;
    private Mock<ILogger<BillingRunProcessor>> _logger = null!;
    private BillingRunContext _runContext = null!;
    private BillingRunProcessor _sut = null!;
    private Mock<ITelemetryClient> _telemetry = null!;

    [TestInitialize]
    public void Init()
    {
        _fixture = TestFixtures.New();
        _runContext = _fixture.Create<BillingRunContext>();
        _builder = _fixture.Freeze<Mock<ICalcResultBuilder>>();
        _telemetry = _fixture.Freeze<Mock<ITelemetryClient>>();
        _logger = _fixture.Freeze<Mock<ILogger<BillingRunProcessor>>>();

        _sut = _fixture.Create<BillingRunProcessor>();
    }

    [TestMethod]
    public async Task Should_handle_success()
    {
        var result = await _sut.Process(_runContext, CancellationToken.None);

        result.ShouldBeTrue();
    }

    [TestMethod]
    public async Task Should_handle_cancelled()
    {
        var exception = new OperationCanceledException("Test cancelled");
        _builder.Setup(b => b.BuildAsync(It.IsAny<RunContext>())).ThrowsAsync(exception);

        var result = await _sut.Process(_runContext, CancellationToken.None);

        result.ShouldBeFalse();
        _telemetry.VerifyExceptionTracked(exception);
        _logger.VerifyLogContains(LogLevel.Error, "timeout");
    }

    [TestMethod]
    public async Task Should_handle_failure()
    {
        var exception = new Exception("Test failure");
        _builder.Setup(b => b.BuildAsync(It.IsAny<RunContext>())).ThrowsAsync(exception);

        var result = await _sut.Process(_runContext, CancellationToken.None);

        result.ShouldBeFalse();
        _telemetry.VerifyExceptionTracked(exception);
        _logger.VerifyLogContains(LogLevel.Error, "failed");
    }
}