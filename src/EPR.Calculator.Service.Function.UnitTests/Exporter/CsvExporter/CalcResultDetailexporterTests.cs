using System.Text;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Detail;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter
{
    [TestClass]
    public class CalcResultDetailexporterTests
    {
        private CalcResultDetailexporter _testClass;

        public CalcResultDetailexporterTests()
        {
            _testClass = new CalcResultDetailexporter();
        }

        [TestMethod]
        public void CanCallExport()
        {
            // Arrange
            var calcResultDetail = TestFixtures.Default.Create<CalcResultDetail>();
            calcResultDetail.RunName = "SomeRunName";
            calcResultDetail.RunId = 999;
            calcResultDetail.RunDate = new DateTime(2024, 12, 1);
            calcResultDetail.RelativeYear = new RelativeYear(2024);
            calcResultDetail.RpdFileORG = "RpdFileOrg";
            calcResultDetail.RpdFilePOM = "RpdFilePom";
            calcResultDetail.LapcapFile = "LapcapFile";
            calcResultDetail.ParametersFile = "ParamsFile";

            var csvContent = new StringBuilder();

            // Act
            _testClass.Export(calcResultDetail, csvContent);

            var result = csvContent.ToString();
            var lines = result.Split(Environment.NewLine);
            Assert.AreEqual(7, lines.Length);

            Assert.IsTrue(lines.First().Contains("Run Name"));
            Assert.IsTrue(lines.First().Contains("SomeRunName"));
            Assert.IsTrue(lines.Last().Contains(string.Empty));
        }
    }
}