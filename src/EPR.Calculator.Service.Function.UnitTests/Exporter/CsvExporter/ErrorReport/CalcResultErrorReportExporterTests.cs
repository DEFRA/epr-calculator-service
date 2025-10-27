using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ErrorReport;
using EPR.Calculator.Service.Function.Models;
using Microsoft.AspNetCore.Mvc.Formatters.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.ErrorReport
{
    [TestClass]
    public class CalcResultErrorReportExporterTests
    {
        [TestMethod]
        public void Export_ShouldWriteExpectedHeadersToCsv()
        {
            // Arrange
            var exporter = new CalcResultErrorReportExporter();
            var stringBuilder = new StringBuilder();

            var calcResultErrorReport = new List<CalcResultErrorReport>();          

            // Act
            exporter.Export(calcResultErrorReport, stringBuilder);
            var csvOutput = stringBuilder.ToString();

            // Assert
            Assert.IsTrue(csvOutput.Contains(CommonConstants.ErrorReportHeader), "CSV should include title header.");            
            Assert.IsTrue(csvOutput.Contains(CommonConstants.ProducerId), "CSV should include ProducerId column.");
            Assert.IsTrue(csvOutput.Contains(CommonConstants.SubsidiaryId), "CSV should include BillingInstructionId column.");
            Assert.IsTrue(csvOutput.Contains(CommonConstants.ProducerOrSubsidiaryName), "CSV should include TradingName column.");
            Assert.IsTrue(csvOutput.Contains(CommonConstants.TradingName), "CSV should include Aluminium column.");
            Assert.IsTrue(csvOutput.Contains(CommonConstants.LeaverCode), "CSV should include FibreComposite column.");
            Assert.IsTrue(csvOutput.Contains(CommonConstants.ErrorCodeText), "CSV should include Glass column.");

        }

        [TestMethod]
        public void Export_ShouldHandleEmptyErrorReport()
        {
            // Arrange  
            var exporter = new CalcResultErrorReportExporter();
            var response = new List<CalcResultErrorReport>();
            var csvContent = new StringBuilder();

            // Act  
            exporter.Export(response, csvContent);

            // Assert  
            var result = csvContent.ToString();
            Assert.IsTrue(result.Contains("Error"));
            Assert.IsTrue(result.Contains("Producer ID"));
            Assert.IsTrue(result.Contains("Subsidiary ID"));
            Assert.IsTrue(result.Contains("Trading Name"));
            Assert.IsTrue(result.Contains("Producer / Subsidiary Name"));
            Assert.IsTrue(result.Contains("Leaver Code"));
            Assert.IsTrue(result.Contains("Error Code Text"));
        }
    }
}
