using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.LaDisposalCost;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.LaDisposalCost
{
    [TestClass]
    public class CalcResultLaDisposalCostExporterTests
    {
        private CalcResultLaDisposalCostExporter exporter;

        public CalcResultLaDisposalCostExporterTests()
        {
            exporter = new CalcResultLaDisposalCostExporter();
        }

        [TestMethod]
        public void Export_ShouldIncludeLaDisposalCostData_PreModulation()
        {
            // Arrange
            var calcResultLaDisposalCostData = GetCalcResultLaDisposalCostData();

            var csvContent = new StringBuilder();

            // Act
            exporter.Export(applyModulation: false, calcResultLaDisposalCostData, csvContent);

            // Assert
            var result = csvContent.ToString().Split("\n").Select(s => s.TrimEnd(',')).ToArray();
            Console.WriteLine(string.Join("\n", result));

            var expected = new[] {
                new string[] {},
                new string[] {},
                new[] { "LA Disposal Cost Data" },
                new[] { "Material",
                        "England",
                        "Wales",
                        "Scotland",
                        "Northern Ireland",
                        "Total",
                        "Producer Household Packaging Tonnage",
                        "Public Bin Tonnage",
                        "Household Drinks Containers Tonnage",
                        "Late Reporting Tonnage",
                        "Producer Household Tonnage + Late Reporting Tonnage + Public Bin Tonnage + Household Drinks Containers Tonnage",
                        "Disposal Cost Price Per Tonne"
                },
                new[] { "Material1","0","0","0","0",  "0","0","0","100.123",null,"200.123","20" },
                new[] { "Material2","0","0","0","0","100","0","0",      "0",null,      "0","10" },
                new[] { "Total"    ,"0","0","0","0","100","0","0","100.123",null,      "0","10" },
                new string[] { }
            };

            CsvTestUtils.AssertCsv(expected, result);
        }

                [TestMethod]
        public void Export_ShouldIncludeLaDisposalCostData_Modulation()
        {
            // Arrange
            var calcResultLaDisposalCostData = GetCalcResultLaDisposalCostData();

            var csvContent = new StringBuilder();

            // Act
            exporter.Export(applyModulation: true, calcResultLaDisposalCostData, csvContent);

            // Assert
            var result = csvContent.ToString().Split("\n").Select(s => s.TrimEnd(',')).ToArray();
            Console.WriteLine(string.Join("\n", result));

            var expected = new[] {
                new string[] {},
                new string[] {},
                new[] { "LA Disposal Cost Data" },
                new[] { "Material",
                        "England",
                        "Wales",
                        "Scotland",
                        "Northern Ireland",
                        "Total",
                        "Producer Household Packaging Tonnage",
                        "Public Bin Tonnage",
                        "Household Drinks Containers Tonnage",
                        "Late Reporting Tonnage",
                        "Actioned Self Managed Consumer Waste",
                        "Net Tonnage + Late Reporting Tonnage",
                        "Disposal Cost Price Per Tonne"
                },
                new[] { "Material1","0","0","0","0",  "0","0","0","100.123",null,   "0","200.123","20" },
                new[] { "Material2","0","0","0","0","100","0","0",      "0",null,"1.23",      "0","10" },
                new[] { "Total"    ,"0","0","0","0","100","0","0","100.123",null,"1.23",      "0","10" },
                new string[] { }
            };

            CsvTestUtils.AssertCsv(expected, result);
        }

        private CalcResultLaDisposalCostData GetCalcResultLaDisposalCostData()
        {
            return new CalcResultLaDisposalCostData
            {
                ByMaterial = new Dictionary<MaterialDetail, CalcResultLaDisposalCostDataDetail>
                {
                    [new MaterialDetail { Name = "Material1", Code = "M1", Description = "Material1" }] = new CalcResultLaDisposalCostDataDetail
                    {
                        DisposalCostPricePerTonne = 20,
                        England = 0m,
                        Wales = 0m,
                        Scotland = 0m,
                        NorthernIreland = 0m,
                        Total = 0m,
                        ProducerReportedHouseholdPackagingWasteTonnage = 0m,
                        ReportedPublicBinTonnage = 0m,
                        HouseholdDrinkContainers = 100.12345m,
                        ProducerReportedTotalTonnage = 200.12345m,
                    },
                    [new MaterialDetail { Name = "Material2", Code = "M2", Description = "Material2" }] = new CalcResultLaDisposalCostDataDetail
                    {
                        DisposalCostPricePerTonne = 10,
                        England = 0m,
                        Wales = 0m,
                        Scotland = 0m,
                        NorthernIreland = 0m,
                        Total = 100,
                        ProducerReportedHouseholdPackagingWasteTonnage = 0m,
                        ReportedPublicBinTonnage = 0m,
                        ActionedSelfManagedConsumerWasteTonnage = 1.23m
                    }
                },
                Total = new CalcResultLaDisposalCostDataDetail
                {
                    DisposalCostPricePerTonne = 10,
                    England = 0m,
                    Wales = 0m,
                    Scotland = 0m,
                    NorthernIreland = 0m,
                    Total = 100,
                    ProducerReportedHouseholdPackagingWasteTonnage = 0m,
                    ReportedPublicBinTonnage = 0m,
                    HouseholdDrinkContainers = 100.12345m,
                    ActionedSelfManagedConsumerWasteTonnage = 1.23m
                }
            };
        }
    }
}
