using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Builder.CommsCost;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Features.Calculator.Contexts;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.Service.Function.UnitTests.Builder;

[TestClass]
public class CalcResultCommsCostBuilderTest
{
    private ApplicationDBContext _dbContext = null!;
    private CalcResultCommsCostBuilder _sut = null!;
    private IFixture _fixture = null!;

    [TestInitialize]
    public void Init()
    {
        _fixture = TestFixtures.New();
        _dbContext = _fixture.Freeze<ApplicationDBContext>();

        _sut = _fixture.Create<CalcResultCommsCostBuilder>();
    }

    [TestMethod]
    public async Task ConstructTest()
    {
        var calcResult = _fixture.Create<CalcResult>();
        calcResult.CalcResultScaledupProducers = GetScaledUpProducers();

        CreateMaterials();
        CreateDefaultTemplate();
        CreateDefaultParameters();
        CreateNewRun();
        CreateProducerDetail();
        var runContext = TestFixtures.Default.Create<CalculatorRunContext>();
        var apportionment = new CalcResultOnePlusFourApportionment
        {
            Name = TestFixtures.Default.Create<string>(),
            CalcResultOnePlusFourApportionmentDetails =
            [
                new CalcResultOnePlusFourApportionmentDetail
                {
                    Name = TestFixtures.Default.Create<string>(),
                    EnglandTotal = 40M,
                    ScotlandTotal = 20M,
                    WalesTotal = 20M,
                    NorthernIrelandTotal = 20M,
                    Total = "100%",
                    EnglandDisposalTotal = "40%",
                    ScotlandDisposalTotal = "20%",
                    WalesDisposalTotal = "20%",
                    NorthernIrelandDisposalTotal = "20%"
                }
            ]
        };
        var result = await _sut.ConstructAsync(runContext, apportionment, calcResult);

        Assert.IsNotNull(result);

        Assert.AreEqual("Parameters - Comms Costs", result.Name);

        var onePlusFourApp = result.CalcResultCommsCostOnePlusFourApportionment;
        Assert.IsNotNull(onePlusFourApp);
        Assert.AreEqual(2, onePlusFourApp.Count());
        var headerApp = onePlusFourApp.First();
        Assert.IsTrue(string.IsNullOrEmpty(headerApp.Name));

        Assert.AreEqual("England", headerApp.England);
        Assert.AreEqual("Wales", headerApp.Wales);
        Assert.AreEqual("Northern Ireland", headerApp.NorthernIreland);
        Assert.AreEqual("Scotland", headerApp.Scotland);

        Assert.AreEqual("Total", headerApp.Total);

        var dataApp = result.CalcResultCommsCostOnePlusFourApportionment.Last();
        Assert.IsNotNull(dataApp);

        Assert.AreEqual("1 + 4 Apportionment %s", dataApp.Name);
        Assert.AreEqual("40%", dataApp.England);
        Assert.AreEqual("20%", dataApp.Wales);
        Assert.AreEqual("20%", dataApp.NorthernIreland);
        Assert.AreEqual("20%", dataApp.Scotland);
        Assert.AreEqual("100%", dataApp.Total);

        var materialCosts = result.CalcResultCommsCostCommsCostByMaterial.ToList();
        Assert.IsNotNull(materialCosts);
        Assert.AreEqual(10, materialCosts.Count);

        var materialHeader = materialCosts.First();

        Assert.IsNotNull(materialHeader);

        Assert.AreEqual("2a Comms Costs - by Material", materialHeader.Name);
        Assert.AreEqual("England", materialHeader.England);
        Assert.AreEqual("Wales", materialHeader.Wales);
        Assert.AreEqual("Scotland", materialHeader.Scotland);
        Assert.AreEqual("Northern Ireland", materialHeader.NorthernIreland);
        Assert.AreEqual("Total", materialHeader.Total);
        Assert.AreEqual(
            "Producer Household Packaging Tonnage",
            materialHeader.ProducerReportedHouseholdPackagingWasteTonnage);
        Assert.AreEqual("Public Bin Tonnage", materialHeader.ReportedPublicBinTonnage);
        Assert.AreEqual("Household Drinks Containers Tonnage", materialHeader.HouseholdDrinksContainers);
        Assert.AreEqual("Late Reporting Tonnage", materialHeader.LateReportingTonnage);
        Assert.AreEqual(
            "Producer Household Tonnage + Late Reporting Tonnage + Public Bin Tonnage + Household Drinks Containers Tonnage",
            materialHeader.ProducerReportedHouseholdPlusLateReportingTonnage);
        Assert.AreEqual(
            "Comms Cost - by Material Price Per Tonne",
            materialHeader.CommsCostByMaterialPricePerTonne);

        var aluminiumCost = materialCosts[1];
        Assert.AreEqual("Aluminium", aluminiumCost.Name);
        Assert.AreEqual("£4.00", aluminiumCost.England);
        Assert.AreEqual("£2.00", aluminiumCost.Wales);
        Assert.AreEqual("£2.00", aluminiumCost.Scotland);
        Assert.AreEqual("£2.00", aluminiumCost.NorthernIreland);
        Assert.AreEqual("£10.00", aluminiumCost.Total);
        Assert.AreEqual(
            "910.000",
            aluminiumCost.ProducerReportedHouseholdPackagingWasteTonnage);
        Assert.AreEqual("8000.000", aluminiumCost.LateReportingTonnage);
        Assert.AreEqual(
            "8920.000",
            aluminiumCost.ProducerReportedHouseholdPlusLateReportingTonnage);
        Assert.AreEqual(
            "0.0011",
            aluminiumCost.CommsCostByMaterialPricePerTonne);

        var fibreCompositeCost = materialCosts[2];
        Assert.AreEqual("Fibre composite", fibreCompositeCost.Name);
        Assert.AreEqual("£4.00", fibreCompositeCost.England);
        Assert.AreEqual("£2.00", fibreCompositeCost.Wales);
        Assert.AreEqual("£2.00", fibreCompositeCost.Scotland);
        Assert.AreEqual("£2.00", fibreCompositeCost.NorthernIreland);
        Assert.AreEqual("£10.00", fibreCompositeCost.Total);
        Assert.AreEqual(
            "1800.000",
            fibreCompositeCost.ProducerReportedHouseholdPackagingWasteTonnage);
        Assert.AreEqual("10.000", fibreCompositeCost.LateReportingTonnage);
        Assert.AreEqual(
            "1810.000",
            fibreCompositeCost.ProducerReportedHouseholdPlusLateReportingTonnage);
        Assert.AreEqual(
            "0.0055",
            fibreCompositeCost.CommsCostByMaterialPricePerTonne);

        var glassCost = materialCosts[3];
        Assert.AreEqual("Glass", glassCost.Name);
        Assert.AreEqual("£4.00", glassCost.England);
        Assert.AreEqual("£2.00", glassCost.Wales);
        Assert.AreEqual("£2.00", glassCost.Scotland);
        Assert.AreEqual("£2.00", glassCost.NorthernIreland);
        Assert.AreEqual("£10.00", glassCost.Total);
        Assert.AreEqual(
            "2700.000",
            glassCost.ProducerReportedHouseholdPackagingWasteTonnage);
        Assert.AreEqual("10.000", glassCost.LateReportingTonnage);
        Assert.AreEqual(
            "2810.000",
            glassCost.ProducerReportedHouseholdPlusLateReportingTonnage);
        Assert.AreEqual(
            "0.0036",
            glassCost.CommsCostByMaterialPricePerTonne);
        Assert.AreEqual("100.0000", glassCost.HouseholdDrinksContainers);

        var totalMaterialCost = materialCosts.Last();
        Assert.AreEqual("Total", totalMaterialCost.Name);
        Assert.AreEqual("£32.00", totalMaterialCost.England);
        Assert.AreEqual("£16.00", totalMaterialCost.Wales);
        Assert.AreEqual("£16.00", totalMaterialCost.Scotland);
        Assert.AreEqual("£16.00", totalMaterialCost.NorthernIreland);
        Assert.AreEqual("£80.00", totalMaterialCost.Total);
        Assert.AreEqual(
            "32410.000",
            totalMaterialCost.ProducerReportedHouseholdPackagingWasteTonnage);
        Assert.AreEqual("10020.000", totalMaterialCost.LateReportingTonnage);
        Assert.AreEqual(
            "42540.000",
            totalMaterialCost.ProducerReportedHouseholdPlusLateReportingTonnage);
        Assert.IsTrue(string.IsNullOrEmpty(totalMaterialCost.CommsCostByMaterialPricePerTonne));
    }

