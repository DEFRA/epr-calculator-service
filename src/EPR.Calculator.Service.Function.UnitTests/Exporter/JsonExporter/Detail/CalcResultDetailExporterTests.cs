using AutoFixture;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Common;
using EPR.Calculator.Service.Common.UnitTests.Utils;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.Detail;
using EPR.Calculator.Service.Function.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Nodes;
using static EPR.Calculator.Service.Common.UnitTests.Utils.JsonNodeComparer;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.JsonExporter.Detail
{
    [TestClass]
    public class CalcResultDetailExporterTests
    {

        private CalcResultDetailJsonExporter calcResultDetailExporter;
        private IFixture Fixture;

        public CalcResultDetailExporterTests()
        {
            calcResultDetailExporter = new CalcResultDetailJsonExporter();
            Fixture = new Fixture();
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
                RelativeYear = new RelativeYear(2025),
                RpdFileORG = "21/07/2017 17:32",
                RpdFilePOM = "21/07/2017 17:32",
                LapcapFile = "lapcap_file.csv,21/07/2017 17:32,John Doe",
                ParametersFile = "parameters_file.csv,21/07/2017 17:32,John Doe"
            };

            // Act
            var result = this.calcResultDetailExporter.Export(calcResultDetail);
            var json = JsonSerializer.Serialize(result);
            // Assert
            var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json);

            Assert.AreEqual(calcResultDetail.RunName, roundTrippedData!["runName"]?.GetValue<string>());
            Assert.AreEqual(calcResultDetail.RunBy, roundTrippedData!["runBy"]?.GetValue<string>());
            Assert.AreEqual("2025-26", roundTrippedData!["financialYear"]?.GetValue<string>());
        }
    }
}
