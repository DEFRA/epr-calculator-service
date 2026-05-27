using EPR.Calculator.Service.Function.Builder;
using EPR.Calculator.Service.Function.Features.CalculatorRun;
using EPR.Calculator.Service.Function.Features.CalculatorRun.Contexts;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Utils;
using Microsoft.Extensions.Logging;

namespace EPR.Calculator.Service.Function.UnitTests.Features.Calculator;

[TestCategory(TestCategories.CalculatorRuns)]
[TestClass]
public class CalculatorRunProcessorTests
{
    private Mock<ICalcResultBuilder> builder = null!;
    private IFixture fixture = null!;
    private Mock<ILogger<CalculatorRunProcessor>> logger = null!;
    private CalculatorRunContext runContext = null!;
    private CalculatorRunProcessor sut = null!;

    [TestInitialize]
    public void Init()
    {
        fixture = TestFixtures.New();
        runContext = fixture.Create<CalculatorRunContext>();
        builder = fixture.Freeze<Mock<ICalcResultBuilder>>();
        logger = fixture.Freeze<Mock<ILogger<CalculatorRunProcessor>>>();

        sut = fixture.Create<CalculatorRunProcessor>();
    }

    [TestMethod]
    public async Task Should_handle_success()
    {
        var result = await sut.Process(runContext, CancellationToken.None);

        result.ShouldBeTrue();
    }

    [TestMethod]
    public async Task Should_handle_cancelled()
    {
        var exception = new OperationCanceledException("Test cancelled");
        builder.Setup(b => b.BuildAsync(It.IsAny<RunContext>())).ThrowsAsync(exception);

        var result = await sut.Process(runContext, CancellationToken.None);

        result.ShouldBeFalse();
        logger.VerifyLogContains(LogLevel.Error, "cancellation");
    }

    [TestMethod]
    public async Task Should_handle_failure()
    {
        var exception = new Exception("Test failure");
        builder.Setup(b => b.BuildAsync(It.IsAny<RunContext>())).ThrowsAsync(exception);

        var result = await sut.Process(runContext, CancellationToken.None);

        result.ShouldBeFalse();
        logger.VerifyLogContains(LogLevel.Error, "failed");
    }
}
