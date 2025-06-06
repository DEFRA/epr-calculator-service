using EPR.Calculator.Service.Function.Exporter.JsonExporter.Detail;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.JsonExporter.Detail
{
    [TestClass]
    public class CalcResultDetailExporterTests
    {

        private CalcResultDetailExporter calcResultDetailExporter;

        public CalcResultDetailExporterTests()
        {
            calcResultDetailExporter = new CalcResultDetailExporter();
        }
        [TestMethod]
        public void Export_ValidCalcResultDetail_ReturnsCorrectJson()
        {
            // Arrange
            var calcResultDetail = new CalcResultDetail
            {
                RunName = "Test Run",
                RunId = 123,
                RunDate = new DateTime(2017, 07, 21, 17, 32, 0, DateTimeKind.Utc),
                RunBy = "John Doe",
                FinancialYear = "2025",
                RpdFileORG = "21/07/2017 17:32",
                RpdFilePOM = "21/07/2017 17:32",
                LapcapFile = "lapcap_file.csv,21/07/2017 17:32,John Doe",
                ParametersFile = "parameters_file.csv,21/07/2017 17:32,John Doe"
            };

            // Act
            var actualJson = this.calcResultDetailExporter.Export(calcResultDetail);

            // Assert
            // Expected JSON output
            string expected = "{\"runName\":\"Test Run\",\"runId\":123,\"runDate\":\"21/07/2017 17:32\",\"runBy\":\"John Doe\",\"financialYear\":\"2025\",\"rpdFileORG\":\"\",\"rpdFileORGTimeStamp\":\"21/07/2017 17:32\",\"rpdFilePOM\":\"\",\"rpdFilePOMTimeStamp\":\"21/07/2017 17:32\",\"lapcapFile\":\"lapcap_file.csv\",\"lapcapFileTimeStamp\":\"21/07/2017 17:32\",\"lapcapFileUploader\":\"John Doe\",\"parametersFile\":\"parameters_file.csv\",\"parametersFileTimeStamp\":\"21/07/2017 17:32\",\"parametersFileUploader\":\"John Doe\"}";

            Assert.AreEqual(expected, actualJson);
        }
    }
}
