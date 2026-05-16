using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Builder.CommsCost;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.UnitTests.Builder
{
    [TestClass]
    public class CalcResultCommsCostBuilderTest
    {
        private readonly CalcResultCommsCostBuilder builder;
        private readonly ApplicationDBContext dbContext;

        public CalcResultCommsCostBuilderTest()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "PayCal")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            dbContext = new ApplicationDBContext(dbContextOptions);
            dbContext.Database.EnsureCreated();
            builder = new CalcResultCommsCostBuilder(dbContext);
        }

        private Fixture Fixture { get; init; } = new Fixture();

        [TestCleanup]
        public void TearDown()
        {
            dbContext.Database.EnsureDeleted();
        }

        [TestMethod]
        public async Task CalcResultCommsCostBuilder_ConstructTest()
        {
            var calcResult = TestDataHelper.GetCalcResult();

            var materialDetails = CreateMaterials();
            CreateDefaultTemplate();
            CreateDefaultParameters();
            CreateNewRun();
            CreateProducerDetail();
            var resultsRequestDto = new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2024) };
            var apportionment = new CalcResultOnePlusFourApportionment
            {
                OnePlusFourApportionment = new CalcResultOnePlusFourApportionmentDetail
                {
                    EnglandTotal         = 40M,
                    WalesTotal           = 20M,
                    ScotlandTotal        = 20M,
                    NorthernIrelandTotal = 20M,
                    Total                = 100
                },
            };
            var result = await builder.ConstructAsync(materialDetails, resultsRequestDto, apportionment, calcResult.CalcResultLateReportingTonnageData);

            Assert.IsNotNull(result);

            var onePlusFourApp = result.CalcResultCommsCostOnePlusFourApportionment;
            Assert.IsNotNull(onePlusFourApp);

            Assert.AreEqual("1 + 4 Apportionment %s", onePlusFourApp.Name);
            Assert.AreEqual(40 , onePlusFourApp.England);
            Assert.AreEqual(20 , onePlusFourApp.Wales);
            Assert.AreEqual(20 , onePlusFourApp.NorthernIreland);
            Assert.AreEqual(20 , onePlusFourApp.Scotland);
            Assert.AreEqual(100, onePlusFourApp.Total);

            var materialCosts = result.CalcResultCommsCostCommsCostByMaterial.ToList();
            Assert.IsNotNull(materialCosts);
            Assert.AreEqual(9, materialCosts.Count);

            var aluminiumCost = materialCosts[0];
            Assert.AreEqual("Aluminium", aluminiumCost.Name);
            Assert.AreEqual(4, aluminiumCost.England);
            Assert.AreEqual(2, aluminiumCost.Wales);
            Assert.AreEqual(2, aluminiumCost.Scotland);
            Assert.AreEqual(2, aluminiumCost.NorthernIreland);
            Assert.AreEqual(10, aluminiumCost.Total);
            Assert.AreEqual(1000, aluminiumCost.ProducerReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual(8000, aluminiumCost.LateReportingTonnage);
            Assert.AreEqual(9000, aluminiumCost.ProducerReportedTotalTonnage);
            // TODO this rounding should happen in the builder
            Assert.AreEqual(0.0011m, Math.Round(aluminiumCost.CommsCostByMaterialPricePerTonne!.Value, 4));

            var fibreCompositeCost = materialCosts[1];
            Assert.AreEqual("Fibre composite", fibreCompositeCost.Name);
            Assert.AreEqual(4, fibreCompositeCost.England);
            Assert.AreEqual(2, fibreCompositeCost.Wales);
            Assert.AreEqual(2, fibreCompositeCost.Scotland);
            Assert.AreEqual(2, fibreCompositeCost.NorthernIreland);
            Assert.AreEqual(10, fibreCompositeCost.Total);
            Assert.AreEqual(2000, fibreCompositeCost.ProducerReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual(10, fibreCompositeCost.LateReportingTonnage);
            Assert.AreEqual(2210, fibreCompositeCost.ProducerReportedTotalTonnage);
            Assert.AreEqual(0.0045m, Math.Round(fibreCompositeCost.CommsCostByMaterialPricePerTonne!.Value, 4));

            var glassCost = materialCosts[2];
            Assert.AreEqual("Glass", glassCost.Name);
            Assert.AreEqual(4, glassCost.England);
            Assert.AreEqual(2, glassCost.Wales);
            Assert.AreEqual(2, glassCost.Scotland);
            Assert.AreEqual(2, glassCost.NorthernIreland);
            Assert.AreEqual(10, glassCost.Total);
            Assert.AreEqual(3000, glassCost.ProducerReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual(10, glassCost.LateReportingTonnage);
            Assert.AreEqual(3210, glassCost.ProducerReportedTotalTonnage);
            Assert.AreEqual(0.0031m, Math.Round(glassCost.CommsCostByMaterialPricePerTonne!.Value, 4));
            Assert.AreEqual(200, glassCost.HouseholdDrinksContainers);

            var totalMaterialCost = materialCosts.Last();
            Assert.AreEqual("Total", totalMaterialCost.Name);
            Assert.AreEqual(32, totalMaterialCost.England);
            Assert.AreEqual(16, totalMaterialCost.Wales);
            Assert.AreEqual(16, totalMaterialCost.Scotland);
            Assert.AreEqual(16, totalMaterialCost.NorthernIreland);
            Assert.AreEqual(80, totalMaterialCost.Total);
            Assert.AreEqual(36000, totalMaterialCost.ProducerReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual(10020, totalMaterialCost.LateReportingTonnage);
            Assert.AreEqual(46420, totalMaterialCost.ProducerReportedTotalTonnage);
            Assert.IsNull(totalMaterialCost.CommsCostByMaterialPricePerTonne);
        }

        [TestMethod]
        public async Task GetProducerReportedMaterials_ShouldReturnValidMaterials()
        {
            // Arrange
            SeedDatabase(dbContext);
            var runId = 1;

            // Act
            var result = await builder.GetProducerReportedMaterials(dbContext, runId);

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

            var producerReportedMaterials = new List<ProducerReportedMaterialProjected>
            {
                new ProducerReportedMaterialProjected { ProducerDetailId = 1, MaterialId = 1, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.Household, PackagingTonnage = 50 },
                new ProducerReportedMaterialProjected { ProducerDetailId = 1, MaterialId = 1, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.Household, PackagingTonnage = 50 },
                new ProducerReportedMaterialProjected { ProducerDetailId = 1, MaterialId = 2, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 100 },
                new ProducerReportedMaterialProjected { ProducerDetailId = 1, MaterialId = 2, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 100 },
                new ProducerReportedMaterialProjected { ProducerDetailId = 1, MaterialId = 3, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 150 },
                new ProducerReportedMaterialProjected { ProducerDetailId = 1, MaterialId = 3, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 150 },
            };
            context.ProducerReportedMaterialProjected.AddRange(producerReportedMaterials);

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
                "Jumbo Box Store",
            };

            var producerId = 1;
            foreach (var producerName in producerNames)
            {
                dbContext.ProducerDetail.Add(new ProducerDetail
                {
                    ProducerId = producerId++,
                    SubsidiaryId = $"{producerId}-Sub",
                    ProducerName = producerName,
                    CalculatorRunId = 1,
                });
            }

            dbContext.SaveChanges();

            foreach (var subPeriod in new[] { "2025-H1", "2025-H2"}) {
                for (int producerDetailId = 1; producerDetailId <= 10; producerDetailId++)
                {
                    for (int materialId = 1; materialId < 9; materialId++)
                    {
                        this.dbContext.ProducerReportedMaterialProjected.Add(new ProducerReportedMaterialProjected
                        {
                            MaterialId = materialId,
                            ProducerDetailId = producerDetailId,
                            PackagingType = "HH",
                            SubmissionPeriod = subPeriod,
                            PackagingTonnage = materialId * 50,
                        });
                    }
                }

                this.dbContext.ProducerReportedMaterialProjected.AddRange(new List<ProducerReportedMaterialProjected> {
                    new ProducerReportedMaterialProjected()
                        {
                            MaterialId = 3,
                            ProducerDetailId = 1,
                            PackagingType = "HDC",
                            SubmissionPeriod = subPeriod,
                            PackagingTonnage = 50,
                        },
                        new ProducerReportedMaterialProjected()
                        {
                            MaterialId = 3,
                            ProducerDetailId = 2,
                            PackagingType = "HDC",
                            SubmissionPeriod = subPeriod,
                            PackagingTonnage = 50,
                        },
                        new ProducerReportedMaterialProjected()
                        {
                            MaterialId = 2,
                            ProducerDetailId = 1,
                            PackagingType = "PB",
                            SubmissionPeriod = subPeriod,
                            PackagingTonnage = 100,
                        }
                });
            }

            dbContext.SaveChanges();
        }

        private void CreateDefaultTemplate()
        {
            dbContext.DefaultParameterTemplateMasterList.RemoveRange(
                dbContext.DefaultParameterTemplateMasterList.ToList());
            dbContext.SaveChanges();

            var materialDictionary = new Dictionary<string, string>
            {
                { "AL", "Aluminium" },
                { "FC", "Fibre composite" },
                { "GL", "Glass" },
                { "PC", "Paper or card" },
                { "PL", "Plastic" },
                { "ST", "Steel" },
                { "WD", "Wood" },
                { "OT", "Other materials" },
            };

            var parameterTypes = new[] { "Communication costs by material", "Late reporting tonnage" };
            foreach (var material in materialDictionary.Values)
            {
                dbContext.DefaultParameterTemplateMasterList.Add(new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = Guid.NewGuid().ToString(),
                    ParameterCategory = material,
                    ParameterType = parameterTypes[0]
                });
                var rag = new[] { "R", "A", "G" };
                foreach (var v in rag)
                {
                    dbContext.DefaultParameterTemplateMasterList.Add(new DefaultParameterTemplateMaster
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
                dbContext.DefaultParameterTemplateMasterList.Add(new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = Guid.NewGuid().ToString(),
                    ParameterCategory = country,
                    ParameterType = "Communication costs by country"
                });
            }

            dbContext.SaveChanges();
        }

        private void CreateNewRun()
        {
            var run = new CalculatorRun
            {
                CalculatorRunClassificationId = (int)RunClassification.RUNNING,
                Name = "Test Run",
                RelativeYear = new RelativeYear(2024),
                CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                CreatedBy = "Test User",
                DefaultParameterSettingMasterId = 1
            };
            dbContext.CalculatorRuns.Add(run);
            dbContext.SaveChanges();
        }

        private void CreateDefaultParameters()
        {
            var templateMasterList = dbContext.DefaultParameterTemplateMasterList.ToList();

            var defaultMaster = new DefaultParameterSettingMaster
            {
                RelativeYear = new RelativeYear(2024),
            };

            dbContext.DefaultParameterSettings.Add(defaultMaster);
            dbContext.SaveChanges();

            foreach (var templateMaster in templateMasterList)
            {
                var defaultDetail = new DefaultParameterSettingDetail
                {
                    ParameterUniqueReferenceId = templateMaster.ParameterUniqueReferenceId,
                    ParameterValue = GetValue(templateMaster),
                    DefaultParameterSettingMasterId = 1,
                    DefaultParameterSettingMaster = defaultMaster
                };
                dbContext.DefaultParameterSettingDetail.Add(defaultDetail);
            }

            dbContext.SaveChanges();
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

        private IImmutableList<MaterialDetail> CreateMaterials()
        {
            var materials = TestDataHelper.GetMaterials();

            dbContext.Material.AddRange(materials.Select(m =>
                new Material
                {
                    Name        = m.Name,
                    Code        = m.Code,
                    Description = m.Description
                }
            ));

            dbContext.SaveChanges();

            return materials;
        }
    }
}
