using EPR.Calculator.Service.Function.Exporter.CsvExporter;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter
{
    /// <summary>
    /// Unit tests for the <see cref="LateReportingExporter"/> class.
    /// </summary>
    [TestClass]
    public class LateReportingExporterTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LateReportingExporterTests"/> class.
        /// </summary>
        public LateReportingExporterTests()
        {
            TestClass = new LateReportingExporter();
        }

        private LateReportingExporter TestClass { get; init; }

        /// <summary>
        /// Checks that the output matches the expected format.
        /// </summary>
        [TestMethod]
        public void CanCallPrepareData()
        {
            // Arrange
            var input = TestFixtures.Default.Create<CalcResultLateReportingTonnage>();
            var expectedheader = $"\"{input.Name}\"," + Environment.NewLine +
                $"\"{input.MaterialHeading}\",\"{input.TonnageHeading}\",\"{input.RedTonnageHeading}\",\"{input.AmberTonnageHeading}\",\"{input.GreenTonnageHeading}\"," + Environment.NewLine;
            var expectedMaterials = input.CalcResultLateReportingTonnageDetails.Select(m
                => $"\"{m.Name}\",\"{m.TotalLateReportingTonnage:0.000}\",\"{m.RedLateReportingTonnage:0.000}\",\"{m.AmberLateReportingTonnage:0.000}\",\"{m.GreenLateReportingTonnage:0.000}\",");

            var expectedResult = Environment.NewLine
                + Environment.NewLine
                + expectedheader
                + string.Join(Environment.NewLine, expectedMaterials)
                + Environment.NewLine;

            // Act
            var result = TestClass.Export(input);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }
    }
}