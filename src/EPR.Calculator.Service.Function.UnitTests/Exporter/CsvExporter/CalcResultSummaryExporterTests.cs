using System.Text;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Exporter.CsvExporter;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter
{
    [TestClass]
    public class CalcResultSummaryExporterTests
    {
        private CalcResultSummaryExporter _testClass;

        public CalcResultSummaryExporterTests()
        {
            _testClass = new CalcResultSummaryExporter();
        }

        [TestMethod]
        public void CalcResultSummaryExporter_CanCallExport()
        {
            // Arrange
            var runContext = TestDataHelper.CalculatorRun2025;
            var resultSummary = new CalcResultSummary
            {
                CalculatorRunId = 0,
                ProducerDisposalFees = TestDataHelper.GetProducerDisposalFees(),
                OverallTotal = TestDataHelper.GetOverallTotalRow()
            };

            var materials = TestDataHelper.GetMaterialDetails();

            var csvContent = new StringBuilder();

            // Act
            var calcResult = TestDataHelper.GetCalcResult();
            var scaledupIds = calcResult.CalcResultScaledupProducers.ScaledupProducers.Select(p => p.ProducerId).ToList();
            var partialIds = calcResult.CalcResultPartialObligations.PartialObligations.Select(p => (p.ProducerId, p.SubsidiaryId)).ToList();
            _testClass.Export(runContext, resultSummary, materials, scaledupIds, partialIds, csvContent);

            // Assert
            Assert.IsNotNull(csvContent.ToString());
        }
    }
}
