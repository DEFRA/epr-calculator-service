using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.Service.Function.Builder.Modulation;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.LaDisposalCost;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Modulation;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.Builder;
using Newtonsoft.Json;
using System.Text;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.Modulation
{
    [TestClass]
    public class CalcResultModulationExporterTests
    {
        private CalcResultModulationExporter exporter;

        private MaterialDetail al = TestDataHelper.GetMaterials().First(m => m.Code == "AL");
        private MaterialDetail fc = TestDataHelper.GetMaterials().First(m => m.Code == "FC");

        public CalcResultModulationExporterTests()
        {
            exporter = new CalcResultModulationExporter();
        }

        [TestMethod]
        public void Modulation_Export_CSV()
        {
            // Arrange
            var calcResultLaDisposalCostData = new CalcResultLaDisposalCostData
            {
                CalcResultLaDisposalCostDetails = new List<CalcResultLaDisposalCostDataDetail>
                {
                    new CalcResultLaDisposalCostDataDetail
                    {
                        Name = al.Name,
                        England = "",
                        Wales = "",
                        Scotland = "",
                        NorthernIreland = "",
                        Total = "",
                        ProducerReportedHouseholdPackagingWasteTonnage = "26181753.110",
                        ReportedPublicBinTonnage = "24610.429",
                        HouseholdDrinkContainers = "",
                        LateReportingTonnage = "3500"
                    },
                    new CalcResultLaDisposalCostDataDetail
                    {
                        Name = fc.Name,
                        England = "",
                        Wales = "",
                        Scotland = "",
                        NorthernIreland = "",
                        Total = "",
                        ProducerReportedHouseholdPackagingWasteTonnage = "401772.341",
                        ReportedPublicBinTonnage = "1146.546",
                        HouseholdDrinkContainers = "",
                        LateReportingTonnage = "789"
                    },
                    new CalcResultLaDisposalCostDataDetail
                    {
                        Name = "Total",
                        England = "",
                        Wales = "",
                        Scotland = "",
                        NorthernIreland = "",
                        Total = "",
                        ProducerReportedHouseholdPackagingWasteTonnage = "26583525.451",
                        ReportedPublicBinTonnage = "25756.975",
                        HouseholdDrinkContainers = "",
                        LateReportingTonnage = "4289"
                    }
                },
                Name = ""
            };

            SelfManagedConsumerWasteData mkSmcwData(decimal red, decimal amber, decimal green, decimal? actionedSmcw, decimal? residualSmcw)
            {
                return new SelfManagedConsumerWasteData
                {
                    SelfManagedConsumerWasteTonnage = 0m,
                    ActionedSelfManagedConsumerWasteTonnage = actionedSmcw,
                    ResidualSelfManagedConsumerWasteTonnage = residualSmcw,
                    NetReportedTonnage = (total: null, red: red, amber: amber, green: green)
                };
            };

            var smcw = new SelfManagedConsumerWaste
            {
                ProducerTotals = new List<ProducerSelfManagedConsumerWaste>(),
                OverallTotalPerMaterials = new Dictionary<string, SelfManagedConsumerWasteData>
                {
                    [al.Code] = mkSmcwData(red: 21000000.123m, amber: 5000000.234m, green: 209863.182m, actionedSmcw: 123.456m, residualSmcw: 654.321m),
                    [fc.Code] = mkSmcwData(red:     3001.333m, amber:  400000.222m, green:    706.332m, actionedSmcw: null    , residualSmcw: null    )
                }
            };

            var modulationResult = new ModulationResult
            {
                RedFactor = 1.2m,
                GreenFactor = 0.751106m,
                MaterialModulation = new Dictionary<MaterialDetail, MaterialModulation>
                {
                    [al] = new MaterialModulation
                    {
                        AmberMaterialDisposalCost = 0.631234m,
                        RedMaterialDisposalCost   = 0.761234m,
                        GreenMaterialDisposalCost = 0.471234m,
                        AmberMaterialTonnages = 5000000.23m,
                        RedMaterialTonnages   = 21000000.12m,
                        GreenMaterialTonnages = 209863.18m,
                        TotalRedMaterialAtAmberDisposalCost   = 6280704.41m,
                        TotalGreenMaterialAtAmberDisposalCost = 4955297.80m
                    },
                    [fc] = new MaterialModulation
                    {
                        AmberMaterialDisposalCost =  96.591234m,
                        RedMaterialDisposalCost   = 115.901234m,
                        GreenMaterialDisposalCost =  72.551234m,
                        AmberMaterialTonnages  = 400000.22m,
                        RedMaterialTonnages   = 3001.33m,
                        GreenMaterialTonnages = 706.33m,
                        TotalRedMaterialAtAmberDisposalCost   = 14817325.23m,
                        TotalGreenMaterialAtAmberDisposalCost = 11697888.16m
                    }
                }
            };

            var csvContent = new StringBuilder();

            // Act
            exporter.Export(calcResultLaDisposalCostData, smcw, modulationResult, csvContent);
            var result = csvContent.ToString().Split("\n").Select(s => s.TrimEnd(',')).ToArray();
            //Console.WriteLine($">> {JsonConvert.SerializeObject(result, Formatting.Indented)}");
            Console.WriteLine(string.Join("\n", result));

            var expected = new[] {
                new string[] {},
                new string[] {},
                new[] { "Modulation Calculation" },
                new[] { "Red Modulation Factor", "1.2" },
                new[] { "Green Modulation Factor", "0.751106" },
                new[]
                {
                    "Material",
                    "Producer Household Packaging Tonnage",
                    "Public Bin Tonnage",
                    "Household Drinks Containers Tonnage",
                    "Late Reporting Tonnage",
                    "Actioned Self-Managed Consumer Waste",
                    "Producer Household Tonnage + Late Reporting Tonnage + Public Bin Tonnage + Household Drinks Containers Tonnage - Self-Managed Consumer Waste",
                    "Red + Red Medical Household Tonnage + Red + Red Medical Public Bin Tonnage + Red + Red Medical Household Drinks Containers Tonnage (Net Tonnage)",
                    "Amber + Amber Medical Household Tonnage + Amber + Amber Medical Public Bin Tonnage + Amber + Amber Medical Household Drinks Containers Tonnage (Net Tonnage)",
                    "Green + Green Medical Household Tonnage + Green + Green Medical Public Bin Tonnage + Green + Green Medical Household Drinks Containers Tonnage (Net Tonnage)",
                    "Total Red Material at Amber Disposal Cost = Amber Material Disposal Cost x Red Material Tonnage",
                    "Total Green Material at Amber Disposal Cost = Amber Material Disposal Cost x Green Material Tonnage",
                    "Red Material Disposal Cost = Red Modulation Factor * Amber Material Disposal Cost",
                    "Amber Material Disposal Cost = Material Disposal Cost per Tonne",
                    "Green Material Disposal Cost = Green Modulation Factor * Amber Material Disposal Cost"
                },
                new[] { "Aluminium"      , "26181753.110", "24610.429", "", "3500", "123.456", "26209863.53", "21000000.12", "5000000.23", "209863.18",  "£6280704.41",  "£4955297.80",   "£0.7612",  "£0.6312",  "£0.4712" },
                new[] { "Fibre composite",   "401772.341",  "1146.546", "",  "789",       "0",   "403707.88",     "3001.33",  "400000.22",    "706.33", "£14817325.23", "£11697888.16", "£115.9012", "£96.5912", "£72.5512" },
                new[] { "Total"          , "26583525.451", "25756.975", "", "4289", "123.456", "26613571.43", "21003001.46", "5400000.46", "210569.51", "£21098029.64", "£16653185.96" },
                new string[] { }
            };


            var expected2 =
                expected.Select(row => string.Join(",", row.Select(cell => $"\"{cell}\""))).ToArray();

            // FluentAssertions would give the clearest diff output?
            //result.Should().Be(expected2);

            void assert(string[] expected, string[] actual)
            {
                Assert.AreEqual(expected.Length, actual.Length, $"CSV sizes differ\nExpected: {expected}\nActual: {actual}");

                for (int i = 0; i < expected.Length; i++)
                {
                    var expectedRow = expected[i];
                    var actualRow = actual[i];
                    var data = $"Expected row: {expectedRow}\nActual row  : {actualRow}";

                    for (int j = 0; j < expectedRow.Length; j++)
                    {
                        var exp = expectedRow[j];
                        var act = actualRow[j];
                        Assert.AreEqual(exp, act, $"First difference on Row {i}, index {j}: expected '{exp}' but was '{act}'.\n{data}");
                    }
                }
            };

            assert(expected2, result);
        }
    }
}