    [TestMethod]
    public async Task GetProducerReportedMaterials_ShouldReturnValidMaterials()
    {
        // Arrange
        SeedDatabase(_dbContext);

        var runId = 1;

        // Act
        var result = await _sut.GetProducerReportedMaterials(runId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(6, result.Count);
        Assert.IsTrue(result.Any(r => r.Material!.Code == "PL" && r.PackagingType == "HH" && r.PackagingTonnage == 50 && r.SubmissionPeriod == "2025-H1"));
        Assert.IsTrue(result.Any(r => r.Material!.Code == "PL" && r.PackagingType == "HH" && r.PackagingTonnage == 50 && r.SubmissionPeriod == "2025-H2"));
        Assert.IsTrue(result.Any(r => r.Material!.Code == "ST" && r.PackagingType == "PB" && r.PackagingTonnage == 100 && r.SubmissionPeriod == "2025-H1"));
        Assert.IsTrue(result.Any(r => r.Material!.Code == "ST" && r.PackagingType == "PB" && r.PackagingTonnage == 100 && r.SubmissionPeriod == "2025-H2"));
        Assert.IsTrue(result.Any(r => r.Material!.Code == "GL" && r.PackagingType == "HDC" && r.PackagingTonnage == 150 && r.SubmissionPeriod == "2025-H1"));
        Assert.IsTrue(result.Any(r => r.Material!.Code == "GL" && r.PackagingType == "HDC" && r.PackagingTonnage == 150 && r.SubmissionPeriod == "2025-H2"));
    }

        private void SeedDatabase(ApplicationDBContext context)
        {
            var run = new CalculatorRun { Id = 1, RelativeYear = new RelativeYear(2024), Name = "CalculatorRunTest1" };
            context.CalculatorRuns.Add(run);

            var producerDetail = new ProducerDetail { Id = 1, CalculatorRunId = 1 };
            context.ProducerDetail.Add(producerDetail);

            var materials = new List<Material>
        {
            new Material { Id = 1, Name = "Plastic", Code = MaterialCodes.Plastic },
            new Material { Id = 2, Name = "Steel", Code = MaterialCodes.Steel },
            new Material { Id = 3, Name = "Glass", Code = MaterialCodes.Glass },
        };
            context.Material.AddRange(materials);

            var producerReportedMaterials = new List<ProducerReportedMaterial>
        {
            new ProducerReportedMaterial { ProducerDetailId = 1, MaterialId = 1, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.Household, PackagingTonnage = 50 },
            new ProducerReportedMaterial { ProducerDetailId = 1, MaterialId = 1, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.Household, PackagingTonnage = 50 },
            new ProducerReportedMaterial { ProducerDetailId = 1, MaterialId = 2, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 100 },
            new ProducerReportedMaterial { ProducerDetailId = 1, MaterialId = 2, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 100 },
            new ProducerReportedMaterial { ProducerDetailId = 1, MaterialId = 3, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 150 },
            new ProducerReportedMaterial { ProducerDetailId = 1, MaterialId = 3, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 150 },
        };
            context.ProducerReportedMaterial.AddRange(producerReportedMaterials);

            context.SaveChanges();
        }

    private void CreateProducerDetail()
    {
        var producerNames = new[]
        {
            "Allied Packaging",
            "Beeline Materials",
            "Cloud Boxes",
            "Decking and Shed",
            "Electric Things",
            "French Flooring",
            "Good Fruit Co",
            "Happy Shopper",
            "Icicle Foods",
            "Jumbo Box Store"
        };

        var producerId = 1;
        foreach (var producerName in producerNames)
        {
            _dbContext.ProducerDetail.Add(new ProducerDetail
            {
                ProducerId = producerId++,
                SubsidiaryId = $"{producerId}-Sub",
                ProducerName = producerName,
                CalculatorRunId = 1
            });
        }

        _dbContext.SaveChanges();

        foreach (var subPeriod in new[] { "2025-H1", "2025-H2" })
        {
            for (var producerDetailId = 1; producerDetailId <= 10; producerDetailId++)
            {
                for (var materialId = 1; materialId < 9; materialId++)
                {
                    _dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                    {
                        MaterialId = materialId,
                        ProducerDetailId = producerDetailId,
                        PackagingType = "HH",
                        SubmissionPeriod = subPeriod,
                        PackagingTonnage = materialId * 50
                    });
                }
            }

            _dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
            {
                MaterialId = 3,
                ProducerDetailId = 1,
                PackagingType = "HDC",
                SubmissionPeriod = subPeriod,
                PackagingTonnage = 50
            });

            _dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
            {
                MaterialId = 3,
                ProducerDetailId = 2,
                PackagingType = "HDC",
                SubmissionPeriod = subPeriod,
                PackagingTonnage = 50
            });

