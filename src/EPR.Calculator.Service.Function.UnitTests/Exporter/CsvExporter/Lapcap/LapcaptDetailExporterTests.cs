namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.Lapcap
{
    using System.Text;
    using EPR.Calculator.Service.Function.Exporter.CsvExporter.Lapcap;
    using EPR.Calculator.Service.Function.UnitTests.Builder;

    [TestClass]
    public class LapcaptDetailExporterTests
    {
        private ILapcaptDetailExporter lapcaptDetailExporter = new LapcaptDetailExporter();

        [TestMethod]
        public void ExportTest_ShouldShowCorrectHeaderAndRows()
        {
            // Arrange
            var csvContent = new StringBuilder();

            // Act
            lapcaptDetailExporter.Export(TestDataHelper.GetCalcResultLapcapData(), csvContent);

            // Assert
            var result = csvContent.ToString();
            var rows = result.Split(Environment.NewLine);
            Assert.AreEqual("LAPCAP Data", rows[2]);
            Assert.AreEqual(15, rows.Length);
        }
    }
}