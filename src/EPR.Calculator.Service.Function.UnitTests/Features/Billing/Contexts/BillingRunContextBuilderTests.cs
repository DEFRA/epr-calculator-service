using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Exceptions;
using EPR.Calculator.Service.Function.Features.Billing.Contexts;
using EPR.Calculator.Service.Function.Messaging;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.Service.Function.UnitTests.Features.Billing.Contexts;

[TestCategory(TestCategories.BillingRuns)]
[TestClass]
public class BillingRunContextBuilderTests
{
    private ApplicationDBContext _dbContext = null!;
    private IFixture _fixture = null!;
    private BillingRunContextBuilder _sut = null!;
    private FakeTimeProvider _timeProvider = null!;

    [TestInitialize]
    public void Init()
    {
        _fixture = TestFixtures.New(o => o.IncludeSeedData());
        _dbContext = _fixture.Freeze<ApplicationDBContext>();
        _timeProvider = (FakeTimeProvider)_fixture.Freeze<TimeProvider>();

        _sut = _fixture.Create<BillingRunContextBuilder>();
    }


    [TestMethod]
    public async Task Should_create_valid_context()
    {
        var now = new DateTimeOffset(2025, 11, 13, 12, 0, 0, TimeSpan.Zero);
        _timeProvider.SetUtcNow(now);

        var message = new CreateBillingFileMessage
        {
            CalculatorRunId = DbSeedData.Defaults.ValidBillingRun.Id,
            ApprovedBy = "Test User"
        };

        var runContext = await _sut.CreateContext(message, CancellationToken.None);
        runContext.RunId.ShouldBe(DbSeedData.Defaults.ValidBillingRun.Id);
        runContext.RunName.ShouldBe(DbSeedData.Defaults.ValidBillingRun.Name);
        runContext.RelativeYear.ShouldBe(DbSeedData.Defaults.ValidBillingRun.RelativeYear);
        runContext.User.ShouldBe(message.ApprovedBy);
        runContext.ProcessingStartedAt.ShouldBe(now);
        runContext.AcceptedProducerIds.ShouldBe([1]);
    }

    [TestMethod]
    public async Task Should_throw_when_run_not_found()
    {
        var message = new CreateBillingFileMessage
        {
            CalculatorRunId = -1,
            ApprovedBy = "ignored"
        };

        var exception = await Should.ThrowAsync<RunContextException>(() =>
            _sut.CreateContext(message, CancellationToken.None));

        exception.Message.ShouldContain("Run not found");
    }

    [TestMethod]
    public async Task Should_throw_when_run_fails_validation()
    {
        var invalidRun = DbSeedData.Defaults.ValidBillingRun;
        invalidRun.Id = 3;
        invalidRun.IsBillingFileGenerating = false;

        await _dbContext.CalculatorRuns.AddAsync(invalidRun);
        await _dbContext.SaveChangesAsync();

        var message = new CreateBillingFileMessage
        {
            CalculatorRunId = DbSeedData.Defaults.ValidCalculatorRunContext.RunId,
            ApprovedBy = "ignored"
        };

        var exception = await Should.ThrowAsync<RunContextException>(() =>
            _sut.CreateContext(message, CancellationToken.None));

        exception.Message.ShouldContain("Run not in valid state");
    }

    [TestMethod]
    public async Task Should_throw_when_no_producers_accepted()
    {
        var invalidRun = DbSeedData.Defaults.ValidBillingRun;
        invalidRun.Id = 4;
        invalidRun.ProducerResultFileSuggestedBillingInstruction.Clear();

        await _dbContext.CalculatorRuns.AddAsync(invalidRun);
        await _dbContext.SaveChangesAsync();

        var message = new CreateBillingFileMessage
        {
            CalculatorRunId = invalidRun.Id,
            ApprovedBy = "ignored"
        };

        var exception = await Should.ThrowAsync<RunContextException>(() =>
            _sut.CreateContext(message, CancellationToken.None));

        exception.Message.ShouldContain("No producers");
    }
}