            _dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
            {
                MaterialId = 2,
                ProducerDetailId = 1,
                PackagingType = "PB",
                SubmissionPeriod = subPeriod,
                PackagingTonnage = 100
            });
        }

        _dbContext.SaveChanges();
    }

    private void CreateDefaultTemplate()
    {
        _dbContext.DefaultParameterTemplateMasterList.RemoveRange(
            _dbContext.DefaultParameterTemplateMasterList.ToList());
        _dbContext.SaveChanges();

        var materialDictionary = new Dictionary<string, string>
        {
            { "AL", "Aluminium" },
            { "FC", "Fibre composite" },
            { "GL", "Glass" },
            { "PC", "Paper or card" },
            { "PL", "Plastic" },
            { "ST", "Steel" },
            { "WD", "Wood" },
            { "OT", "Other materials" }
        };

        var parameterTypes = new[] { "Communication costs by material", "Late reporting tonnage" };
        foreach (var material in materialDictionary.Values)
        {
            _dbContext.DefaultParameterTemplateMasterList.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = Guid.NewGuid().ToString(),
                ParameterCategory = material,
                ParameterType = parameterTypes[0]
            });
            var rag = new[] { "R", "A", "G" };
            foreach (var v in rag)
            {
                _dbContext.DefaultParameterTemplateMasterList.Add(new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = Guid.NewGuid().ToString(),
                    ParameterCategory = $"{material}-{v}",
                    ParameterType = parameterTypes[1]
                });
            }
        }

        var countries = new[]
        {
            "England",
            "Northern Ireland",
            "Scotland",
            "United Kingdom",
            "Wales"
        };

        foreach (var country in countries)
        {
            _dbContext.DefaultParameterTemplateMasterList.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = Guid.NewGuid().ToString(),
                ParameterCategory = country,
                ParameterType = "Communication costs by country"
            });
        }

        _dbContext.SaveChanges();
    }

    private void CreateNewRun()
    {
        var run = new CalculatorRun
        {
            CalculatorRunClassificationId = RunClassificationStatusIds.RUNNINGID,
            Name = "Test Run",
            RelativeYear = new RelativeYear(2024),
            CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
            CreatedBy = "Test User",
            DefaultParameterSettingMasterId = 1
        };
        _dbContext.CalculatorRuns.Add(run);
        _dbContext.SaveChanges();
    }

    private void CreateDefaultParameters()
    {
        var templateMasterList = _dbContext.DefaultParameterTemplateMasterList.ToList();

        var defaultMaster = new DefaultParameterSettingMaster
        {
            RelativeYear = new RelativeYear(2024)
        };

        _dbContext.DefaultParameterSettings.Add(defaultMaster);
        _dbContext.SaveChanges();

        foreach (var templateMaster in templateMasterList)
        {
            var defaultDetail = new DefaultParameterSettingDetail
            {
                ParameterUniqueReferenceId = templateMaster.ParameterUniqueReferenceId,
                ParameterValue = GetValue(templateMaster),
                DefaultParameterSettingMasterId = 1,
                DefaultParameterSettingMaster = defaultMaster
            };
            _dbContext.DefaultParameterSettingDetail.Add(defaultDetail);
        }

        _dbContext.SaveChanges();
    }

    private static decimal GetValue(DefaultParameterTemplateMaster templateMaster)
    {
        if (templateMaster.ParameterType == "Communication costs by material")
        {
            switch (templateMaster.ParameterCategory)
            {
                case "England":
                    return 40M;
                case "Northern Ireland":
                    return 10M;
                case "Scotland":
                    return 20M;
                case "Wales":
                    return 30M;
            }
        }

        return 10;
    }

    private static CalcResultScaledupProducers GetScaledUpProducers()
    {
        return new CalcResultScaledupProducers
        {
            ScaledupProducers =
            [
                new CalcResultScaledupProducer
                {
                    ProducerId = 1,
                    IsTotalRow = true,
                    ScaledupProducerTonnageByMaterial = new Dictionary<string, CalcResultScaledupProducerTonnage>
                    {
                        ["Aluminium"] = new()
                        {
                            ReportedHouseholdPackagingWasteTonnage = 10,
                            ReportedPublicBinTonnage = 10,
                            TotalReportedTonnage = 10,
                            ReportedSelfManagedConsumerWasteTonnage = 10,
                            NetReportedTonnage = 10,
                            ScaledupReportedHouseholdPackagingWasteTonnage = 10,
                            ScaledupReportedPublicBinTonnage = 10,
                            ScaledupTotalReportedTonnage = 10,
                            ScaledupReportedSelfManagedConsumerWasteTonnage = 10,
                            ScaledupNetReportedTonnage = 10
                        }
                    }
                },

                new CalcResultScaledupProducer
                {
                    ProducerId = 1,
                    IsTotalRow = true,
                    ScaledupProducerTonnageByMaterial = new Dictionary<string, CalcResultScaledupProducerTonnage>
                    {
                        ["GL"] = new()
                        {
                            ReportedHouseholdPackagingWasteTonnage = 10,
                            ReportedPublicBinTonnage = 10,
                            TotalReportedTonnage = 10,
                            ReportedSelfManagedConsumerWasteTonnage = 10,
                            NetReportedTonnage = 10,
                            ScaledupReportedHouseholdPackagingWasteTonnage = 10,
                            ScaledupReportedPublicBinTonnage = 10,
                            ScaledupTotalReportedTonnage = 10,
                            ScaledupReportedSelfManagedConsumerWasteTonnage = 10,
                            ScaledupNetReportedTonnage = 10
                        }
                    }
                }
            ]
        };
    }

    private void CreateMaterials()
    {
        var materialDictionary = new Dictionary<string, string>();
        materialDictionary.Add("AL", "Aluminium");
        materialDictionary.Add("FC", "Fibre composite");
        materialDictionary.Add("GL", "Glass");
        materialDictionary.Add("PC", "Paper or card");
        materialDictionary.Add("PL", "Plastic");
        materialDictionary.Add("ST", "Steel");
        materialDictionary.Add("WD", "Wood");
        materialDictionary.Add("OT", "Other materials");

        foreach (var materialKv in materialDictionary)
        {
            _dbContext.Material.Add(new Material
            {
                Name = materialKv.Value,
                Code = materialKv.Key,
                Description = "Some"
            });
        }

        _dbContext.SaveChanges();
    }
}