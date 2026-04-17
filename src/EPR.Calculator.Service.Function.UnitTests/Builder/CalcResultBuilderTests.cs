using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Builder;
using EPR.Calculator.Service.Function.Builder.CancelledProducers;
using EPR.Calculator.Service.Function.Builder.CommsCost;
using EPR.Calculator.Service.Function.Builder.Detail;
using EPR.Calculator.Service.Function.Builder.ErrorReport;
using EPR.Calculator.Service.Function.Builder.LaDisposalCost;
using EPR.Calculator.Service.Function.Builder.Lapcap;
using EPR.Calculator.Service.Function.Builder.LateReportingTonnages;
using EPR.Calculator.Service.Function.Builder.OnePlusFourApportionment;
using EPR.Calculator.Service.Function.Builder.ParametersOther;
using EPR.Calculator.Service.Function.Builder.PartialObligations;
using EPR.Calculator.Service.Function.Builder.ProjectedProducers;
using EPR.Calculator.Service.Function.Builder.RejectedProducers;
using EPR.Calculator.Service.Function.Builder.ScaledupProducers;
using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Features.Billing.Contexts;
using EPR.Calculator.Service.Function.Features.Calculator.Contexts;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.Service.Function.UnitTests.Builder;

[TestClass]
public class CalcResultBuilderTests
{
    private CalcResultBuilder _calcResultBuilder = null!;
    private IFixture _fixture = null!;
    private Mock<ICalcResultCancelledProducersBuilder> _mockCalcResultCancelledProducersBuilder = null!;
    private Mock<ICalcResultDetailBuilder> _mockCalcResultDetailBuilder = null!;
    private Mock<ICalcResultErrorReportBuilder> _mockCalcResultErrorReportBuilder = null!;
    private Mock<ICalcResultParameterOtherCostBuilder> _mockCalcResultParameterOtherCostBuilder = null!;
    private Mock<ICalcResultPartialObligationBuilder> _mockCalcResultPartialObligationBuilder = null!;
    private Mock<ICalcResultProjectedProducersBuilder> _mockCalcResultProjectedProducersBuilder = null!;
    private Mock<ICalcResultRejectedProducersBuilder> _mockCalcResultRejectedProducersBuilder = null!;
    private Mock<ICalcResultScaledupProducersBuilder> _mockCalcResultScaledupProducersBuilder = null!;
    private Mock<ICalcRunLaDisposalCostBuilder> _mockCalcRunLaDisposalCostBuilder = null!;
    private Mock<ICalcResultCommsCostBuilder> _mockCommsCostReportBuilder = null!;
    private Mock<ICalcResultLapcapDataBuilder> _mockLapcapBuilder = null!;
    private Mock<ICalcResultLateReportingBuilder> _mockLateReportingBuilder = null!;
    private Mock<ICalcResultOnePlusFourApportionmentBuilder> _mockOnePlusFourApportionmentBuilder = null!;
    private Mock<IProjectedProducersService> _mockProjectedProducerService = null!;
    private Mock<ICalcResultSummaryBuilder> _mockSummaryBuilder = null!;

    [TestInitialize]
    public void Init()
    {
        _fixture = TestFixtures.New();
        _mockCalcResultDetailBuilder = _fixture.Freeze<Mock<ICalcResultDetailBuilder>>();
        _mockLapcapBuilder = _fixture.Freeze<Mock<ICalcResultLapcapDataBuilder>>();
        _mockSummaryBuilder = _fixture.Freeze<Mock<ICalcResultSummaryBuilder>>();
        _mockCalcRunLaDisposalCostBuilder = _fixture.Freeze<Mock<ICalcRunLaDisposalCostBuilder>>();
        _mockCommsCostReportBuilder = _fixture.Freeze<Mock<ICalcResultCommsCostBuilder>>();
        _mockLateReportingBuilder = _fixture.Freeze<Mock<ICalcResultLateReportingBuilder>>();
        _mockCalcResultParameterOtherCostBuilder = _fixture.Freeze<Mock<ICalcResultParameterOtherCostBuilder>>();
        _mockOnePlusFourApportionmentBuilder = _fixture.Freeze<Mock<ICalcResultOnePlusFourApportionmentBuilder>>();
        _mockCalcResultScaledupProducersBuilder = _fixture.Freeze<Mock<ICalcResultScaledupProducersBuilder>>();
        _mockCalcResultPartialObligationBuilder = _fixture.Freeze<Mock<ICalcResultPartialObligationBuilder>>();
        _mockCalcResultProjectedProducersBuilder = _fixture.Freeze<Mock<ICalcResultProjectedProducersBuilder>>();
        _mockCalcResultCancelledProducersBuilder = _fixture.Freeze<Mock<ICalcResultCancelledProducersBuilder>>();
        _mockCalcResultRejectedProducersBuilder = _fixture.Freeze<Mock<ICalcResultRejectedProducersBuilder>>();
        _mockCalcResultErrorReportBuilder = _fixture.Freeze<Mock<ICalcResultErrorReportBuilder>>();
        _mockProjectedProducerService = _fixture.Freeze<Mock<IProjectedProducersService>>();

        _calcResultBuilder = _fixture.Create<CalcResultBuilder>();
    }

