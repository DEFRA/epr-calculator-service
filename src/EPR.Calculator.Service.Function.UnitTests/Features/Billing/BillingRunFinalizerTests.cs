using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Features.Billing;
using EPR.Calculator.Service.Function.Features.Billing.Contexts;
using EPR.Calculator.Service.Function.Features.Billing.FileExports;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.UnitTests.Features.Billing;

[TestClass]
public class BillingRunFinalizerTests
{
    private BillingRunFinalizer _sut = null!;
    private CalcResult _calcResult = null!;
    private ApplicationDBContext _dbContext = null!;
    private BillingFileExportResult _exportResult = null!;
    private IFixture _fixture = null!;
    private BillingRunContext _runContext = null!;

    [TestInitialize]
    public void Init()
    {
        _fixture = TestFixtures.New(o => o.IncludeSeedData());
        _dbContext = _fixture.Freeze<ApplicationDBContext>();
        _fixture.Register<IMaterialService>(() => new MaterialService(_dbContext));
        _runContext = _fixture.Create<BillingRunContext>();
        _exportResult = _fixture.Create<BillingFileExportResult>();
        _calcResult = _fixture.Create<CalcResult>();

        _sut = _fixture.Create<BillingRunFinalizer>();
    }

    [TestMethod]
    public async Task Should_commit_changes()
    {
        // Arrange
        var section = new CalcResultSummaryBillingInstruction
        {
            CurrentYearInvoiceTotalToDate = 123.45m,
            TonnageChangeSinceLastInvoice = "Yes",
            LiabilityDifference = 678.90m,
            MaterialThresholdBreached = "Mat TH",
            TonnageThresholdBreached = "Ton TH",
            PercentageLiabilityDifference = 11.22m,
            TonnagePercentageThresholdBreached = "Ton % TH",
            SuggestedBillingInstruction = "ISSUE",
            SuggestedInvoiceAmount = 999.01m
        };

        _calcResult.CalcResultSummary = TestData.GetCalcResultSummary();
        _calcResult.CalcResultSummary.ProducerDisposalFees =
        [
            new CalcResultSummaryProducerDisposalFees
            {
                ProducerId = "1",
                ProducerIdInt = 1,
                ProducerName = "Producer 1",
                SubsidiaryId = string.Empty,
                Level = CommonConstants.LevelOne.ToString(),
                BillingInstructionSection = section
            }
        ];

        // Act
        await _sut.FinalizeAsCompleted(_runContext, _calcResult, _exportResult, CancellationToken.None);

        // Assert
        var entity = _dbContext.ProducerResultFileSuggestedBillingInstruction
            .Single(p => p.CalculatorRunId == _runContext.RunId && p.ProducerId == 1);

        entity.CurrentYearInvoiceTotalToDate.ShouldBe(section.CurrentYearInvoiceTotalToDate);
        entity.TonnageChangeSinceLastInvoice.ShouldBe(section.TonnageChangeSinceLastInvoice);
        entity.AmountLiabilityDifferenceCalcVsPrev.ShouldBe(section.LiabilityDifference);
        entity.MaterialPoundThresholdBreached.ShouldBe(section.MaterialThresholdBreached);
        entity.TonnagePoundThresholdBreached.ShouldBe(section.TonnageThresholdBreached);
        entity.PercentageLiabilityDifferenceCalcVsPrev.ShouldBe(section.PercentageLiabilityDifference);
        entity.TonnagePercentageThresholdBreached.ShouldBe(section.TonnagePercentageThresholdBreached);
        entity.SuggestedBillingInstruction.ShouldBe(section.SuggestedBillingInstruction);
        entity.SuggestedInvoiceAmount.ShouldBe(section.SuggestedInvoiceAmount);
    }

