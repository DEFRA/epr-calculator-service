namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter
{
    using AutoFixture;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Exporter.CsvExporter;
    using EPR.Calculator.Service.Function.Models;
    using EPR.Calculator.Service.Function.UnitTests.Builder;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Text;

    [TestClass]
    public class CalcResultSummaryExporterTests
    {
        private CalcResultSummaryExporter _testClass;

        public CalcResultSummaryExporterTests()
        {
            _testClass = new CalcResultSummaryExporter();
        }

        [TestMethod]
        public void CanCallWriteColumnHeaders()
        {
            // Arrange
            var fixture = new Fixture();
            var resultSummary = new CalcResultSummary
            {
                ColumnHeaders = new List<CalcResultSummaryHeader>
                {
                    new CalcResultSummaryHeader
                    {
                        ColumnIndex = 0,
                        Name = "Column 0",
                    },
                    new CalcResultSummaryHeader
                    {
                        ColumnIndex = 1,
                        Name = "Column 2",
                    },
                    new CalcResultSummaryHeader
                    {
                        ColumnIndex = 5,
                        Name = "Column 6",
                    },
                    new CalcResultSummaryHeader
                    {
                        ColumnIndex = 10,
                        Name = "Column 11",
                    },
                    new CalcResultSummaryHeader
                    {
                        ColumnIndex = 20,
                        Name = "Column 21",
                    },
                },
            };
            var csvContent = new StringBuilder();

            // Act
            _testClass.WriteColumnHeaders(resultSummary, csvContent);

            // Assert
            Assert.AreEqual("\"Column 0\",\"Column 2\",\"Column 6\",\"Column 11\",\"Column 21\",", csvContent.ToString());
        }

        [TestMethod]
        public void CanCallWriteSecondaryHeaders()
        {
            var columnHeaders = new List<CalcResultSummaryHeader>
            {
                new CalcResultSummaryHeader
                {
                    ColumnIndex = 1,
                    Name = "Column 0",
                },
                new CalcResultSummaryHeader
                {
                    ColumnIndex = 2,
                    Name = "Column 2",
                },
                new CalcResultSummaryHeader
                {
                    ColumnIndex = 6,
                    Name = "Column 6",
                },
                new CalcResultSummaryHeader
                {
                    ColumnIndex = 11,
                    Name = "Column 11",
                },
                new CalcResultSummaryHeader
                {
                    ColumnIndex = 21,
                    Name = "Column 21",
                },
            };
            var csvContent = new StringBuilder();
            _testClass.WriteSecondaryHeaders(csvContent, columnHeaders);

            var rowContents = csvContent.ToString().Split(",");
            Assert.AreEqual(CommonConstants.SecondaryHeaderMaxColumnSize, rowContents.Length);
            Assert.AreEqual("\"Column 0\"", rowContents[0]);
            Assert.AreEqual("\"Column 2\"", rowContents[1]);
            Assert.AreEqual("\"Column 6\"", rowContents[5]);
            Assert.AreEqual("\"Column 11\"", rowContents[10]);
            Assert.AreEqual("\"Column 21\"", rowContents[20]);
        }

        [TestMethod]
        public void CanCallAddNewRow()
        {
            // Arrange
            var csvContent = new StringBuilder();
            var producer = TestDataHelper.GetProducerDisposalFees()[0];

            // Act
            _testClass.AddNewRow(csvContent, producer);
            var results = csvContent.ToString().Split(",");

            // Assert
            Assert.AreEqual(302, results.Length);
        }

        [TestMethod]
        public void CanCallAddNewRow_IsOverAllTotalTrue()
        {
            // Arrange
            var csvContent = new StringBuilder();
            var producer = TestDataHelper.GetProducerDisposalFeesForOverAllTotal()[0];

            // Act
            _testClass.AddNewRow(csvContent, producer);
            var results = csvContent.ToString().Split(",");

            // Assert
            Assert.AreEqual(302, results.Length);
        }

        [TestMethod]
        public void CanCallAddNewRow_TonnageValueNull()
        {
            // Arrange
            var csvContent = new StringBuilder();
            var producer = TestDataHelper.GetProducerDisposalFeesTonnageValueNull()[0];

            // Act
            _testClass.AddNewRow(csvContent, producer);
            var results = csvContent.ToString().Split(",");

            // Assert
            Assert.AreEqual(302, results.Length);
        }

        [TestMethod]
        public void CanCallExport()
        {
            // Arrange
            var resultSummary = new CalcResultSummary();
            var csvContent = new StringBuilder();

            resultSummary.ProducerDisposalFees = TestDataHelper.GetProducerDisposalFees();

            // Act
            _testClass.Export(resultSummary, csvContent);

            // Assert
            Assert.IsNotNull(csvContent.ToString());
        }
    }
}