using EPR.Calculator.Service.Function.Builder.Modulation;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using Moq;
using Newtonsoft.Json;
using EPR.Calculator.API.Data.Enums;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Modulation
{
    [TestClass]
    public class CalcResultModulationBuilderTest
    {
        public CalcResultModulationBuilder builder;
        private Mock<IMaterialService> materialServiceMock;

        public CalcResultModulationBuilderTest()
        {
            materialServiceMock = new Mock<IMaterialService>();
            materialServiceMock.Setup(t => t.GetMaterials()).ReturnsAsync(TestDataHelper.GetMaterials().ToList());

            builder = new CalcResultModulationBuilder(materialServiceMock.Object);
        }

        public CalcResultLaDisposalCostDataDetail mkLaDisposalCost(string materialName, decimal costPerTonnage)
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

        public Dictionary<RagRating, decimal> mkProducerData(decimal r, decimal rm, decimal a, decimal am, decimal g, decimal gm)
        {
            return new Dictionary<RagRating, decimal>
            {
                [RagRating.Red         ] = r,
                [RagRating.RedMedical  ] = rm,
                [RagRating.Amber       ] = a,
                [RagRating.AmberMedical] = am,
                [RagRating.Green       ] = g,
                [RagRating.GreenMedical] = gm
            };
        }

        private void assertPricePerTonnePerMaterial(Dictionary<string, (decimal, decimal, decimal)> expected, ModulationResult modulationResults)
        {
            foreach (var entry in expected)
            {
                var material = entry.Key;
                var (r, a, g) = entry.Value;

                Assert.AreEqual(r, modulationResults.PricePerTonnePerMaterial[material][RagRating.Red], $"For {material}");
                Assert.AreEqual(r, modulationResults.PricePerTonnePerMaterial[material][RagRating.RedMedical], $"For {material}");
                Assert.AreEqual(a, modulationResults.PricePerTonnePerMaterial[material][RagRating.Amber], $"For {material}");
                Assert.AreEqual(a, modulationResults.PricePerTonnePerMaterial[material][RagRating.AmberMedical], $"For {material}");
                Assert.AreEqual(g, modulationResults.PricePerTonnePerMaterial[material][RagRating.Green], $"For {material}");
                Assert.AreEqual(g, modulationResults.PricePerTonnePerMaterial[material][RagRating.GreenMedical], $"For {material}");
            }
        }

        private void assertCostPerMaterial(Dictionary<string, (decimal, decimal, decimal)> expected, ModulationResult modulationResults)
        {
            foreach (var entry in expected)
            {
                var material = entry.Key;
                var (r, a, g) = entry.Value;

                Assert.AreEqual(r, modulationResults.CostPerMaterial[material][RagRating.Red], $"For {material}");
                Assert.AreEqual(r, modulationResults.CostPerMaterial[material][RagRating.RedMedical], $"For {material}");
                Assert.AreEqual(a, modulationResults.CostPerMaterial[material][RagRating.Amber], $"For {material}");
                Assert.AreEqual(a, modulationResults.CostPerMaterial[material][RagRating.AmberMedical], $"For {material}");
                Assert.AreEqual(g, modulationResults.CostPerMaterial[material][RagRating.Green], $"For {material}");
                Assert.AreEqual(g, modulationResults.CostPerMaterial[material][RagRating.GreenMedical], $"For {material}");
            }
        }


        [TestMethod]
        public async Task Modulation_Test1()
        {
            var laDisposalCostData = new CalcResultLaDisposalCostData
            {
                Name = "",
                CalcResultLaDisposalCostDetails = new List<CalcResultLaDisposalCostDataDetail>
                {
                    mkLaDisposalCost("Aluminium", 100),
                    mkLaDisposalCost("Fibre composite", 130),
                    mkLaDisposalCost("Glass", 150),
                    mkLaDisposalCost("Paper or card", 200),
                    mkLaDisposalCost("Plastic", 250),
                    mkLaDisposalCost("Steel", 175),
                    mkLaDisposalCost("Wood", 150),
                    mkLaDisposalCost("Other materials", 400)
                },
                NetByMaterialAndRag = new Dictionary<string, Dictionary<RagRating, decimal>>
                {
                    ["Aluminium"      ] = mkProducerData( 200,  20, 300,  30,  500,  50),
                    ["Fibre composite"] = mkProducerData( 250,  25,  50,   5,   50,   5),
                    ["Glass"          ] = mkProducerData( 100,  10, 200,  20,  200,  20),
                    ["Paper or card"  ] = mkProducerData( 100, 300, 600, 450, 1800, 600),
                    ["Plastic"        ] = mkProducerData(2000, 150, 200,  75,  150, 120),
                    ["Steel"          ] = mkProducerData(  15,  18,  25,  15,   40,  34),
                    ["Wood"           ] = mkProducerData( 250,  15,   0,   0,    0,   0),
                    ["Other materials"] = mkProducerData(  20,  10,   0,   0,    0,   0)
                }
            };
            Console.WriteLine($">> {JsonConvert.SerializeObject(laDisposalCostData.CalcResultLaDisposalCostDetails, Formatting.Indented)}");

            var defaultParameters = new Dictionary<string, decimal> { ["REDM-RF"] = 1.2m };
            var modulationResults = await builder.ConstructAsync(laDisposalCostData, defaultParameters);
            Console.WriteLine($">> {JsonConvert.SerializeObject(modulationResults, Formatting.Indented)}");

            //Assert.AreEqual(655600m, modulationResults.GreenTotal);
            //Assert.AreEqual(358900m, modulationResults.AmberTotal);
            //Assert.AreEqual(899130m, modulationResults.RedTotal);
            Assert.AreEqual(0.2285768761439902379499694936m, 1 - modulationResults.GreenFactor);

            assertPricePerTonnePerMaterial(
                new Dictionary<string, (decimal, decimal, decimal)>
                {
                    ["Aluminium"]       = (120, 100,  77.1423m),
                    ["Fibre composite"] = (156, 130, 100.2850m),
                    ["Glass"]           = (180, 150, 115.7135m),
                    ["Paper or card"]   = (240, 200, 154.2846m),
                    ["Plastic"]         = (300, 250, 192.8558m),
                    ["Steel"]           = (210, 175, 134.9990m),
                    ["Wood"]            = (180, 150, 115.7135m),
                    ["Other materials"] = (480, 400, 308.5692m),
                },
                modulationResults
            );

            assertCostPerMaterial(
                new Dictionary<string, (decimal, decimal, decimal)>
                {
                    ["Aluminium"] = (26400, 33000, 42428.271812080536912751677852m),
                    ["Fibre composite"] = (42900, 7150, 5515.6753355704697986577181208m),
                    ["Glass"] = (19800, 33000, 25456.963087248322147651006711m),
                    ["Paper or card"] = (96000, 210000, 370283.09945088468578401464307m),
                    ["Plastic"] = (645000, 68750, 52071.060860280658938377059182m),
                    ["Steel"] = (6930, 7000, 9989.929453935326418547895058m),
                    ["Wood"] = (47700, 0, 0m),
                    ["Other materials"] = (14400, 0, 0m),
                },
                modulationResults
            );
        }
    }
}
