using AutoFixture;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Builder.CommsCost;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

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
            builder = new CalcResultCommsCostBuilder(
                dbContext,
                new TelemetryClient(TelemetryConfiguration.CreateDefault()));
        }

        private Fixture Fixture { get; init; } = new Fixture();

        [TestCleanup]
        public void TearDown()
        {
            dbContext.Database.EnsureDeleted();
        }

        [TestMethod]
        public async Task ConstructTest()
        {
            var calcResult = TestDataHelper.GetCalcResult();

            CreateMaterials();
            CreateDefaultTemplate();
            CreateDefaultParameters();
            CreateNewRun();
            CreateProducerDetail();
            var resultsRequestDto = new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2024) };
            var apportionment = new CalcResultOnePlusFourApportionment
            {
                Name = Fixture.Create<string>(),
                CalcResultOnePlusFourApportionmentDetails = new List<CalcResultOnePlusFourApportionmentDetail>
                {
                    new CalcResultOnePlusFourApportionmentDetail
                    {
                        Name = Fixture.Create<string>(),
                        EnglandTotal = 40M,
                        ScotlandTotal = 20M,
                        WalesTotal = 20M,
                        NorthernIrelandTotal = 20M,
                        Total = "100%",
                        EnglandDisposalTotal = "40%",
                        ScotlandDisposalTotal = "20%",
                        WalesDisposalTotal = "20%",
                        NorthernIrelandDisposalTotal = "20%",
                    },
                },
            };
            var result = await builder.ConstructAsync(resultsRequestDto, apportionment, calcResult.CalcResultLateReportingTonnageData);

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
            Assert.AreEqual("1000.000", aluminiumCost.ProducerReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual("8000.000", aluminiumCost.LateReportingTonnage);
            Assert.AreEqual("9000.000", aluminiumCost.ProducerReportedHouseholdPlusLateReportingTonnage);
            Assert.AreEqual("0.0011", aluminiumCost.CommsCostByMaterialPricePerTonne);

            var fibreCompositeCost = materialCosts[2];
            Assert.AreEqual("Fibre composite", fibreCompositeCost.Name);
            Assert.AreEqual("£4.00", fibreCompositeCost.England);
            Assert.AreEqual("£2.00", fibreCompositeCost.Wales);
            Assert.AreEqual("£2.00", fibreCompositeCost.Scotland);
            Assert.AreEqual("£2.00", fibreCompositeCost.NorthernIreland);
            Assert.AreEqual("£10.00", fibreCompositeCost.Total);
            Assert.AreEqual("2000.000", fibreCompositeCost.ProducerReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual("10.000", fibreCompositeCost.LateReportingTonnage);
            Assert.AreEqual("2210.000", fibreCompositeCost.ProducerReportedHouseholdPlusLateReportingTonnage);
            Assert.AreEqual("0.0045", fibreCompositeCost.CommsCostByMaterialPricePerTonne);

            var glassCost = materialCosts[3];
            Assert.AreEqual("Glass", glassCost.Name);
            Assert.AreEqual("£4.00", glassCost.England);
            Assert.AreEqual("£2.00", glassCost.Wales);
            Assert.AreEqual("£2.00", glassCost.Scotland);
            Assert.AreEqual("£2.00", glassCost.NorthernIreland);
            Assert.AreEqual("£10.00", glassCost.Total);
            Assert.AreEqual("3000.000", glassCost.ProducerReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual("10.000", glassCost.LateReportingTonnage);
            Assert.AreEqual("3210.000", glassCost.ProducerReportedHouseholdPlusLateReportingTonnage);
            Assert.AreEqual("0.0031", glassCost.CommsCostByMaterialPricePerTonne);
            Assert.AreEqual("200.0000", glassCost.HouseholdDrinksContainers);

            var totalMaterialCost = materialCosts.Last();
            Assert.AreEqual("Total", totalMaterialCost.Name);
            Assert.AreEqual("£32.00", totalMaterialCost.England);
            Assert.AreEqual("£16.00", totalMaterialCost.Wales);
            Assert.AreEqual("£16.00", totalMaterialCost.Scotland);
            Assert.AreEqual("£16.00", totalMaterialCost.NorthernIreland);
            Assert.AreEqual("£80.00", totalMaterialCost.Total);
            Assert.AreEqual("36000.000", totalMaterialCost.ProducerReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual("10020.000", totalMaterialCost.LateReportingTonnage);
            Assert.AreEqual("46420.000", totalMaterialCost.ProducerReportedHouseholdPlusLateReportingTonnage);
            Assert.IsTrue(string.IsNullOrEmpty(totalMaterialCost.CommsCostByMaterialPricePerTonne));
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
                dbContext.Material.Add(new Material
                {
                    Name = materialKv.Value,
                    Code = materialKv.Key,
                    Description = "Some"
                });
            }

            dbContext.SaveChanges();
        }
    }
}
