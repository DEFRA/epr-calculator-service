using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.Service.Function.Builder.Modulation;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.Builder.Modulation;
using EPR.Calculator.Service.Function.UnitTests.Builder;
using Moq;
using NetTopologySuite.Operation.Buffer;
using Newtonsoft.Json;
using System.Globalization;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Modulation
{
    [TestClass]
    public class CalcResultModulationBuilderTest
    {
        private CalcResultModulationBuilder builder;

        private static List<MaterialDetail> materials = TestDataHelper.GetMaterials().ToList();

        private MaterialDetail al = materials.First(m => m.Code == "AL");
        private MaterialDetail fc = materials.First(m => m.Code == "FC");
        private MaterialDetail gl = materials.First(m => m.Code == "GL");
        private MaterialDetail pc = materials.First(m => m.Code == "PC");
        private MaterialDetail pl = materials.First(m => m.Code == "PL");
        private MaterialDetail st = materials.First(m => m.Code == "ST");
        private MaterialDetail wd = materials.First(m => m.Code == "WD");
        private MaterialDetail ot = materials.First(m => m.Code == "OT");

        public CalcResultModulationBuilderTest()
        {
            builder = new CalcResultModulationBuilder();
        }

        private CalcResultLaDisposalCostDataDetail mkLaDisposalCost(MaterialDetail material, decimal costPerTonnage)
        {
            return new CalcResultLaDisposalCostDataDetail
            {
                Name = material.Name,
                DisposalCostPricePerTonne = "£" + costPerTonnage.ToString(),
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

        private SelfManagedConsumerWasteData mkSmcwData(decimal red, decimal amber, decimal green)
        {
            return new SelfManagedConsumerWasteData
            {
                SelfManagedConsumerWasteTonnage = 0m,
                ActionedSelfManagedConsumerWasteTonnage = null,
                ResidualSelfManagedConsumerWasteTonnage = null,
                NetReportedTonnage = (total: null, red: red, amber: amber, green: green)
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
                    mkLaDisposalCost(al, 100),
                    mkLaDisposalCost(fc, 130),
                    mkLaDisposalCost(gl, 150),
                    mkLaDisposalCost(pc, 200),
                    mkLaDisposalCost(pl, 250),
                    mkLaDisposalCost(st, 175),
                    mkLaDisposalCost(wd, 150),
                    mkLaDisposalCost(ot, 400)
                }
            };

            var smcw = new SelfManagedConsumerWaste
            {
                ProducerTotals = new List<ProducerSelfManagedConsumerWaste>(),
                OverallTotalPerMaterials = new Dictionary<string, SelfManagedConsumerWasteData>
                {
                    [al.Code] = mkSmcwData(red:  220, amber:  330, green:  550),
                    [fc.Code] = mkSmcwData(red:  275, amber:   55, green:   55),
                    [gl.Code] = mkSmcwData(red:  110, amber:  220, green:  220),
                    [pc.Code] = mkSmcwData(red:  400, amber: 1050, green: 2400),
                    [pl.Code] = mkSmcwData(red: 2150, amber:  275, green:  270),
                    [st.Code] = mkSmcwData(red:   33, amber:   40, green:   74),
                    [wd.Code] = mkSmcwData(red:  265, amber:    0, green:    0),
                    [ot.Code] = mkSmcwData(red:   30, amber:    0, green:    0)
                }
            };

            var defaultParameters = new Dictionary<string, decimal> { ["REDM-RF"] = 1.2m };
            var modulationResults = await builder.ConstructAsync(defaultParameters, TestDataHelper.GetMaterials(), laDisposalCostData, smcw);
            //Console.WriteLine($">> {JsonConvert.SerializeObject(modulationResults, Formatting.Indented)}");

            Assert.AreEqual(     1.2m, modulationResults.RedFactor);
            Assert.AreEqual(0.771423m, modulationResults.GreenFactor);

            var expected =
                new Dictionary<MaterialDetail, MaterialModulation>
                {
                    [al] = mkMaterialModulation(100, 120,  77.1423m,  220,  550,  22000,  55000),
                    [fc] = mkMaterialModulation(130, 156, 100.2850m,  275,   55,  35750,   7150),
                    [gl] = mkMaterialModulation(150, 180, 115.7134m,  110,  220,  16500,  33000),
                    [pc] = mkMaterialModulation(200, 240, 154.2846m,  400, 2400,  80000, 480000),
                    [pl] = mkMaterialModulation(250, 300, 192.8558m, 2150,  270, 537500,  67500),
                    [st] = mkMaterialModulation(175, 210, 134.9990m,   33,   74,   5775,  12950),
                    [wd] = mkMaterialModulation(150, 180, 115.7134m,  265,    0,  39750,      0),
                    [ot] = mkMaterialModulation(400, 480, 308.5692m,   30,    0,  12000,      0)
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
                    mkLaDisposalCost(al, 100),
                    mkLaDisposalCost(fc, 130),
                    mkLaDisposalCost(gl, 150),
                    mkLaDisposalCost(pc, 200),
                    mkLaDisposalCost(pl, 250),
                    mkLaDisposalCost(st, 175),
                    mkLaDisposalCost(wd, 150),
                    mkLaDisposalCost(ot, 400)
                }
            };
            var smcw = new SelfManagedConsumerWaste
            {
                ProducerTotals = new List<ProducerSelfManagedConsumerWaste>(),
                OverallTotalPerMaterials = new Dictionary<string, SelfManagedConsumerWasteData>
                {
                    [al.Code] = mkSmcwData(red:  220, amber:  330, green:  550),
                    [fc.Code] = mkSmcwData(red:  275, amber:   55, green:   55),
                    [gl.Code] = mkSmcwData(red:  110, amber:  220, green:  220),
                    [pc.Code] = mkSmcwData(red:  400, amber: 1050, green: 2400),
                    [pl.Code] = mkSmcwData(red: 2150, amber:  275, green:  270),
                    [st.Code] = mkSmcwData(red:   33, amber:   40, green:   74),
                    [wd.Code] = mkSmcwData(red:  265, amber:    0, green:    0),
                    [ot.Code] = mkSmcwData(red:   30, amber:    0, green:    0)
                }
            };

            var defaultParameters = new Dictionary<string, decimal> { ["REDM-RF"] = 1m };
            var modulationResults = await builder.ConstructAsync(defaultParameters, materials, laDisposalCostData, smcw);

            Assert.AreEqual(1m, modulationResults.RedFactor);
            Assert.AreEqual(1m, modulationResults.GreenFactor);
            foreach (var material in materials)
            {
                var costStr = laDisposalCostData.CalcResultLaDisposalCostDetails.First(d => d.Name == material.Name).DisposalCostPricePerTonne;
                var cost = decimal.Parse(costStr!.TrimStart('£'), CultureInfo.InvariantCulture);

                var mm = modulationResults.MaterialModulation[material];
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
                    mkLaDisposalCost(al, 100),
                    mkLaDisposalCost(fc, 130),
                    mkLaDisposalCost(gl, 150),
                    mkLaDisposalCost(pc, 200),
                    mkLaDisposalCost(pl, 250),
                    mkLaDisposalCost(st, 175),
                    mkLaDisposalCost(wd, 150),
                    mkLaDisposalCost(ot, 400)
                }
            };
            var smcw = new SelfManagedConsumerWaste
            {
                ProducerTotals = new List<ProducerSelfManagedConsumerWaste>(),
                OverallTotalPerMaterials = new Dictionary<string, SelfManagedConsumerWasteData>
                {
                    [al.Code] = mkSmcwData(red:  220, amber:  330, green: 0),
                    [fc.Code] = mkSmcwData(red:  275, amber:   55, green: 0),
                    [gl.Code] = mkSmcwData(red:  110, amber:  220, green: 0),
                    [pc.Code] = mkSmcwData(red:  400, amber: 1050, green: 0),
                    [pl.Code] = mkSmcwData(red: 2150, amber:  275, green: 0),
                    [st.Code] = mkSmcwData(red:   33, amber:   40, green: 0),
                    [wd.Code] = mkSmcwData(red:  265, amber:    0, green: 0),
                    [ot.Code] = mkSmcwData(red:   30, amber:    0, green: 0)
                }
            };

            var redFactor = 1.2m;
            var defaultParameters = new Dictionary<string, decimal> { ["REDM-RF"] = redFactor };
            var modulationResults = await builder.ConstructAsync(defaultParameters, materials, laDisposalCostData, smcw);

            Assert.AreEqual(redFactor, modulationResults.RedFactor);
            Assert.AreEqual(1.0m, modulationResults.GreenFactor);
            foreach (var material in materials)
            {
                var costStr = laDisposalCostData.CalcResultLaDisposalCostDetails.First(d => d.Name == material.Name).DisposalCostPricePerTonne;
                var cost = decimal.Parse(costStr!.TrimStart('£'), CultureInfo.InvariantCulture);

                var mm = modulationResults.MaterialModulation[material];
                Assert.AreEqual(cost, mm.AmberMaterialDisposalCost);
                Assert.AreEqual(cost * redFactor, mm.RedMaterialDisposalCost);
                Assert.AreEqual(cost, mm.AmberMaterialDisposalCost);
            }
        }
    }
}
