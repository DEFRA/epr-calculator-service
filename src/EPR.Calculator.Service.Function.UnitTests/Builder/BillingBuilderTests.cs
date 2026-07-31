using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder;
using EPR.Calculator.Service.Function.Builder.RejectedProducers;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Builder;

[TestCategory(TestCategories.BillingBuilder)]
[TestClass]
public class BillingBuilderTests : TestsFor<BillingBuilder>
{
    private Mock<ICalcResultRejectedProducersBuilder> mockCalcResultRejectedProducersBuilder = null!;
    private Mock<ICalcResultReader> mockCalcResultReader = null!;

    protected override void TestInitialize()
    {
        mockCalcResultRejectedProducersBuilder = fixture.Freeze<Mock<ICalcResultRejectedProducersBuilder>>();
        mockCalcResultReader = fixture.Freeze<Mock<ICalcResultReader>>();

        mockCalcResultRejectedProducersBuilder.Setup(x => x.ConstructAsync(It.IsAny<RunContext>())).ReturnsAsync([]);
    }

    [TestMethod]
    public async Task Build_ShouldReturnResult()
    {
        var runContext = TestDataHelper.BillingRun2026;
        var mockCalcResultPartialData = new Mock<CalcResultPartialObligations>();
        var mockSummary = new Mock<ProducerFees>();
        var mockSmcw = new Mock<SelfManagedConsumerWaste>();
        var mockMod = new Mock<ModulationResult>();
        var mockLapcapData = new Mock<CalcResultLapcapData>();
        var mockLateReportingTonnage = new Mock<CalcResultLateReportingTonnage>();
        var mockParameterOtherCost = new Mock<CalcResultParameterOtherCost>();
        var mockOnePlusFourApportionment = new Mock<CalcResultOnePlusFourApportionment>();
        var mockLaDisposalCostData = new Mock<CalcResultLaDisposalCostData>();
        var mockCommsCost = new Mock<CalcResultCommsCost>();

        mockCalcResultReader.Setup(m => m.ReadCancelledProducers(runContext.RunId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        mockCalcResultReader.Setup(m => m.ReadH1ProjectedData(runContext.RunId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        mockCalcResultReader.Setup(m => m.ReadH2ProjectedData(runContext.RunId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        mockCalcResultReader.Setup(m => m.ReadPartialData(runContext.RunId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCalcResultPartialData.Object);
        mockCalcResultReader.Setup(m => m.ReadProducerFees(runContext.RunId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockSummary.Object);
        mockCalcResultReader.Setup(m => m.ReadSmcw(runContext.RunId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockSmcw.Object);
        mockCalcResultReader.Setup(m => m.ReadModulationResult(runContext.RunId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockMod.Object);
        mockCalcResultReader.Setup(m => m.ReadLapcapData(runContext.RunId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockLapcapData.Object);
        mockCalcResultReader.Setup(m => m.ReadLateReportingTonnage(runContext.RunId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockLateReportingTonnage.Object);
        mockCalcResultReader.Setup(m => m.ReadParameterOtherCost(runContext.RunId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockParameterOtherCost.Object);
        mockCalcResultReader.Setup(m => m.ReadOnePlusFourApportionment(runContext.RunId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockOnePlusFourApportionment.Object);
        mockCalcResultReader.Setup(m => m.ReadLaDisposalCostData(runContext.RunId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockLaDisposalCostData.Object);
        mockCalcResultReader.Setup(m => m.ReadCommsCost(runContext.RunId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCommsCost.Object);

        var result = await testSubject.BuildAsync(runContext, It.IsAny<CancellationToken>());

        Assert.IsNotNull(result);
        Assert.AreSame(mockSmcw.Object, result.Smcw);
        Assert.AreSame(mockMod.Object, result.CalcResultModulation);
        Assert.AreSame(mockSummary.Object, result.ProducerFees);
        Assert.AreSame(mockLapcapData.Object, result.CalcResultLapcapData);
        Assert.AreSame(mockLateReportingTonnage.Object, result.CalcResultLateReportingTonnageData);
        Assert.AreSame(mockParameterOtherCost.Object, result.CalcResultParameterOtherCost);
        Assert.AreSame(mockOnePlusFourApportionment.Object, result.CalcResultOnePlusFourApportionment);
        Assert.AreSame(mockLaDisposalCostData.Object, result.CalcResultLaDisposalCostData);
        Assert.AreSame(mockCommsCost.Object, result.CalcResultCommsCostReportDetail);
        result.CalcResultCancelledProducers.ShouldBe([]);
    }

    [TestMethod]
    public async Task Build_ShouldReturnResult_WithProjectedProducers()
    {
        var runContext = TestDataHelper.BillingRun2026;
        var mockCalcResultPartialData = new Mock<CalcResultPartialObligations>();

        mockCalcResultReader.Setup(m => m.ReadCancelledProducers(runContext.RunId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        mockCalcResultReader.Setup(m => m.ReadH1ProjectedData(runContext.RunId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        mockCalcResultReader.Setup(m => m.ReadH2ProjectedData(runContext.RunId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        mockCalcResultReader.Setup(m => m.ReadPartialData(runContext.RunId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCalcResultPartialData.Object);

        var result = await testSubject.BuildAsync(runContext, It.IsAny<CancellationToken>());

        Assert.IsNotNull(result);
        result.CalcResultProjectedProducers!.H1ProjectedProducers.ShouldBe([]);
        result.CalcResultProjectedProducers!.H2ProjectedProducers.ShouldBe([]);
        Assert.AreSame(mockCalcResultPartialData.Object, result.CalcResultPartialObligations);
    }

    [TestMethod]
    public async Task Build_ShouldReturnResult_WithScaledUpProducers()
    {
        var runContext = TestDataHelper.BillingRun2025;
        var mockCalcResultPartialData = new Mock<CalcResultPartialObligations>();

        mockCalcResultReader.Setup(m => m.ReadCancelledProducers(runContext.RunId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        mockCalcResultReader.Setup(m => m.ReadScaledData(runContext.RunId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        mockCalcResultReader.Setup(m => m.ReadPartialData(runContext.RunId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCalcResultPartialData.Object);

        var result = await testSubject.BuildAsync(runContext, It.IsAny<CancellationToken>());

        Assert.IsNotNull(result);
        result.CalcResultScaledupProducers!.ScaledupProducers.ShouldBe([]);
        Assert.AreSame(mockCalcResultPartialData.Object, result.CalcResultPartialObligations);
    }
}
