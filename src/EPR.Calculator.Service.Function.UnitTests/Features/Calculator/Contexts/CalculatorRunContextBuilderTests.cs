using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Exceptions;
using EPR.Calculator.Service.Function.Features.Calculator.Contexts;
using EPR.Calculator.Service.Function.Messaging;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.Service.Function.UnitTests.Features.Calculator.Contexts;

[TestCategory(TestCategories.CalculatorRuns)]
[TestClass]
public class CalculatorRunContextBuilderTests
{
    private ApplicationDBContext _dbContext = null!;
    private IFixture _fixture = null!;
    private CalculatorRunContextBuilder _sut = null!;
    private FakeTimeProvider _timeProvider = null!;

    [TestInitialize]
    public void Init()
    {
        _fixture = TestFixtures.New(o => o.IncludeSeedData());
        _dbContext = _fixture.Freeze<ApplicationDBContext>();
        _timeProvider = (FakeTimeProvider) _fixture.Freeze<TimeProvider>();

        _sut = _fixture.Create<CalculatorRunContextBuilder>();
    }


    [TestMethod]
    public async Task Should_create_valid_context()
    {
        var now = new DateTimeOffset(2025, 11, 13, 12, 0, 0, TimeSpan.Zero);
        _timeProvider.SetUtcNow(now);

        var message = new CreateResultFileMessage
        {
            CalculatorRunId = DbSeedData.Defaults.ValidCalculatorRun.Id,
            CreatedBy = "Test User"
        };

        var runContext = await _sut.CreateContext(message, CancellationToken.None);
        runContext.RunId.ShouldBe(DbSeedData.Defaults.ValidCalculatorRun.Id);
        runContext.RunName.ShouldBe(DbSeedData.Defaults.ValidCalculatorRun.Name);
        runContext.RelativeYear.ShouldBe(DbSeedData.Defaults.ValidCalculatorRun.RelativeYear);
        runContext.User.ShouldBe(message.CreatedBy);
        runContext.ProcessingStartedAt.ShouldBe(now);
    }

    [TestMethod]
    public async Task Should_throw_when_run_not_found()
    {
        var message = new CreateResultFileMessage
        {
            CalculatorRunId = -1,
            CreatedBy = "ignored"
        };

        var exception = await Should.ThrowAsync<RunContextException>(() =>
            _sut.CreateContext(message, CancellationToken.None));

        exception.Message.ShouldContain("Run not found");
    }

    [TestMethod]
    public async Task Should_throw_when_run_fails_validation()
    {
        var invalidCalculatorRun = DbSeedData.Defaults.ValidCalculatorRun;
        invalidCalculatorRun.Id = 3;
        invalidCalculatorRun.CalculatorRunClassificationId = RunClassificationStatusIds.UNCLASSIFIEDID;

        var message = new CreateResultFileMessage
        {
            CalculatorRunId = invalidCalculatorRun.Id,
            CreatedBy = "ignored"
        };

        await _dbContext.CalculatorRuns.AddAsync(invalidCalculatorRun);
        await _dbContext.SaveChangesAsync();

        var exception = await Should.ThrowAsync<RunContextException>(() =>
            _sut.CreateContext(message, CancellationToken.None));

        exception.Message.ShouldContain("Run classification");
    }
}