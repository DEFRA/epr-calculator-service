using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Features.Calculator;
using EPR.Calculator.Service.Function.Features.Calculator.Contexts;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.Service.Function.UnitTests.Features.Calculator;

[TestCategory(TestCategories.CalculatorRuns)]
[TestClass]
public class CalculatorRunInitializerTests
{
    private IFixture _fixture = null!;
    private CalculatorRunContext _runContext = null!;
    private CalculatorRunInitializer _sut = null!;
    private Mock<ITransposePomAndOrgDataService> _transposer = null!;
    private ApplicationDBContext _dbContext;

    [TestInitialize]
    public void Init()
    {
        _fixture = TestFixtures.New(o => o.IncludeSeedData());
        _dbContext = _fixture.Freeze<ApplicationDBContext>();
        _runContext = _fixture.Create<CalculatorRunContext>();
        _transposer = _fixture.Freeze<Mock<ITransposePomAndOrgDataService>>();

        _sut = _fixture.Create<CalculatorRunInitializer>();
    }

    [TestMethod]
    public async Task Should_initialize()
    {
        var calculatorRun = _dbContext.CalculatorRuns.Single(cr => cr.Id == _runContext.RunId);
        calculatorRun.CalculatorRunClassificationId = RunClassificationStatusIds.INTHEQUEUEID;
        await _dbContext.SaveChangesAsync();

        await _sut.Initialize(_runContext, CancellationToken.None);

        calculatorRun.CalculatorRunClassificationId.ShouldBe(RunClassificationStatusIds.RUNNINGID);
    }

    [TestMethod]
    public async Task Should_handle_failure()
    {
        _transposer.Setup(dl => dl.Transpose(_runContext, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test failure"));

        await Should.ThrowAsync<Exception>(() => _sut.Initialize(_runContext, CancellationToken.None));
    }
}