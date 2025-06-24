using AutoFixture;
using EPR.Calculator.Service.Function.Exporter.JsonExporter;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.CalculationResults;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.CancelledProducers;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.CommsCostByMaterial2A;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.Detail;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.Lapcap;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.LateReportingTonnage;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.OnePlusFourApportionment;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.ScaledupProducers;
using EPR.Calculator.Service.Function.Models;
using Moq;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter
{
    [TestClass]
    public class CalcResultsJsonExporterTests
    {
        private Fixture fixture;
        private CalcResultsJsonExporter testClass;
        private Mock<ICalcResultDetailJsonExporter> mockCalcResultDetailExporter;
        private Mock<ICalcResultLapcapExporter> mockCalcResultLapcapExporter;
        private Mock<ILateReportingTonnage> mockLateReportingTonnage;
        private Mock<IOnePlusFourApportionmentJsonExporter> mockOnePlusFourApportionmentJsonExporter;
        private Mock<ICommsCostJsonExporter> mockCommsCostExporter;
        private Mock<ICommsCostByMaterial2AExporter> mockCommsCostByMaterial2AExporter;
        private Mock<ICancelledProducersExporter> mockCancelledProducersExporter;
        private Mock<ICalcResultScaledupProducersJsonExporter> mockCalcResultScaledupProducersJsonExporter;
        private Mock<ICalculationResultsExporter> mockCalculationResultsExporter;

        public CalcResultsJsonExporterTests()
        {
            fixture = new Fixture();
            mockCalcResultDetailExporter = new Mock<ICalcResultDetailJsonExporter>();
            mockCalcResultLapcapExporter = new Mock<ICalcResultLapcapExporter>();
            mockLateReportingTonnage = new Mock<ILateReportingTonnage>();
            mockOnePlusFourApportionmentJsonExporter = new Mock<IOnePlusFourApportionmentJsonExporter>();
            mockCommsCostExporter = new Mock<ICommsCostJsonExporter>();
            mockCommsCostByMaterial2AExporter = new Mock<ICommsCostByMaterial2AExporter>();
            mockCancelledProducersExporter = new Mock<ICancelledProducersExporter>();
            mockCalcResultScaledupProducersJsonExporter = new Mock<ICalcResultScaledupProducersJsonExporter>();
            mockCalculationResultsExporter = new Mock<ICalculationResultsExporter>();

            testClass = new CalcResultsJsonExporter(
                mockCalcResultDetailExporter.Object,
                mockCalcResultLapcapExporter.Object,
                mockLateReportingTonnage.Object,
                mockOnePlusFourApportionmentJsonExporter.Object,
                mockCommsCostExporter.Object,
                mockCommsCostByMaterial2AExporter.Object,
                mockCancelledProducersExporter.Object,
                mockCalcResultScaledupProducersJsonExporter.Object,
                mockCalculationResultsExporter.Object);
        }

        [TestMethod]
        public void Export_ShouldReturnJsonContent()
        {
            // Arrange
            var calcResult = fixture.Create<CalcResult>();

            // Act
            var result = testClass.Export(calcResult, new List<int> { 1,2 });

            // Assert
            Assert.IsNotNull(result);

            //mockCalcResultDetailExporter.Verify(x => x.Export(calcResult.CalcResultDetail));
            //mockCalcResultLapcapExporter.Verify(x => x.ConvertToJson(calcResult.CalcResultLapcapData));
            //mockLateReportingTonnage.Verify(x => x.Export(calcResult.CalcResultLateReportingTonnageData));
            //mockOnePlusFourApportionmentJsonExporter.Verify(x => x.Export(calcResult.CalcResultOnePlusFourApportionment));
            //mockCommsCostExporter.Verify(x => x.Export(calcResult.CalcResultCommsCostReportDetail));
            //mockCommsCostByMaterial2AExporter.Verify(x => x.Export(calcResult.CalcResultCommsCostReportDetail.CalcResultCommsCostCommsCostByMaterial));
            //mockCancelledProducersExporter.Verify(x => x.Export(calcResult.CalcResultCancelledProducers));
            //mockCalcResultScaledupProducersJsonExporter.Verify(x => x.Export(calcResult.CalcResultScaledupProducers, It.IsAny<List<int>>()));
            //mockCalculationResultsExporter.Verify(x => x.Export(It.IsAny<CalcResultSummary>(), It.IsAny<List<int>>()));
        }
    }
}