    [TestMethod]
    public async Task Build_ShouldReturnCalcResult()
    {
        var runContext = _fixture.Create<CalculatorRunContext>();

        var result = await _calcResultBuilder.BuildAsync(runContext);
        result.ShouldNotBeNull();

        _mockCalcResultDetailBuilder.Verify(m => m.ConstructAsync(runContext), Times.Once);
        _mockLapcapBuilder.Verify(m => m.ConstructAsync(runContext), Times.Once);
        _mockSummaryBuilder.Verify(m => m.ConstructAsync(runContext, It.IsAny<CalcResult>()), Times.Once);
        _mockCalcRunLaDisposalCostBuilder.Verify(m => m.ConstructAsync(runContext, It.IsAny<CalcResult>()), Times.Once);
        _mockLateReportingBuilder.Verify(m => m.ConstructAsync(runContext), Times.Once);
        _mockCalcResultParameterOtherCostBuilder.Verify(m => m.ConstructAsync(runContext), Times.Once);
        _mockOnePlusFourApportionmentBuilder.Verify(m => m.ConstructAsync(runContext, It.IsAny<CalcResult>()), Times.Once);
        _mockCalcResultCancelledProducersBuilder.Verify(m => m.ConstructAsync(runContext), Times.Once);
        _mockCalcResultErrorReportBuilder.Verify(m => m.ConstructAsync(runContext), Times.Once);
    }

    [TestMethod]
    public async Task Build_ShouldReturnCalcResult_WithProjectedProducers()
    {
        var runContext = _fixture.Create<CalculatorRunContext>() with { RelativeYear = new RelativeYear(2026) };
        var mockCalcResultScaledUpProducersData = _fixture.Create<CalcResultScaledupProducers>();
        var mockCalcResultProjectedProducersData = _fixture.Create<CalcResultProjectedProducers>();

        _mockCalcResultScaledupProducersBuilder.Setup(m => m.ConstructAsync(runContext)).ReturnsAsync(mockCalcResultScaledUpProducersData);
        _mockCalcResultProjectedProducersBuilder.Setup(m => m.ConstructAsync(runContext)).ReturnsAsync(mockCalcResultProjectedProducersData);

        var result = await _calcResultBuilder.BuildAsync(runContext);

        result.ShouldNotBeNull();
        result.CalcResultScaledupProducers.ShouldNotBe(mockCalcResultScaledUpProducersData);
        result.CalcResultProjectedProducers.ShouldBe(mockCalcResultProjectedProducersData);

        _mockCalcResultScaledupProducersBuilder.Verify(m => m.ConstructAsync(runContext), Times.Never);
        _mockCalcResultProjectedProducersBuilder.Verify(m => m.ConstructAsync(runContext), Times.Once);
        _mockProjectedProducerService.Verify(m => m.StoreProjectedProducers(1, It.IsAny<List<CalcResultH2ProjectedProducer>>()), Times.Once);
    }

    [TestMethod]
    public async Task Build_ShouldReturnCalcResult_WithProjectedProducers_Billing()
    {
        var runContext = _fixture.Create<BillingRunContext>() with { RelativeYear = new RelativeYear(2026)};
        var mockCalcResultScaledUpProducersData = _fixture.Create<CalcResultScaledupProducers>();
        var mockCalcResultProjectedProducersData = _fixture.Create<CalcResultProjectedProducers>();

        _mockCalcResultScaledupProducersBuilder.Setup(m => m.ConstructAsync(runContext)).ReturnsAsync(mockCalcResultScaledUpProducersData);
        _mockCalcResultProjectedProducersBuilder.Setup(m => m.ConstructAsync(runContext)).ReturnsAsync(mockCalcResultProjectedProducersData);

        var result = await _calcResultBuilder.BuildAsync(runContext);

        result.ShouldNotBeNull();
        result.CalcResultScaledupProducers.ShouldNotBe(mockCalcResultScaledUpProducersData);
        result.CalcResultProjectedProducers.ShouldBe(mockCalcResultProjectedProducersData);

        _mockCalcResultScaledupProducersBuilder.Verify(m => m.ConstructAsync(runContext), Times.Never);
        _mockCalcResultProjectedProducersBuilder.Verify(m => m.ConstructAsync(runContext), Times.Once);
        _mockProjectedProducerService.Verify(m => m.StoreProjectedProducers(1, It.IsAny<List<CalcResultH2ProjectedProducer>>()), Times.Never);
    }
}