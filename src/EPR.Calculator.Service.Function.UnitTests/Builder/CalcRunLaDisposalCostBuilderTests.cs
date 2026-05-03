using System.Globalization;
using AutoFixture;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Builder.LaDisposalCost;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

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
            Assert.AreEqual(CommonConstants.LADisposalCostData, lapcapDisposalCostResults.Name);
            Assert.AreEqual(10, lapcapDisposalCostResults.CalcResultLaDisposalCostDetails.Count());
        }

        [TestMethod]
        public async Task Should_Return_HeaderRow()
        {
            // Assign
            var resultsDto = new CalcResultsRequestDto { RunId = 2, RelativeYear = new RelativeYear(2025) };
            var calcResult = TestDataHelper.GetCalcResult();

            // Act
            var lapcapDisposalCostResults = await builder.ConstructAsync(resultsDto, TestDataHelper.GetMaterials(), calcResult.CalcResultLapcapData, calcResult.CalcResultLateReportingTonnageData, smcw, calcResult.ApplyModulation);

            // Assert
            var headerRow = lapcapDisposalCostResults.CalcResultLaDisposalCostDetails.Single(x => x.OrderId == 1);
            Assert.IsNotNull(headerRow);
            Assert.AreEqual(CommonConstants.Material, headerRow.Name);
            Assert.AreEqual(CommonConstants.England, headerRow.England);
            Assert.AreEqual(CommonConstants.Wales, headerRow.Wales);
            Assert.AreEqual(CommonConstants.Scotland, headerRow.Scotland);
            Assert.AreEqual(CommonConstants.NorthernIreland, headerRow.NorthernIreland);
            Assert.AreEqual(CommonConstants.Total, headerRow.Total);
            Assert.AreEqual(CommonConstants.ProducerReportedHouseholdPackagingWasteTonnage, headerRow.ProducerReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual(CommonConstants.ReportedPublicBinTonnage, headerRow.ReportedPublicBinTonnage);
            Assert.AreEqual(CommonConstants.HouseholdDrinkContainers, headerRow.HouseholdDrinkContainers);
            Assert.AreEqual(CommonConstants.LateReportingTonnage, headerRow.LateReportingTonnage);
            Assert.AreEqual(CommonConstants.ProducerReportedTotalTonnage, headerRow.ProducerReportedTotalTonnage);
            Assert.AreEqual(CommonConstants.DisposalCostPricePerTonne, headerRow.DisposalCostPricePerTonne);
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
            var laDisposalCost = lapcapDisposalCostResults.CalcResultLaDisposalCostDetails.Single(x => x.Name == MaterialNames.Plastic);

            Assert.IsNotNull(laDisposalCost);
            Assert.AreEqual(MaterialNames.Plastic, laDisposalCost.Name);
            Assert.IsTrue(laDisposalCost.England.Contains("23,000.00"));
            Assert.IsTrue(laDisposalCost.Wales.Contains("4,500.00"));
            Assert.IsTrue(laDisposalCost.Scotland.Contains("6,700.00"));
            Assert.IsTrue(laDisposalCost.NorthernIreland.Contains("2,100.00"));
            Assert.IsTrue(laDisposalCost.Total.Contains("36,300.00"));

            Assert.AreEqual("400", laDisposalCost.ProducerReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual("0", laDisposalCost.ReportedPublicBinTonnage);
            Assert.AreEqual(string.Empty, laDisposalCost.HouseholdDrinkContainers);
            Assert.AreEqual("2000.00", laDisposalCost.LateReportingTonnage);
            Assert.AreEqual("2400.00", laDisposalCost.ProducerReportedTotalTonnage);
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
            var laDisposalCost = lapcapDisposalCostResults.CalcResultLaDisposalCostDetails.Single(x => x.Name == MaterialNames.Glass);
            Assert.IsNotNull(laDisposalCost);
            Assert.AreEqual(MaterialNames.Glass, laDisposalCost.Name);
            Assert.AreEqual(45000.00, double.Parse(laDisposalCost.England, NumberStyles.Currency, culture));
            Assert.AreEqual(0, double.Parse(laDisposalCost.Wales, NumberStyles.Currency, culture));
            Assert.AreEqual(20700.00, double.Parse(laDisposalCost.Scotland, NumberStyles.Currency, culture));
            Assert.AreEqual(4500.00, double.Parse(laDisposalCost.NorthernIreland, NumberStyles.Currency, culture));
            Assert.AreEqual(70200.00, double.Parse(laDisposalCost.Total, NumberStyles.Currency, culture));
            Assert.AreEqual(0, double.Parse(laDisposalCost.ProducerReportedHouseholdPackagingWasteTonnage));
            Assert.AreEqual(0, double.Parse(laDisposalCost.ReportedPublicBinTonnage));
            Assert.AreEqual(500, double.Parse(laDisposalCost.HouseholdDrinkContainers));
            Assert.AreEqual(10, double.Parse(laDisposalCost.LateReportingTonnage));
            Assert.AreEqual(510, double.Parse(laDisposalCost.ProducerReportedTotalTonnage));
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
            var laDisposalCost = lapcapDisposalCostResults.CalcResultLaDisposalCostDetails.Single(x => x.Name == CommonConstants.Total);
            Assert.IsNotNull(laDisposalCost);
            Assert.AreEqual("400", laDisposalCost.ProducerReportedHouseholdPackagingWasteTonnage);
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
            var laDisposalCost = lapcapDisposalCostResults.CalcResultLaDisposalCostDetails.Single(x => x.Name == MaterialNames.Plastic);
            Assert.IsNotNull(laDisposalCost);
            Assert.AreEqual(400, double.Parse(laDisposalCost.ProducerReportedHouseholdPackagingWasteTonnage));
        }

        [TestMethod]
        public void GetDecimalValue_InvalidDecimalString_ReturnsZero()
        {
            // Arrange
            string value = "invalid";

            // Act
            decimal result = CalcRunLaDisposalCostBuilder.GetDecimalValue(value);

            // Assert
            Assert.AreEqual(0m, result);
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