    [TestMethod]
    public async Task Should_not_commit_changes_when_no_level1_fees()
    {
        // Arrange:
        _calcResult.CalcResultSummary.ProducerDisposalFees = TestData.GetProducerDisposalFeesWithoutLevel1();

        var originals = await _dbContext.ProducerResultFileSuggestedBillingInstruction
            .AsNoTracking()
            .Where(p => p.CalculatorRunId == _runContext.RunId)
            .ToListAsync();

        // Act
        await _sut.FinalizeAsCompleted(_runContext, _calcResult, _exportResult, CancellationToken.None);

        // Assert: no row for this run was mutated
        var after = _dbContext.ProducerResultFileSuggestedBillingInstruction
            .AsNoTracking()
            .Where(p => p.CalculatorRunId == _runContext.RunId)
            .ToList();

        after.Count.ShouldBe(originals.Count);

        foreach (var original in originals)
        {
            var updated = after.Single(p => p.Id == original.Id);
            updated.SuggestedInvoiceAmount.ShouldBe(original.SuggestedInvoiceAmount);
            updated.SuggestedBillingInstruction.ShouldBe(original.SuggestedBillingInstruction);
            updated.CurrentYearInvoiceTotalToDate.ShouldBe(original.CurrentYearInvoiceTotalToDate);
            updated.TonnageChangeSinceLastInvoice.ShouldBe(original.TonnageChangeSinceLastInvoice);
            updated.AmountLiabilityDifferenceCalcVsPrev.ShouldBe(original.AmountLiabilityDifferenceCalcVsPrev);
            updated.MaterialPoundThresholdBreached.ShouldBe(original.MaterialPoundThresholdBreached);
            updated.TonnagePoundThresholdBreached.ShouldBe(original.TonnagePoundThresholdBreached);
            updated.PercentageLiabilityDifferenceCalcVsPrev.ShouldBe(original.PercentageLiabilityDifferenceCalcVsPrev);
            updated.TonnagePercentageThresholdBreached.ShouldBe(original.TonnagePercentageThresholdBreached);
            updated.BillingInstructionAcceptReject.ShouldBe(original.BillingInstructionAcceptReject);
        }
    }

    [TestMethod]
    public async Task Should_not_commit_changes_when_level1_but_producer_is_missing()
    {
        // Arrange: capture the untracked pre-state of every existing entity so
        // we can prove none of them were touched by the SUT.
        var originals = _dbContext.ProducerResultFileSuggestedBillingInstruction
            .AsNoTracking()
            .Where(p => p.CalculatorRunId == _runContext.RunId)
            .ToList();

        _calcResult.CalcResultSummary = TestData.GetCalcResultSummary();
        _calcResult.CalcResultSummary.ProducerDisposalFees =
        [
            new CalcResultSummaryProducerDisposalFees
            {
                ProducerId = "999",
                ProducerIdInt = 999,
                ProducerName = "ProducerNotExistsInEntityList",
                SubsidiaryId = string.Empty,
                Level = CommonConstants.LevelOne.ToString(),
                BillingInstructionSection = new CalcResultSummaryBillingInstruction
                {
                    SuggestedBillingInstruction = "INITIAL",
                    SuggestedInvoiceAmount = 123m
                }
            }
        ];

        // Act
        await _sut.FinalizeAsCompleted(_runContext, _calcResult, _exportResult, CancellationToken.None);

        // Assert: re-read untracked so we observe the persisted state, and
        // verify every field on every pre-existing row is preserved exactly.
        var after = _dbContext.ProducerResultFileSuggestedBillingInstruction
            .AsNoTracking()
            .Where(p => p.CalculatorRunId == _runContext.RunId)
            .ToList();

        after.Count.ShouldBe(originals.Count);
        foreach (var original in originals)
        {
            var updated = after.Single(p => p.Id == original.Id);
            updated.SuggestedInvoiceAmount.ShouldBe(original.SuggestedInvoiceAmount);
            updated.SuggestedBillingInstruction.ShouldBe(original.SuggestedBillingInstruction);
            updated.CurrentYearInvoiceTotalToDate.ShouldBe(original.CurrentYearInvoiceTotalToDate);
            updated.TonnageChangeSinceLastInvoice.ShouldBe(original.TonnageChangeSinceLastInvoice);
            updated.AmountLiabilityDifferenceCalcVsPrev.ShouldBe(original.AmountLiabilityDifferenceCalcVsPrev);
            updated.MaterialPoundThresholdBreached.ShouldBe(original.MaterialPoundThresholdBreached);
            updated.TonnagePoundThresholdBreached.ShouldBe(original.TonnagePoundThresholdBreached);
            updated.PercentageLiabilityDifferenceCalcVsPrev.ShouldBe(original.PercentageLiabilityDifferenceCalcVsPrev);
            updated.TonnagePercentageThresholdBreached.ShouldBe(original.TonnagePercentageThresholdBreached);
            updated.BillingInstructionAcceptReject.ShouldBe(original.BillingInstructionAcceptReject);
        }
    }

