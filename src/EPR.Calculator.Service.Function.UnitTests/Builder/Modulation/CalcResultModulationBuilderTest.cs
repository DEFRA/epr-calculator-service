using EPR.Calculator.Service.Function.Builder.Modulation;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using Moq;
using Newtonsoft.Json;
using EPR.Calculator.API.Data.Enums;
using System.Globalization;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Modulation
{
    [TestClass]
    public class CalcResultModulationBuilderTest
    {
        public CalcResultModulationBuilder builder;

        public CalcResultModulationBuilderTest()
        {
            builder = new CalcResultModulationBuilder();
        }

        private CalcResultLaDisposalCostDataDetail mkLaDisposalCost(string materialName, decimal costPerTonnage)
        {
            return new CalcResultLaDisposalCostDataDetail
            {
                Name = materialName,
                DisposalCostPricePerTonne = "£" + costPerTonnage.ToString(),
                Material = "",
                England = "",
                Wales = "",
                Scotland = "",
                NorthernIreland = "",
                Total = "",
                ProducerReportedHouseholdPackagingWasteTonnage = "",
                ReportedPublicBinTonnage = "",
                HouseholdDrinkContainers = "",
                LateReportingTonnage = "",
                TotalReportedTonnage = "",
                ProducerReportedTotalTonnage = "",
                OrderId = 0
            };
        }

        private Dictionary<RagRating, decimal> mkProducerData(decimal r, decimal rm, decimal a, decimal am, decimal g, decimal gm)
        {
            return new Dictionary<RagRating, decimal>
            {
                [RagRating.Red] = r,
                [RagRating.RedMedical] = rm,
                [RagRating.Amber] = a,
                [RagRating.AmberMedical] = am,
                [RagRating.Green] = g,
                [RagRating.GreenMedical] = gm
            };
        }

        private MaterialModulation mkMaterialModulation(decimal adc, decimal rdc, decimal gdc, decimal rt, decimal gt, decimal rAtAdc, decimal gAtAdc)
        {
            return new MaterialModulation
            {
                AmberMaterialDisposalCost = adc,
                RedMaterialDisposalCost   = rdc,
                GreenMaterialDisposalCost = gdc,
                RedMaterialTonnages       = rt,
                GreenMaterialTonnages     = gt,
                TotalRedMaterialAtAmberDisposalCost   = rAtAdc,
                TotalGreenMaterialAtAmberDisposalCost = gAtAdc
            };
        }

        [TestMethod]
        public async Task ModulationBuilder_TestCalculation()
        {
            var laDisposalCostData = new CalcResultLaDisposalCostData
            {
                Name = "",
                CalcResultLaDisposalCostDetails = new List<CalcResultLaDisposalCostDataDetail>
                {
                    mkLaDisposalCost("Aluminium"      , 100),
                    mkLaDisposalCost("Fibre composite", 130),
                    mkLaDisposalCost("Glass"          , 150),
                    mkLaDisposalCost("Paper or card"  , 200),
                    mkLaDisposalCost("Plastic"        , 250),
                    mkLaDisposalCost("Steel"          , 175),
                    mkLaDisposalCost("Wood"           , 150),
                    mkLaDisposalCost("Other materials", 400)
                },
                NetByMaterialAndRag = new Dictionary<string, Dictionary<RagRating, decimal>>
                {
                    ["Aluminium"]       = mkProducerData( 200,  20, 300,  30,  500,  50),
                    ["Fibre composite"] = mkProducerData( 250,  25,  50,   5,   50,   5),
                    ["Glass"]           = mkProducerData( 100,  10, 200,  20,  200,  20),
                    ["Paper or card"]   = mkProducerData( 100, 300, 600, 450, 1800, 600),
                    ["Plastic"]         = mkProducerData(2000, 150, 200,  75,  150, 120),
                    ["Steel"]           = mkProducerData(  15,  18,  25,  15,   40,  34),
                    ["Wood"]            = mkProducerData( 250,  15,   0,   0,    0,   0),
                    ["Other materials"] = mkProducerData(  20,  10,   0,   0,    0,   0)
                }
            };
            Console.WriteLine($">> {JsonConvert.SerializeObject(laDisposalCostData.CalcResultLaDisposalCostDetails, Formatting.Indented)}");

            var defaultParameters = new Dictionary<string, decimal> { ["REDM-RF"] = 1.2m };
            var materials = TestDataHelper.GetMaterials().ToList();
            var modulationResults = await builder.ConstructAsync(defaultParameters, materials, laDisposalCostData);
            Console.WriteLine($">> {JsonConvert.SerializeObject(modulationResults, Formatting.Indented)}");

            Assert.AreEqual(     1.2m, modulationResults.RedFactor);
            Assert.AreEqual(0.771423m, modulationResults.GreenFactor);

            var expected =
                new Dictionary<string, MaterialModulation>
                {
                    ["Aluminium"]       = mkMaterialModulation(100, 120,  77.1423m,  220,  550,  22000,  55000),
                    ["Fibre composite"] = mkMaterialModulation(130, 156, 100.2850m,  275,   55,  35750,   7150),
                    ["Glass"]           = mkMaterialModulation(150, 180, 115.7134m,  110,  220,  16500,  33000),
                    ["Paper or card"]   = mkMaterialModulation(200, 240, 154.2846m,  400, 2400,  80000, 480000),
                    ["Plastic"]         = mkMaterialModulation(250, 300, 192.8558m, 2150,  270, 537500,  67500),
                    ["Steel"]           = mkMaterialModulation(175, 210, 134.9990m,   33,   74,   5775,  12950),
                    ["Wood"]            = mkMaterialModulation(150, 180, 115.7134m,  265,    0,  39750,      0),
                    ["Other materials"] = mkMaterialModulation(400, 480, 308.5692m,   30,    0,  12000,      0)
                };

            CollectionAssert.AreEquivalent(expected.Keys.ToList(), modulationResults.MaterialModulation.Keys.ToList());

            foreach (var kvp in expected)
            {
                Assert.AreEqual(kvp.Value, modulationResults.MaterialModulation[kvp.Key], $"Value mismatch for key: {kvp.Key}");
            }
        }

        [TestMethod]
        public async Task ModulationBuilder_Factor1()
        {
            var laDisposalCostData = new CalcResultLaDisposalCostData
            {
                Name = "",
                CalcResultLaDisposalCostDetails = new List<CalcResultLaDisposalCostDataDetail>
                {
                    mkLaDisposalCost("Aluminium"      , 100),
                    mkLaDisposalCost("Fibre composite", 130),
                    mkLaDisposalCost("Glass"          , 150),
                    mkLaDisposalCost("Paper or card"  , 200),
                    mkLaDisposalCost("Plastic"        , 250),
                    mkLaDisposalCost("Steel"          , 175),
                    mkLaDisposalCost("Wood"           , 150),
                    mkLaDisposalCost("Other materials", 400)
                },
                NetByMaterialAndRag = new Dictionary<string, Dictionary<RagRating, decimal>>
                {
                    ["Aluminium"]       = mkProducerData( 200,  20, 300,  30,  500,  50),
                    ["Fibre composite"] = mkProducerData( 250,  25,  50,   5,   50,   5),
                    ["Glass"]           = mkProducerData( 100,  10, 200,  20,  200,  20),
                    ["Paper or card"]   = mkProducerData( 100, 300, 600, 450, 1800, 600),
                    ["Plastic"]         = mkProducerData(2000, 150, 200,  75,  150, 120),
                    ["Steel"]           = mkProducerData(  15,  18,  25,  15,   40,  34),
                    ["Wood"]            = mkProducerData( 250,  15,   0,   0,    0,   0),
                    ["Other materials"] = mkProducerData(  20,  10,   0,   0,    0,   0)
                }
            };

            var defaultParameters = new Dictionary<string, decimal> { ["REDM-RF"] = 1m };
            var materials = TestDataHelper.GetMaterials().ToList();
            var modulationResults = await builder.ConstructAsync(defaultParameters, materials, laDisposalCostData);

            Assert.AreEqual(1m, modulationResults.RedFactor);
            Assert.AreEqual(1m, modulationResults.GreenFactor);
            foreach (var material in materials)
            {
                var costStr = laDisposalCostData.CalcResultLaDisposalCostDetails.First(d => d.Name == material.Name).DisposalCostPricePerTonne;
                var cost = decimal.Parse(costStr!.TrimStart('£'), CultureInfo.InvariantCulture);

                var mm = modulationResults.MaterialModulation[material.Name];
                Assert.AreEqual(cost, mm.AmberMaterialDisposalCost);
                Assert.AreEqual(cost, mm.RedMaterialDisposalCost);
                Assert.AreEqual(cost, mm.GreenMaterialDisposalCost);
            }
        }

                [TestMethod]
        public async Task ModulationBuilder_NoGreen()
        {
            var laDisposalCostData = new CalcResultLaDisposalCostData
            {
                Name = "",
                CalcResultLaDisposalCostDetails = new List<CalcResultLaDisposalCostDataDetail>
                {
                    mkLaDisposalCost("Aluminium"      , 100),
                    mkLaDisposalCost("Fibre composite", 130),
                    mkLaDisposalCost("Glass"          , 150),
                    mkLaDisposalCost("Paper or card"  , 200),
                    mkLaDisposalCost("Plastic"        , 250),
                    mkLaDisposalCost("Steel"          , 175),
                    mkLaDisposalCost("Wood"           , 150),
                    mkLaDisposalCost("Other materials", 400)
                },
                NetByMaterialAndRag = new Dictionary<string, Dictionary<RagRating, decimal>>
                {
                    ["Aluminium"]       = mkProducerData( 200,  20, 300,  30, 0, 0),
                    ["Fibre composite"] = mkProducerData( 250,  25,  50,   5, 0, 0),
                    ["Glass"]           = mkProducerData( 100,  10, 200,  20, 0, 0),
                    ["Paper or card"]   = mkProducerData( 100, 300, 600, 450, 0, 0),
                    ["Plastic"]         = mkProducerData(2000, 150, 200,  75, 0, 0),
                    ["Steel"]           = mkProducerData(  15,  18,  25,  15, 0, 0),
                    ["Wood"]            = mkProducerData( 250,  15,   0,   0, 0, 0),
                    ["Other materials"] = mkProducerData(  20,  10,   0,   0, 0, 0)
                }
            };

            var redFactor = 1.2m;
            var defaultParameters = new Dictionary<string, decimal> { ["REDM-RF"] = redFactor };
            var materials = TestDataHelper.GetMaterials().ToList();
            var modulationResults = await builder.ConstructAsync(defaultParameters, materials, laDisposalCostData);

            Assert.AreEqual(redFactor, modulationResults.RedFactor);
            Assert.AreEqual(1.0m, modulationResults.GreenFactor);
            foreach (var material in materials)
            {
                var costStr = laDisposalCostData.CalcResultLaDisposalCostDetails.First(d => d.Name == material.Name).DisposalCostPricePerTonne;
                var cost = decimal.Parse(costStr!.TrimStart('£'), CultureInfo.InvariantCulture);

                var mm = modulationResults.MaterialModulation[material.Name];
                Assert.AreEqual(cost, mm.AmberMaterialDisposalCost);
                Assert.AreEqual(cost * redFactor, mm.RedMaterialDisposalCost);
                Assert.AreEqual(cost, mm.AmberMaterialDisposalCost);
            }
        }
    }
}
