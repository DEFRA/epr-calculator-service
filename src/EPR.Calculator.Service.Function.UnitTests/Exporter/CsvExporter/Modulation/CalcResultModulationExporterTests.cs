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
        public void Modulation_Export_CSV()
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
                        ProducerReportedHouseholdPackagingWasteTonnage = "26181753.110",
                        ReportedPublicBinTonnage = "24610.429",
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
                    new CalcResultLaDisposalCostDataDetail
                    {
                        Material  = "",
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
                Name = "",
                NetByMaterialAndRag = new Dictionary<string, Dictionary<RagRating, decimal>>
                {
                    ["Aluminium"] = new Dictionary<RagRating, decimal>
                    {
                        [RagRating.Red]   = 21000000.123m,
                        [RagRating.Amber] =  5000000.234m,
                        [RagRating.Green] =   209863.182m,
                    },
                    ["Fibre composite"] = new Dictionary<RagRating, decimal>
                    {
                        [RagRating.Red]   = 3001.333m,
                        [RagRating.Amber] = 400000.222m,
                        [RagRating.Green] = 706.332m,
                    },
                }
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
                        [RagRating.Red] = 26181753.110m,
                        [RagRating.Amber] = 0.6322m,
                        [RagRating.Green] = 0.4675m,
                    },
                    ["Fibre composite"] = new Dictionary<RagRating, decimal>
                    {
                        [RagRating.Red] = 115.9m,
                        [RagRating.Amber] = 96.5911m,
                        [RagRating.Green] = 2.5511m,
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
                    "Green Material Disposal Cost = Green Modulation Factor * Amber Material Disposal Cost"
                },
                new[] { "Aluminium"      , "26181753.110", "24610.429", "", "3500", "", "26209863.54", "21000000.12", "5000000.23", "209863.18" , "6280704.41", "4955297.8" , "26181753.11",  "0.63", "0.47" },
                new[] { "Fibre composite",   "401772.341",  "1146.546", "",  "789", "",   "403707.89",     "3001.33",  "400000.22", "706.33"    ,      "115.9",       "2.55",      "115.9" , "96.59", "2.55" },
                new[] { "Total"          , "26583525.451", "25756.975", "", "4289", "", "26613571.43", "21003001.46", "5400000.46", "210569.51" , "6280820.31", "4955300.35", "26181869.01", "97.22", "3.02" },
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
                    var data = $"Expected row: {expectedRow}\nActual row: {actualRow}";

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