    [TestMethod]
    public async Task Should_commit_empty_values_when_level1_with_no_billing_instruction()
    {
        // Arrange: pre-populate every field on the existing entity so the test
        // can prove each one is actually cleared by the SUT.
        var original = _dbContext.ProducerResultFileSuggestedBillingInstruction
            .Single(p => p.CalculatorRunId == _runContext.RunId && p.ProducerId == 1);

        original.CurrentYearInvoiceTotalToDate = 999.99m;
        original.TonnageChangeSinceLastInvoice = "CHANGE";
        original.AmountLiabilityDifferenceCalcVsPrev = 888.88m;
        original.MaterialPoundThresholdBreached = "MAT";
        original.TonnagePoundThresholdBreached = "TON";
        original.PercentageLiabilityDifferenceCalcVsPrev = 77.77m;
        original.TonnagePercentageThresholdBreached = "TON%";
        original.SuggestedBillingInstruction = "INITIAL";
        original.SuggestedInvoiceAmount = 555.55m;
        await _dbContext.SaveChangesAsync();

        _calcResult.CalcResultSummary = TestData.GetCalcResultSummary();
        _calcResult.CalcResultSummary.ProducerDisposalFees =
        [
            new CalcResultSummaryProducerDisposalFees
            {
                ProducerId = "1",
                ProducerIdInt = 1,
                ProducerName = "Producer 1",
                SubsidiaryId = string.Empty,
                Level = CommonConstants.LevelOne.ToString(),
                BillingInstructionSection = null
            }
        ];

        // Act
        await _sut.FinalizeAsCompleted(_runContext, _calcResult, _exportResult, CancellationToken.None);

        // Assert: re-read with no tracking so we exercise persisted state
        // rather than the tracked in-memory instance mutated by the SUT.
        var updated = _dbContext.ProducerResultFileSuggestedBillingInstruction
            .AsNoTracking()
            .Single(p => p.CalculatorRunId == _runContext.RunId && p.ProducerId == 1);

        updated.CurrentYearInvoiceTotalToDate.ShouldBeNull();
        updated.TonnageChangeSinceLastInvoice.ShouldBeNull();
        updated.AmountLiabilityDifferenceCalcVsPrev.ShouldBeNull();
        updated.MaterialPoundThresholdBreached.ShouldBeNull();
        updated.TonnagePoundThresholdBreached.ShouldBeNull();
        updated.PercentageLiabilityDifferenceCalcVsPrev.ShouldBeNull();
        updated.TonnagePercentageThresholdBreached.ShouldBeNull();
        updated.SuggestedBillingInstruction.ShouldBeNull();
        updated.SuggestedInvoiceAmount.ShouldBe(0m);
    }

    [TestMethod]
    public async Task Should_commit_empty_values_when_level1_with_no_suggested_invoice_amount()
    {
        // Arrange
        var original = _dbContext.ProducerResultFileSuggestedBillingInstruction
            .Single(p => p.CalculatorRunId == _runContext.RunId && p.ProducerId == 1);
        original.SuggestedInvoiceAmount = 321.45m;
        await _dbContext.SaveChangesAsync();

        var section = new CalcResultSummaryBillingInstruction
        {
            CurrentYearInvoiceTotalToDate = 10m,
            TonnageChangeSinceLastInvoice = "No",
            LiabilityDifference = 5.5m,
            MaterialThresholdBreached = "Mat TH",
            TonnageThresholdBreached = "Ton TH",
            PercentageLiabilityDifference = 1.23m,
            TonnagePercentageThresholdBreached = "Ton % TH",
            SuggestedBillingInstruction = "ISSUE",
            SuggestedInvoiceAmount = null
        };

        _calcResult.CalcResultSummary = TestData.GetCalcResultSummary();
        _calcResult.CalcResultSummary.ProducerDisposalFees =
        [
            new CalcResultSummaryProducerDisposalFees
            {
                ProducerId = "1",
                ProducerIdInt = 1,
                ProducerName = "Producer 1",
                SubsidiaryId = string.Empty,
                Level = CommonConstants.LevelOne.ToString(),
                BillingInstructionSection = section
            }
        ];

        // Act
        await _sut.FinalizeAsCompleted(_runContext, _calcResult, _exportResult, CancellationToken.None);

        // Assert
        var updated = _dbContext.ProducerResultFileSuggestedBillingInstruction
            .Single(p => p.CalculatorRunId == _runContext.RunId && p.ProducerId == 1);

        updated.CurrentYearInvoiceTotalToDate.ShouldBe(section.CurrentYearInvoiceTotalToDate);
        updated.TonnageChangeSinceLastInvoice.ShouldBe(section.TonnageChangeSinceLastInvoice);
        updated.AmountLiabilityDifferenceCalcVsPrev.ShouldBe(section.LiabilityDifference);
        updated.MaterialPoundThresholdBreached.ShouldBe(section.MaterialThresholdBreached);
        updated.TonnagePoundThresholdBreached.ShouldBe(section.TonnageThresholdBreached);
        updated.PercentageLiabilityDifferenceCalcVsPrev.ShouldBe(section.PercentageLiabilityDifference);
        updated.TonnagePercentageThresholdBreached.ShouldBe(section.TonnagePercentageThresholdBreached);
        updated.SuggestedBillingInstruction.ShouldBe(section.SuggestedBillingInstruction);
        updated.SuggestedInvoiceAmount.ShouldBe(0m);
    }
}