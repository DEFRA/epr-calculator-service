using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.CancelledProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.CommsCost;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Detail;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.LaDisposalCost;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Lapcap;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.OtherCosts;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.PartialObligations;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ProjectedProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.RejectedProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ScaledupProducers;
using EPR.Calculator.Service.Function.Features.Billing.Contexts;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter;

[TestCategory(TestCategories.BillingRuns)]
[TestClass]
public class BillingFileCsvWriterTests
{
    private readonly Mock<ICalcResultCancelledProducersExporter> _calcResultCancelledProducersExporter;
    private readonly Mock<ICalcResultPartialObligationsExporter> _calcResultPartialObligationsExporter;
    private readonly Mock<ICalcResultRejectedProducersExporter> _calcResultRejectedProducersExporter;
    private readonly Mock<ICalcResultScaledupProducersExporter> _calcResultScaledupProducersExporter;
    private readonly Mock<ICalcResultSummaryExporter> _calcResultSummaryExporter;
    private readonly Mock<ICommsCostExporter> _commsCostExporter;
    private readonly Mock<ICalcResultLaDisposalCostExporter> _laDisposalCostExporter;
    private readonly Mock<ILapcaptDetailExporter> _lapcaptDetailExporter;
    private readonly Mock<ILateReportingExporter> _lateReportingExporter;
    private readonly Mock<IOnePlusFourApportionmentExporter> _onePlusFourApportionmentExporter;
    private readonly Mock<ICalcResultParameterOtherCostExporter> _parameterOtherCosts;
    private readonly Mock<ICalcResultDetailExporter> _resultDetailexporter;
    private readonly BillingFileCsvWriter testClass;
    private readonly Mock<ICalcResultProjectedProducersExporter> _calcResultProjectedProducersExporter;

    public BillingFileCsvWriterTests()
    {
        _lateReportingExporter = new Mock<ILateReportingExporter>();
        _resultDetailexporter = new Mock<ICalcResultDetailExporter>();
        _onePlusFourApportionmentExporter = new Mock<IOnePlusFourApportionmentExporter>();
        _laDisposalCostExporter = new Mock<ICalcResultLaDisposalCostExporter>();
        _calcResultScaledupProducersExporter = new Mock<ICalcResultScaledupProducersExporter>();
        _calcResultPartialObligationsExporter = new Mock<ICalcResultPartialObligationsExporter>();
        _lapcaptDetailExporter = new Mock<ILapcaptDetailExporter>();
        _parameterOtherCosts = new Mock<ICalcResultParameterOtherCostExporter>();
        _commsCostExporter = new Mock<ICommsCostExporter>();
        _calcResultSummaryExporter = new Mock<ICalcResultSummaryExporter>();
        _calcResultCancelledProducersExporter = new Mock<ICalcResultCancelledProducersExporter>();
        _calcResultRejectedProducersExporter = new Mock<ICalcResultRejectedProducersExporter>();
        _calcResultProjectedProducersExporter = new Mock<ICalcResultProjectedProducersExporter>();

        testClass = new BillingFileCsvWriter(
            _lateReportingExporter.Object,
            _resultDetailexporter.Object,
            _onePlusFourApportionmentExporter.Object,
            _laDisposalCostExporter.Object,
            _calcResultScaledupProducersExporter.Object,
            _calcResultPartialObligationsExporter.Object,
            _calcResultProjectedProducersExporter.Object,
            _lapcaptDetailExporter.Object,
            _parameterOtherCosts.Object,
            _commsCostExporter.Object,
            _calcResultSummaryExporter.Object,
            _calcResultCancelledProducersExporter.Object,
            _calcResultRejectedProducersExporter.Object);
    }

    [TestMethod]
    public void CanCallExport()
    {
        // Arrange
        var results = TestFixtures.Default.Create<CalcResult>();
        var runContext = TestFixtures.Default.Create<BillingRunContext>() with { AcceptedProducerIds = results.CalcResultScaledupProducers.ScaledupProducers!.Select(t => t.ProducerId).Take(1).ToImmutableHashSet() };

        _lateReportingExporter.Setup(mock => mock
            .Export(It.IsAny<CalcResultLateReportingTonnage>()))
            .Returns(TestFixtures.Default.Create<string>());

        // Act
        var result = testClass.WriteToString(runContext, results);

        // Assert
        _resultDetailexporter.Verify(mock => mock.Export(It.IsAny<CalcResultDetail>(), It.IsAny<StringBuilder>()));
        _onePlusFourApportionmentExporter.Verify(mock => mock.Export(It.IsAny<CalcResultOnePlusFourApportionment>(), It.IsAny<StringBuilder>()));
        _lateReportingExporter.Verify(mock => mock.Export(It.IsAny<CalcResultLateReportingTonnage>()));
        _calcResultScaledupProducersExporter.Verify(mock => mock.Export(It.IsAny<CalcResultScaledupProducers>(), It.IsAny<StringBuilder>()));
        _calcResultPartialObligationsExporter.Verify(mock => mock.Export(It.IsAny<CalcResultPartialObligations>(), It.IsAny<StringBuilder>()));
        _lapcaptDetailExporter.Verify(mock => mock.Export(It.IsAny<CalcResultLapcapData>(), It.IsAny<StringBuilder>()));
        _parameterOtherCosts.Verify(mock => mock.Export(It.IsAny<CalcResultParameterOtherCost>(), It.IsAny<StringBuilder>()));
        _calcResultSummaryExporter.Verify(mock => mock.Export(It.IsAny<CalcResultSummary>(), It.IsAny<StringBuilder>(), It.IsAny<bool>()));
        _laDisposalCostExporter.Verify(mock => mock.Export(It.IsAny<CalcResultLaDisposalCostData>(), It.IsAny<StringBuilder>()));
        _commsCostExporter.Verify(mock => mock.Export(It.IsAny<CalcResultCommsCost>(), It.IsAny<StringBuilder>()));
        _calcResultCancelledProducersExporter.Verify(mock => mock.Export(It.IsAny<CalcResultCancelledProducersResponse>(), It.IsAny<StringBuilder>()));
        _calcResultRejectedProducersExporter.Verify(mock => mock.Export(It.IsAny<IEnumerable<CalcResultRejectedProducer>>(), It.IsAny<StringBuilder>()));
        _calcResultProjectedProducersExporter.Verify(mock => mock.Export(It.IsAny<CalcResultProjectedProducers>(), It.IsAny<StringBuilder>()), Times.Never);

        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void CanCallExportModulation()
    {
        // Arrange
        var results = TestFixtures.Default.Create<CalcResult>();
        results.CalcResultModulation = "???";
        var runContext = TestFixtures.Default.Create<BillingRunContext>() with { AcceptedProducerIds = results.CalcResultScaledupProducers.ScaledupProducers!.Select(t => t.ProducerId).Take(1).ToImmutableHashSet() };

        _lateReportingExporter.Setup(mock => mock
            .Export(It.IsAny<CalcResultLateReportingTonnage>()))
            .Returns(TestFixtures.Default.Create<string>());

        // Act
        var result = testClass.WriteToString(runContext, results);

        // Assert
        _calcResultScaledupProducersExporter.Verify(mock => mock.Export(It.IsAny<CalcResultScaledupProducers>(), It.IsAny<StringBuilder>()), Times.Never);
        _calcResultProjectedProducersExporter.Verify(mock => mock.Export(It.IsAny<CalcResultProjectedProducers>(), It.IsAny<StringBuilder>()), Times.Once);

        Assert.IsNotNull(result);
    }
}