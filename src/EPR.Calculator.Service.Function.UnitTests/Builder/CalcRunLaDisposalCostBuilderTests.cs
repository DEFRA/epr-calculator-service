using System.Globalization;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Builder.LaDisposalCost;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.UnitTests.Builder
{
    [TestClass]
    public class CalcRunLaDisposalCostBuilderTests
    {
        private readonly CalcRunLaDisposalCostBuilder builder;
        private readonly ApplicationDBContext dbContext;
        private readonly SelfManagedConsumerWaste smcw;

        public class ProducerData
        {
            public int ProducerId { get; set; }

            public required string MaterialName { get; set; }

            public required string PackagingType { get; set; }

            public decimal Tonnage { get; set; }

            public ProducerDetail? ProducerDetail { get; set; }
        }

        public CalcRunLaDisposalCostBuilderTests()
        {
            Fixture = new Fixture();
            var dbContextOptions =
                new DbContextOptionsBuilder<ApplicationDBContext>()
                    .UseInMemoryDatabase(databaseName: "PayCal")
                    .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                    .Options;

            dbContext = new ApplicationDBContext(dbContextOptions);
            dbContext.Database.EnsureCreated();
            builder = new CalcRunLaDisposalCostBuilder(dbContext);

            smcw = new SelfManagedConsumerWaste
            {
                ProducerTotals = [],
                OverallTotalPerMaterials = []
            };
        }

        private Fixture Fixture { get; init; }

        [TestCleanup]
        public void TearDown()
        {
            dbContext.Database.EnsureDeleted();
            dbContext.Dispose();
        }

        [TestMethod]
        public void Should_Set_And_Get_MaterialName()
        {
            // Arrange
            var producerData = new ProducerData { MaterialName = "Plastic", PackagingType = "PB" };

            // Act
            var result = producerData.MaterialName;

            // Assert
            Assert.AreEqual("Plastic", result);
        }

        [TestMethod]
        public void Should_Set_And_Get_PackagingType()
        {
            // Arrange
            var producerData = new ProducerData { PackagingType = "HDC", MaterialName = "Glass" };

            // Act
            var result = producerData.PackagingType;

            // Assert
            Assert.AreEqual("HDC", result);
        }

        [TestMethod]
        public void Should_Set_And_Get_Tonnage()
        {
            // Arrange
            var producerData = new ProducerData { Tonnage = 100.5m, MaterialName = "Plastic", PackagingType = "PB" };

            // Act
            var result = producerData.Tonnage;

            // Assert
            Assert.AreEqual(100.5m, result);
        }

        [TestMethod]
        public void Should_Set_And_Get_ProducerDetail()
        {
            // Arrange
            var producerDetail = new ProducerDetail { ProducerId = 1, ProducerName = "Test Producer" };
            var producerData = new ProducerData { MaterialName = "Plastic", PackagingType = "PB", ProducerDetail = producerDetail };

            // Act
            var result = producerData.ProducerDetail;

            // Assert
            Assert.AreEqual(producerDetail, result);
        }

        [TestMethod]
        public async Task Should_Return_LA_Disposal_Costs()
        {
            // Assign
            var resultsDto = new CalcResultsRequestDto { RunId = 2, RelativeYear = new RelativeYear(2025) };
            var calcResult = TestDataHelper.GetCalcResult();

            // Act
            var lapcapDisposalCostResults = await builder.ConstructAsync(resultsDto, TestDataHelper.GetMaterials(), calcResult.CalcResultLapcapData, calcResult.CalcResultLateReportingTonnageData, smcw, calcResult.ApplyModulation);

            // Assert
            Assert.IsNotNull(lapcapDisposalCostResults);
            Assert.AreEqual(8, lapcapDisposalCostResults.ByMaterial.Count);
        }

        [TestMethod]
        public async Task Should_Return_Material_Data_With_PublicBin()
        {
            // Assign
            var resultsDto = new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2025) };
            var calcResult = TestDataHelper.GetCalcResult();
            SeedDatabase(dbContext);

            // Act
            var lapcapDisposalCostResults = await builder.ConstructAsync(resultsDto, TestDataHelper.GetMaterials(), calcResult.CalcResultLapcapData, calcResult.CalcResultLateReportingTonnageData, smcw, calcResult.ApplyModulation);

            // Assert
            var laDisposalCostKv = lapcapDisposalCostResults.ByMaterial.Single(kv => kv.Key.Name == MaterialNames.Plastic);

            Assert.IsNotNull(laDisposalCostKv.Value);
            Assert.AreEqual(MaterialNames.Plastic, laDisposalCostKv.Key.Name);
            Assert.AreEqual(23000, laDisposalCostKv.Value.England);
            Assert.AreEqual(4500, laDisposalCostKv.Value.Wales);
            Assert.AreEqual(6700, laDisposalCostKv.Value.Scotland);
            Assert.AreEqual(2100, laDisposalCostKv.Value.NorthernIreland);
            Assert.AreEqual(36300, laDisposalCostKv.Value.Total);

            Assert.AreEqual(400, laDisposalCostKv.Value.ProducerReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual(0, laDisposalCostKv.Value.ReportedPublicBinTonnage);
            Assert.AreEqual(null, laDisposalCostKv.Value.HouseholdDrinkContainers);
            Assert.AreEqual(2000, laDisposalCostKv.Value.LateReportingTonnage);
            Assert.AreEqual(2400, laDisposalCostKv.Value.ProducerReportedTotalTonnage);
        }


        [TestMethod]
        public async Task Should_Apply_Modulations()
        {
            // Assign
            var resultsDto = new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2025) };
            var calcResult = TestDataHelper.GetCalcResult(applyModulation: true);
            SeedDatabase(dbContext);

            static SelfManagedConsumerWasteData MkSelfManagedConsumerWasteData(decimal red, decimal amber, decimal green)
            {
                return new SelfManagedConsumerWasteData
                {
                    SelfManagedConsumerWasteTonnage         = amber,
                    ActionedSelfManagedConsumerWasteTonnage = (total: amber      , red: 0, amber   , green: 0),
                    ResidualSelfManagedConsumerWasteTonnage = 0,
                    NetReportedTonnage                      = (total: red + green, red   , amber: 0, green)
                };
            }

            var smcw = new SelfManagedConsumerWaste
                {
                    ProducerTotals = [],
                    OverallTotalPerMaterials = new Dictionary<string, SelfManagedConsumerWasteData>
                    {
                        [MaterialCodes.Aluminium]      = MkSelfManagedConsumerWasteData(red:  220, amber:  330, green:  550),
                        [MaterialCodes.FibreComposite] = MkSelfManagedConsumerWasteData(red:  275, amber:   55, green:   55),
                        [MaterialCodes.Glass]          = MkSelfManagedConsumerWasteData(red:  110, amber:  220, green:  220),
                        [MaterialCodes.Plastic]        = MkSelfManagedConsumerWasteData(red:  400, amber: 1050, green: 2400),
                        [MaterialCodes.PaperOrCard]    = MkSelfManagedConsumerWasteData(red: 2150, amber:  275, green:  270),
                        [MaterialCodes.Steel]          = MkSelfManagedConsumerWasteData(red:   33, amber:   40, green:   74),
                        [MaterialCodes.Wood]           = MkSelfManagedConsumerWasteData(red:  265, amber:    0, green:    0),
                        [MaterialCodes.OtherMaterials] = MkSelfManagedConsumerWasteData(red:   30, amber:    0, green:    0)
                    }
                };
            // Act
            var lapcapDisposalCostResults = await builder.ConstructAsync(resultsDto, TestDataHelper.GetMaterials(), calcResult.CalcResultLapcapData, calcResult.CalcResultLateReportingTonnageData, smcw, calcResult.ApplyModulation);

            // Assert
            var plasticKv = lapcapDisposalCostResults.ByMaterial.Single(kv => kv.Key.Name == MaterialNames.Plastic);
            Assert.IsNotNull(plasticKv.Value);
            Assert.AreEqual(MaterialNames.Plastic, plasticKv.Key.Name);
            Assert.AreEqual(1050, plasticKv.Value.ActionedSelfManagedConsumerWasteTonnage);
            Assert.AreEqual(1350, plasticKv.Value.ProducerReportedTotalTonnage);

            var steelKv = lapcapDisposalCostResults.ByMaterial.Single(kv => kv.Key.Name == MaterialNames.Steel);
            Assert.IsNotNull(steelKv.Value);
            Assert.AreEqual(MaterialNames.Steel, steelKv.Key.Name);
            Assert.AreEqual(40 , steelKv.Value.ActionedSelfManagedConsumerWasteTonnage);
            Assert.AreEqual(360, steelKv.Value.ProducerReportedTotalTonnage);

            var glassKv = lapcapDisposalCostResults.ByMaterial.Single(kv => kv.Key.Name == MaterialNames.Glass);
            Assert.IsNotNull(glassKv.Value);
            Assert.AreEqual(MaterialNames.Glass, glassKv.Key.Name);
            Assert.AreEqual(220, glassKv.Value.ActionedSelfManagedConsumerWasteTonnage);
            Assert.AreEqual(290, glassKv.Value.ProducerReportedTotalTonnage);
        }

        [TestMethod]
        public async Task Should_Return_Material_Data_With_Household_Drink_Containers()
        {
            // Assign
            var resultsDto = new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2025) };
            var calcResult = TestDataHelper.GetCalcResult();
            SeedDatabase(dbContext);

            // Act
            var lapcapDisposalCostResults = await builder.ConstructAsync(resultsDto, TestDataHelper.GetMaterials(), calcResult.CalcResultLapcapData, calcResult.CalcResultLateReportingTonnageData, smcw, calcResult.ApplyModulation);

            // Assert
            var culture = CultureInfo.GetCultureInfo("en-GB");
            var glassKv = lapcapDisposalCostResults.ByMaterial.Single(kv => kv.Key.Name == MaterialNames.Glass);
            Assert.IsNotNull(glassKv.Value);
            Assert.AreEqual(MaterialNames.Glass, glassKv.Key.Name);
            Assert.AreEqual(45000, glassKv.Value.England);
            Assert.AreEqual(0, glassKv.Value.Wales);
            Assert.AreEqual(20700, glassKv.Value.Scotland);
            Assert.AreEqual(4500, glassKv.Value.NorthernIreland);
            Assert.AreEqual(70200, glassKv.Value.Total);
            Assert.AreEqual(0, glassKv.Value.ProducerReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual(0, glassKv.Value.ReportedPublicBinTonnage);
            Assert.AreEqual(500, glassKv.Value.HouseholdDrinkContainers);
            Assert.AreEqual(10, glassKv.Value.LateReportingTonnage);
            Assert.AreEqual(510, glassKv.Value.ProducerReportedTotalTonnage);
        }

        [TestMethod]
        public async Task Should_Calculate_ProducerDataTotal_For_Total_Material()
        {
            // Arrange
            // Assign
            var resultsDto = new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2025) };
            var calcResult = TestDataHelper.GetCalcResult();
            SeedDatabase(dbContext);

            // Act
            var lapcapDisposalCostResults = await builder.ConstructAsync(resultsDto, TestDataHelper.GetMaterials(), calcResult.CalcResultLapcapData, calcResult.CalcResultLateReportingTonnageData, smcw, calcResult.ApplyModulation);

            // Assert
            var laDisposalCost = lapcapDisposalCostResults.Total;
            Assert.IsNotNull(laDisposalCost);
            Assert.AreEqual(400, laDisposalCost.ProducerReportedHouseholdPackagingWasteTonnage);
        }


        [TestMethod]
        public async Task Should_Calculate_ProducerDataTotal_For_Specific_Material()
        {
            // Assign
            var resultsDto = new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2025) };
            var calcResult = TestDataHelper.GetCalcResult();
            SeedDatabase(dbContext);

            // Act
            var lapcapDisposalCostResults = await builder.ConstructAsync(resultsDto, TestDataHelper.GetMaterials(), calcResult.CalcResultLapcapData, calcResult.CalcResultLateReportingTonnageData, smcw, calcResult.ApplyModulation);

            // Assert
            var plasticKv = lapcapDisposalCostResults.ByMaterial.Single(kv => kv.Key.Name == MaterialNames.Plastic);
            Assert.IsNotNull(plasticKv.Value);
            Assert.AreEqual(400, plasticKv.Value.ProducerReportedHouseholdPackagingWasteTonnage);
        }

        private static void SeedDatabase(ApplicationDBContext context)
        {
            var run = new CalculatorRun { Id = 1, RelativeYear = new RelativeYear(2024), Name = "CalculatorRunTest1" };
            context.CalculatorRuns.Add(run);

            var producerDetail = new List<ProducerDetail>
            {
                new ProducerDetail
                {
                    Id = 3, CalculatorRunId = 1,
                },
                new ProducerDetail
                {
                    Id = 2, CalculatorRunId = 1,
                },
            };
            context.ProducerDetail.AddRange(producerDetail);

            var materials = new List<Material>
            {
                new Material { Id = 1, Name = "Plastic", Code = MaterialCodes.Plastic },
                new Material { Id = 2, Name = "Steel", Code = MaterialCodes.Steel },
                new Material { Id = 3, Name = "Glass", Code = MaterialCodes.Glass },
            };
            context.Material.AddRange(materials);

            var producerReportedMaterials = new List<ProducerReportedMaterialProjected>
            {
                new ProducerReportedMaterialProjected { ProducerDetailId = 3, MaterialId = 1, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.Household, PackagingTonnage = 25 },
                new ProducerReportedMaterialProjected { ProducerDetailId = 3, MaterialId = 1, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.Household, PackagingTonnage = 75 },
                new ProducerReportedMaterialProjected { ProducerDetailId = 2, MaterialId = 1, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.Household, PackagingTonnage = 100 },
                new ProducerReportedMaterialProjected { ProducerDetailId = 2, MaterialId = 1, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.Household, PackagingTonnage = 200 },
                new ProducerReportedMaterialProjected { ProducerDetailId = 3, MaterialId = 2, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 100 },
                new ProducerReportedMaterialProjected { ProducerDetailId = 3, MaterialId = 2, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 100 },
                new ProducerReportedMaterialProjected { ProducerDetailId = 2, MaterialId = 2, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 50 },
                new ProducerReportedMaterialProjected { ProducerDetailId = 2, MaterialId = 2, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 150 },
                new ProducerReportedMaterialProjected { ProducerDetailId = 1, MaterialId = 3, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 150 },
                new ProducerReportedMaterialProjected { ProducerDetailId = 1, MaterialId = 3, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 150 },
                new ProducerReportedMaterialProjected { ProducerDetailId = 2, MaterialId = 3, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 200 },
                new ProducerReportedMaterialProjected { ProducerDetailId = 2, MaterialId = 3, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 300 },
            };
            context.ProducerReportedMaterialProjected.AddRange(producerReportedMaterials);

            context.SaveChanges();
        }
    }
}
