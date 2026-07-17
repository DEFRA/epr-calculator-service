using EPR.Calculator.Service.Function.Builder.Modulation;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Modulation;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class CalcResultModulationBuilderTest
{
    private static readonly IImmutableList<MaterialDetail> materials = TestDataHelper.GetMaterialDetails();

    private readonly MaterialDetail al = materials.First(m => m.Code == "AL");
    private readonly CalcResultModulationBuilder builder;
    private readonly MaterialDetail fc = materials.First(m => m.Code == "FC");
    private readonly MaterialDetail gl = materials.First(m => m.Code == "GL");

    private readonly IReadOnlyDictionary<string, string> lateReportingTonnageDict;
    private readonly MaterialDetail ot = materials.First(m => m.Code == "OT");
    private readonly MaterialDetail pc = materials.First(m => m.Code == "PC");
    private readonly MaterialDetail pl = materials.First(m => m.Code == "PL");
    private readonly MaterialDetail st = materials.First(m => m.Code == "ST");
    private readonly MaterialDetail wd = materials.First(m => m.Code == "WD");

    public CalcResultModulationBuilderTest()
    {
        builder = new CalcResultModulationBuilder();

        lateReportingTonnageDict = materials
            .SelectMany(m => new[]
            {
                ($"LRET-{m.Code}-R", "1"),
                ($"LRET-{m.Code}"  , "2"),
                ($"LRET-{m.Code}-G", "3")
            })
            .ToDictionary(t => t.Item1, t => t.Item2);
    }

    private static CalcResultLaDisposalCostDataDetail MkLaDisposalCost(decimal costPerTonnage) =>
        new()
        {
            Cost = ByCountryCost.Empty with { England = 100 * costPerTonnage },
            HouseholdPackagingWasteTonnage = 100,
            PublicBinTonnage = 0,
            HouseholdDrinkContainersTonnage = 0
        };

    private SelfManagedConsumerWasteData mkSmcw(decimal red, decimal amber, decimal green)
    {
        return new SelfManagedConsumerWasteData
        {
            SmcwTonnage = 0m,
            ActionedSmcwTonnage = new RamTonnageGroup { Total = null, Red = null, Amber = null, Green = null },
            ResidualSmcwTonnage = null,
            NetTonnage = new RamTonnageGroup { Total = null, Red = red, Amber = amber, Green = green }
        };
    }

    private ModulationDetail mkModulationDetail(decimal adc, decimal rdc, decimal gdc, decimal at, decimal rt, decimal gt, decimal rAtAdc, decimal gAtAdc)
    {
        return new ModulationDetail
        {
            RedMaterialDisposalCost = rdc,
            AmberMaterialDisposalCost = adc,
            GreenMaterialDisposalCost = gdc,
            RedMaterialTonnages = rt,
            AmberMaterialTonnages = at,
            GreenMaterialTonnages = gt,
            TotalRedMaterialAtAmberDisposalCost = rAtAdc,
            TotalGreenMaterialAtAmberDisposalCost = gAtAdc
        };
    }

    [TestMethod]
    public async Task ModulationBuilder_TestCalculation()
    {
        var laDisposalCostData = new CalcResultLaDisposalCostData
        {
            ByMaterial = new Dictionary<string, CalcResultLaDisposalCostDataDetail>
            {
                [al.Code] = MkLaDisposalCost(100),
                [fc.Code] = MkLaDisposalCost(130),
                [gl.Code] = MkLaDisposalCost(150),
                [pc.Code] = MkLaDisposalCost(200),
                [pl.Code] = MkLaDisposalCost(250),
                [st.Code] = MkLaDisposalCost(175),
                [wd.Code] = MkLaDisposalCost(150),
                [ot.Code] = MkLaDisposalCost(400)
            }
        };

        var smcw = new SelfManagedConsumerWaste
        {
            CalculatorRunId = 1,
            ProducerTotals = new List<ProducerSelfManagedConsumerWaste>(),
            TotalByMaterial = new Dictionary<string, SelfManagedConsumerWasteData>
            {
                [al.Code] = mkSmcw(220, 330, 550),
                [fc.Code] = mkSmcw(275, 55, 55),
                [gl.Code] = mkSmcw(110, 220, 220),
                [pc.Code] = mkSmcw(400, 1050, 2400),
                [pl.Code] = mkSmcw(2150, 275, 270),
                [st.Code] = mkSmcw(33, 40, 74),
                [wd.Code] = mkSmcw(265, 0, 0),
                [ot.Code] = mkSmcw(30, 0, 0)
            }
        };

        var redFactor = 1.2m;
        var defaultParameters = new Dictionary<string, string> { ["REDM-RF"] = redFactor.ToString() }.Concat(lateReportingTonnageDict).ToDictionary(k => k.Key, v => v.Value);
        var modulationResults = await builder.ConstructAsync(defaultParameters, TestDataHelper.GetMaterialDetails(), laDisposalCostData, smcw);
        //Console.WriteLine($">> {JsonConvert.SerializeObject(modulationResults, Formatting.Indented)}");

        Assert.AreEqual(1.2m, modulationResults.RedFactor);
        Assert.AreEqual(0.772567m, modulationResults.GreenFactor);

        var expected =
            new Dictionary<MaterialDetail, ModulationDetail>
            {
                [al] = mkModulationDetail(100, 120, 77.2567m, 332, 221, 553, 22100, 55300),
                [fc] = mkModulationDetail(130, 156, 100.4337m, 57, 276, 58, 35880, 7540),
                [gl] = mkModulationDetail(150, 180, 115.8851m, 222, 111, 223, 16650, 33450),
                [pc] = mkModulationDetail(200, 240, 154.5134m, 1052, 401, 2403, 80200, 480600),
                [pl] = mkModulationDetail(250, 300, 193.1418m, 277, 2151, 273, 537750, 68250),
                [st] = mkModulationDetail(175, 210, 135.1992m, 42, 34, 77, 5950, 13475),
                [wd] = mkModulationDetail(150, 180, 115.8851m, 2, 266, 3, 39900, 450),
                [ot] = mkModulationDetail(400, 480, 309.0268m, 2, 31, 3, 12400, 1200)
            };

        CollectionAssert.AreEquivalent(expected.Keys.ToList(), modulationResults.ModulationByMaterial.Keys.ToList());

        foreach (var kvp in expected)
            Assert.AreEqual(kvp.Value, modulationResults.ModulationByMaterial[kvp.Key], $"Value mismatch for key: {kvp.Key}");
    }

    [TestMethod]
    public async Task ModulationBuilder_TestCalculationRounding()
    {
        var laDisposalCostData = new CalcResultLaDisposalCostData
        {
            ByMaterial = new Dictionary<string, CalcResultLaDisposalCostDataDetail>
            {
                [al.Code] = MkLaDisposalCost(0.1508m),
                [fc.Code] = MkLaDisposalCost(0.0045m),
                [gl.Code] = MkLaDisposalCost(0.4961m),
                [pc.Code] = MkLaDisposalCost(0.5788m),
                [pl.Code] = MkLaDisposalCost(0.0057m),
                [st.Code] = MkLaDisposalCost(0.2118m),
                [wd.Code] = MkLaDisposalCost(0.1134m),
                [ot.Code] = MkLaDisposalCost(0.0039m)
            }
        };

        var smcw = new SelfManagedConsumerWaste
        {
            CalculatorRunId = 1,
            ProducerTotals = new List<ProducerSelfManagedConsumerWaste>(),
            TotalByMaterial = new Dictionary<string, SelfManagedConsumerWasteData>
            {
                [al.Code] = mkSmcw(96.000m, 696175.000m, 50.000m),
                [fc.Code] = mkSmcw(101.000m, 3838302.000m, 50.000m),
                [gl.Code] = mkSmcw(138.000m, 9121268.500m, 72.000m),
                [pc.Code] = mkSmcw(121.000m, 39046.000m, 50.000m),
                [pl.Code] = mkSmcw(131.000m, 6376556.120m, 50.000m),
                [st.Code] = mkSmcw(141.000m, 99915.100m, 50.000m),
                [wd.Code] = mkSmcw(151.000m, 155059.900m, 50.000m),
                [ot.Code] = mkSmcw(161.000m, 2645868.000m, 50.000m)
            }
        };

        var redFactor = 1.2m;
        var defaultParameters = new Dictionary<string, string> { ["REDM-RF"] = redFactor.ToString() }.Concat(lateReportingTonnageDict).ToDictionary(k => k.Key, v => v.Value);
        var modulationResults = await builder.ConstructAsync(defaultParameters, TestDataHelper.GetMaterialDetails(), laDisposalCostData, smcw);
        //Console.WriteLine($">> {JsonConvert.SerializeObject(modulationResults, Formatting.Indented)}");

        Assert.AreEqual(1.2m, modulationResults.RedFactor);
        Assert.AreEqual(0.566720m, modulationResults.GreenFactor);

        var expected =
            new Dictionary<MaterialDetail, ModulationDetail>
            {
                [al] = mkModulationDetail(0.1508m, 0.1810m, 0.0855m, 696177.000m, 97.000m, 53.000m, 14.63m, 7.99m),
                [fc] = mkModulationDetail(0.0045m, 0.0054m, 0.0026m, 3838304.000m, 102.000m, 53.000m, 0.46m, 0.24m),
                [gl] = mkModulationDetail(0.4961m, 0.5953m, 0.2811m, 9121270.500m, 139.000m, 75.000m, 68.96m, 37.21m),
                [pc] = mkModulationDetail(0.5788m, 0.6946m, 0.3280m, 39048.000m, 122.000m, 53.000m, 70.61m, 30.68m),
                [pl] = mkModulationDetail(0.0057m, 0.0068m, 0.0032m, 6376558.120m, 132.000m, 53.000m, 0.75m, 0.30m),
                [st] = mkModulationDetail(0.2118m, 0.2542m, 0.1200m, 99917.100m, 142.000m, 53.000m, 30.08m, 11.23m),
                [wd] = mkModulationDetail(0.1134m, 0.1361m, 0.0643m, 155061.900m, 152.000m, 53.000m, 17.24m, 6.01m),
                [ot] = mkModulationDetail(0.0039m, 0.0047m, 0.0022m, 2645870.000m, 162.000m, 53.000m, 0.63m, 0.21m)
            };

        CollectionAssert.AreEquivalent(expected.Keys.ToList(), modulationResults.ModulationByMaterial.Keys.ToList());

        foreach (var kvp in expected)
            Assert.AreEqual(kvp.Value, modulationResults.ModulationByMaterial[kvp.Key], $"Value mismatch for key: {kvp.Key}");
    }

    [TestMethod]
    public async Task ModulationBuilder_Factor1()
    {
        var laDisposalCostData = new CalcResultLaDisposalCostData
        {
            ByMaterial = new Dictionary<string, CalcResultLaDisposalCostDataDetail>
            {
                [al.Code] = MkLaDisposalCost(100),
                [fc.Code] = MkLaDisposalCost(130),
                [gl.Code] = MkLaDisposalCost(150),
                [pc.Code] = MkLaDisposalCost(200),
                [pl.Code] = MkLaDisposalCost(250),
                [st.Code] = MkLaDisposalCost(175),
                [wd.Code] = MkLaDisposalCost(150),
                [ot.Code] = MkLaDisposalCost(400)
            }
        };
        var smcw = new SelfManagedConsumerWaste
        {
            CalculatorRunId = 1,
            ProducerTotals = new List<ProducerSelfManagedConsumerWaste>(),
            TotalByMaterial = new Dictionary<string, SelfManagedConsumerWasteData>
            {
                [al.Code] = mkSmcw(220, 330, 550),
                [fc.Code] = mkSmcw(275, 55, 55),
                [gl.Code] = mkSmcw(110, 220, 220),
                [pc.Code] = mkSmcw(400, 1050, 2400),
                [pl.Code] = mkSmcw(2150, 275, 270),
                [st.Code] = mkSmcw(33, 40, 74),
                [wd.Code] = mkSmcw(265, 0, 0),
                [ot.Code] = mkSmcw(30, 0, 0)
            }
        };

        var redFactor = 1m;
        var defaultParameters = new Dictionary<string, string> { ["REDM-RF"] = redFactor.ToString() }.Concat(lateReportingTonnageDict).ToDictionary(k => k.Key, v => v.Value);
        var modulationResults = await builder.ConstructAsync(defaultParameters, materials, laDisposalCostData, smcw);

        Assert.AreEqual(1m, modulationResults.RedFactor);
        Assert.AreEqual(1m, modulationResults.GreenFactor);
        foreach (var material in materials)
        {
            var cost = laDisposalCostData.ByMaterial[material.Code].DisposalCostPricePerTonne;

            var mm = modulationResults.ModulationByMaterial[material];
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
            ByMaterial = new Dictionary<string, CalcResultLaDisposalCostDataDetail>
            {
                [al.Code] = MkLaDisposalCost(100),
                [fc.Code] = MkLaDisposalCost(130),
                [gl.Code] = MkLaDisposalCost(150),
                [pc.Code] = MkLaDisposalCost(200),
                [pl.Code] = MkLaDisposalCost(250),
                [st.Code] = MkLaDisposalCost(175),
                [wd.Code] = MkLaDisposalCost(150),
                [ot.Code] = MkLaDisposalCost(400)
            }
        };
        var smcw = new SelfManagedConsumerWaste
        {
            CalculatorRunId = 1,
            ProducerTotals = new List<ProducerSelfManagedConsumerWaste>(),
            TotalByMaterial = new Dictionary<string, SelfManagedConsumerWasteData>
            {
                [al.Code] = mkSmcw(220, 330, 0),
                [fc.Code] = mkSmcw(275, 55, 0),
                [gl.Code] = mkSmcw(110, 220, 0),
                [pc.Code] = mkSmcw(400, 1050, 0),
                [pl.Code] = mkSmcw(2150, 275, 0),
                [st.Code] = mkSmcw(33, 40, 0),
                [wd.Code] = mkSmcw(265, 0, 0),
                [ot.Code] = mkSmcw(30, 0, 0)
            }
        };

        var redFactor = 1.2m;
        var lateReportingTonnageDict = materials
            .SelectMany(m => new[]
            {
                ($"LRET-{m.Code}-R", "1"),
                ($"LRET-{m.Code}"  , "2"),
                ($"LRET-{m.Code}-G", "0")
            })
            .ToDictionary(t => t.Item1, t => t.Item2);
        var defaultParameters = new Dictionary<string, string> { ["REDM-RF"] = redFactor.ToString() }.Concat(lateReportingTonnageDict).ToDictionary(k => k.Key, v => v.Value);
        var modulationResults = await builder.ConstructAsync(defaultParameters, materials, laDisposalCostData, smcw);

        Assert.AreEqual(redFactor, modulationResults.RedFactor);
        Assert.AreEqual(1.0m, modulationResults.GreenFactor);
        foreach (var material in materials)
        {
            var cost = laDisposalCostData.ByMaterial[material.Code].DisposalCostPricePerTonne;

            var mm = modulationResults.ModulationByMaterial[material];
            Assert.AreEqual(cost, mm.AmberMaterialDisposalCost);
            Assert.AreEqual(cost * redFactor, mm.RedMaterialDisposalCost);
            Assert.AreEqual(cost, mm.AmberMaterialDisposalCost);
        }
    }
}
