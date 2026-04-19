using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.LaDisposalCost;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Modulation;
using EPR.Calculator.Service.Function.Builder.Modulation;
using EPR.Calculator.API.Data.Enums;
using Newtonsoft.Json;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.Modulation
{
    [TestClass]
    public class CalcResultModulationExporterTests
    {
        private CalcResultModulationExporter exporter;

        public CalcResultModulationExporterTests()
        {
            exporter = new CalcResultModulationExporter();
        }

        [TestMethod]
        public void Modulation_Export_ShouldIncludeLaDisposalCostData_WhenNotNull()
        {
            // Arrange
            var calcResultLaDisposalCostData = new CalcResultLaDisposalCostData
            {
                CalcResultLaDisposalCostDetails = new List<CalcResultLaDisposalCostDataDetail>
                {
                    new CalcResultLaDisposalCostDataDetail
                    {
                        Material = "Aluminium",
                        Name = "",
                        England = "",
                        Wales = "",
                        Scotland = "",
                        NorthernIreland = "",
                        Total = "",
                        ProducerReportedHouseholdPackagingWasteTonnage = "26181753.11",
                        ReportedPublicBinTonnage = "24610.429", // TODO 3 dp?
                        HouseholdDrinkContainers = "",
                        LateReportingTonnage = "3500"
                    },
                    new CalcResultLaDisposalCostDataDetail
                    {
                        Material  = "Fibre composite",
                        Name = "",
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
                },
                Name = "",
            };

            var modulationResult = new ModulationResult
            {
                GreenTotal = 0m,
                AmberTotal = 0m,
                RedTotal = 0m,
                RedFactor = 1.2m,
                GreenFactor = 0.751106m,
                MaterialNames = new List<string> { "Aluminium", "Fibre composite" },
                PricePerTonnePerMaterial = new Dictionary<string, Dictionary<RagRating, decimal>>
                {
                    ["Aluminium"] = new Dictionary<RagRating, decimal>
                    {
                        [RagRating.Red] = 0.76m,
                        [RagRating.Amber] = 0.63m,
                        [RagRating.Green] = 0.47m,
                    },
                    ["Fibre composite"] = new Dictionary<RagRating, decimal>
                    {
                        [RagRating.Red] = 115.9m,
                        [RagRating.Amber] = 96.59m,
                        [RagRating.Green] = 2.55m,
                    },
                },
                CostPerMaterial = new Dictionary<string, Dictionary<RagRating, decimal>>
                {
                    ["Aluminium"] = new Dictionary<RagRating, decimal>
                    {
                        [RagRating.Red] = 6280704.41m,
                        [RagRating.Amber] = 0m,
                        [RagRating.Green] = 4955297.8m,
                    },
                    ["Fibre composite"] = new Dictionary<RagRating, decimal>
                    {
                        [RagRating.Red] = 115.9m,
                        [RagRating.Amber] = 96.59m,
                        [RagRating.Green] = 2.55m,
                    },
                }
            };

            var csvContent = new StringBuilder();

            // Act
            exporter.Export(calcResultLaDisposalCostData, modulationResult, csvContent);
            var result = csvContent.ToString();
            //Console.WriteLine($">> {JsonConvert.SerializeObject(result, Formatting.Indented)}");
            Console.WriteLine(result);

            var expected = new[] {
                new string[] {},
                new string[] {},
                new[] { "Modulation Calculation" },
                new[] { "Red Modulation Factor", "1.2" },
                new[] { "Green discount", "0.751106" },
                new[]
                {
                    "Material",
                    "Producer Household Packaging Tonnage",
                    "Public Bin Tonnage","Household Drinks Containers Tonnage",
                    "Late Reporting Tonnage",
                    "Actioned Self-Managed Consumer Waste",
                    "Producer Household Tonnage + Late Reporting Tonnage + Public Bin Tonnage + Household Drinks Containers Tonnage - Self-Managed Consumer Waste",
                    "Red + Red-Medical Household Tonnage + Red + Red-Medical Public Bin Tonnage + Red + Red-Medical Household Drinks Containers Tonnage (Net Tonnage)",
                    "Amber + Amber-Medical Household Tonnage + Amber Public Bin Tonnage + Amber-Medical Public Bin + Amber Household Drinks Containers + Amber Household Drinks Containers Tonnage (Net Tonnage)",
                    "Green + Green Medical Household + Green Public Bin + Green Medical Public Bin Tonnage+ Green Household Drinks Containers + Green Medical Household Drinks Containers (Net Tonnage)",
                    "Total Red Material at Amber Disposal Cost = Amber Material Disposal Cost x Red Material Tonnage",
                    "Total Green Material at Amber Disposal Cost = Amber Material Disposal Cost x Green Material Tonnage",
                    "Red Material Disposal Cost = Red Modulation Factor * Amber Material Disposal Cost",
                    "Amber Material Disposal Cost = Material Disposal Cost per Tonne",
                    "Green Material Disposal Cost = Green Factor * Amber Material Disposal Cost"
                },
                new[] { "Aluminium", "26181753.11", "24610.429", "", "3500", "", "0", "0", "0", "0", "6280704.41", "4955297.8", "0.76", "0.63", "0.47" },
                new[] { "Fibre composite", "401772.341", "1146.546", "", "789", "", "0", "0", "0", "0", "115.9", "2.55", "115.9", "96.59", "2.55" }
            };
            var expected2 = string.Join("\n",
                expected.Select(row => string.Join(",", row.Select(cell => $"\"{cell}\"")) + ",")
            ) + "\n";

            //Assert.AreEqual(expected2, result);

            // FluentAssertions would give the clearest diff output?
            //result.Should().Be(expected2);

            if (expected2 != result)
            {
                // show first mismatch index
                int i = 0;
                int len = Math.Min(expected2.Length, result.Length);
                while (i < len && expected2[i] == result[i]) i++;
                string msg = i == len
                    ? $"Length differs. expected.Length={expected2.Length} actual.Length={result.Length}"
                    : $"First difference at index {i}: expected=0x{(int)expected2[i]:X2} ('{expected2[i]}') actual=0x{(int)result[i]:X2} ('{result[i]}')";
                Assert.Fail(msg + Environment.NewLine + "Expected:" + Environment.NewLine + expected2 + Environment.NewLine + "Actual:" + Environment.NewLine + result);
            }
        }
    }
}
