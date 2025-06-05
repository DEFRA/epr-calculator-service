namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.LaDisposalCost
{
    using System.Text;
    using EPR.Calculator.Service.Function.Exporter.CsvExporter.LaDisposalCost;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalcResultLaDisposalCostExporterTests
    {
        private CalcResultLaDisposalCostExporter exporter;

        public CalcResultLaDisposalCostExporterTests()
        {
            exporter = new CalcResultLaDisposalCostExporter();
        }

        [TestMethod]
        public void Export_ShouldIncludeLaDisposalCostData_WhenNotNull()
        {
            // Arrange
            var calcResultLaDisposalCostData = new CalcResultLaDisposalCostData
            {
                CalcResultLaDisposalCostDetails = new List<CalcResultLaDisposalCostDataDetail>
                {
                    new CalcResultLaDisposalCostDataDetail
                    {
                        DisposalCostPricePerTonne = "20",
                        England = "EnglandTest",
                        Wales = "WalesTest",
                        Name = "ScotlandTest",
                        Scotland = "ScotlandTest",
                        Material = "Material1",
                        NorthernIreland = "NorthernIrelandTest",
                        Total = "null",
                        ProducerReportedHouseholdPackagingWasteTonnage = "null",
                        ReportedPublicBinTonnage = string.Empty,
                        HouseholdDrinkContainers = "100.12345",
                        ProducerReportedTotalTonnage = "200.12345",
                    },
                    new CalcResultLaDisposalCostDataDetail
                    {
                        DisposalCostPricePerTonne = "20",
                        England = "EnglandTest",
                        Wales = "WalesTest",
                        Name = "Material1",
                        Scotland = "ScotlandTest",
                        NorthernIreland = "NorthernIrelandTest",
                        Total = "null",
                        ProducerReportedHouseholdPackagingWasteTonnage = "null",
                        ReportedPublicBinTonnage = string.Empty,
                    },
                    new CalcResultLaDisposalCostDataDetail
                    {
                        DisposalCostPricePerTonne = "10",
                        England = "EnglandTest",
                        Wales = "WalesTest",
                        Name = "Material2",
                        Scotland = "ScotlandTest",
                        NorthernIreland = "NorthernIrelandTest",
                        Total = "100",
                        ProducerReportedHouseholdPackagingWasteTonnage = "null",
                        ReportedPublicBinTonnage = string.Empty,
                    },
                },
                Name = "LA Disposal Cost Data",
            };

            var csvContent = new StringBuilder();

            // Act
            exporter.Export(calcResultLaDisposalCostData, csvContent);
            var result = csvContent.ToString();

            // Assert
            Assert.IsTrue(result.Contains("LA Disposal Cost Data"));
            Assert.IsTrue(result.Contains("200.123"));
            Assert.IsFalse(result.Contains("200.12345"));
            Assert.IsTrue(result.Contains("100.123"));
            Assert.IsFalse(result.Contains("100.12345"));
        }
    }
}