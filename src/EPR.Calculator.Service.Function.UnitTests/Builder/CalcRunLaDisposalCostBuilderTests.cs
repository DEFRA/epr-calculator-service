namespace EPR.Calculator.Service.Function.UnitTests.Builder
{
    using System;
    using System.Globalization;
    using AutoFixture;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.Service.Function.Builder.LaDisposalCost;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Dtos;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalcRunLaDisposalCostBuilderTests
    {
        private readonly CalcRunLaDisposalCostBuilder builder;
        private readonly ApplicationDBContext dbContext;

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
            this.Fixture = new Fixture();
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                                    .UseInMemoryDatabase(databaseName: "PayCal")
                                    .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                                    .Options;

            this.dbContext = new ApplicationDBContext(dbContextOptions);
            this.dbContext.Database.EnsureCreated();
            this.builder = new CalcRunLaDisposalCostBuilder(this.dbContext);
        }

        private Fixture Fixture { get; init; }

        [TestCleanup]
        public void TearDown()
        {
            this.dbContext?.Database.EnsureDeleted();
            this.dbContext?.Dispose();
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
        public void TestProducerDataFiltering()
        {
            // Arrange
            var producerData = new List<ProducerData>
            {
                new ProducerData { ProducerDetail = new ProducerDetail { ProducerId = 1 }, MaterialName = "Plastic", PackagingType = "PB" },
                new ProducerData { ProducerDetail = new ProducerDetail { ProducerId = 2 }, MaterialName = "Glass", PackagingType = "HDC" },
            };

            var resultsDto = new CalcResultsRequestDto { RunId = 2 };
            var calcResult = TestDataHelper.GetCalcResult();
            calcResult.CalcResultScaledupProducers = new CalcResultScaledupProducers
            {
                ScaledupProducers = new List<CalcResultScaledupProducer>
                {
                    new CalcResultScaledupProducer { ProducerId = 1 },
                    new CalcResultScaledupProducer { ProducerId = 3 },
                },
            };

            // Act
            var filteredData = producerData.Where(t => !calcResult.CalcResultScaledupProducers.ScaledupProducers.Any(i => i.ProducerId == t?.ProducerDetail?.ProducerId)).ToList();

            // Assert
            Assert.AreEqual(1, filteredData.Count);
            Assert.AreEqual(2, filteredData?.First().ProducerDetail?.ProducerId);
        }

        [TestMethod]
        public void Should_Return_LA_Disposal_Costs()
        {
            // Assign
            var resultsDto = new CalcResultsRequestDto { RunId = 2 };
            var calcResult = TestDataHelper.GetCalcResult();
            calcResult.CalcResultScaledupProducers = GetScaledUpProducers();

            // Act
            var results = this.builder.Construct(resultsDto, calcResult);
            results.Wait();
            var lapcapDisposalCostResults = results.Result;

            // Assert
            Assert.IsNotNull(lapcapDisposalCostResults);
            Assert.AreEqual(CommonConstants.LADisposalCostData, lapcapDisposalCostResults.Name);
            Assert.AreEqual(10, lapcapDisposalCostResults.CalcResultLaDisposalCostDetails?.Count());
        }

        [TestMethod]
        public void Should_Return_HeaderRow()
        {
            // Assign
            var resultsDto = new CalcResultsRequestDto { RunId = 2 };
            var calcResult = TestDataHelper.GetCalcResult();
            calcResult.CalcResultScaledupProducers = GetScaledUpProducers();

            // Act
            var results = this.builder.Construct(resultsDto, calcResult);
            results.Wait();
            var lapcapDisposalCostResults = results.Result;

            // Assert
            var headerRow = lapcapDisposalCostResults.CalcResultLaDisposalCostDetails?.Single(x => x.OrderId == 1);
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
        public void Should_Return_Material_Data_With_PublicBin()
        {
            // Assign
            var resultsDto = new CalcResultsRequestDto { RunId = 1 };
            var calcResult = TestDataHelper.GetCalcResult();
            SeedDatabase(this.dbContext);

            calcResult.CalcResultScaledupProducers = new CalcResultScaledupProducers()
            {
                ScaledupProducers = new List<CalcResultScaledupProducer>()
                 {
                      new CalcResultScaledupProducer()
                      {
                          ProducerId = 1,
                          IsTotalRow = true,
                          ScaledupProducerTonnageByMaterial = new()
                            {
                                ["Plastic"] = new CalcResultScaledupProducerTonnage
                                {
                                        ReportedHouseholdPackagingWasteTonnage = 1000,
                                        ReportedPublicBinTonnage = 2000,
                                        TotalReportedTonnage = 3000,
                                        ReportedSelfManagedConsumerWasteTonnage = 1000,
                                        NetReportedTonnage = 5000,
                                        ScaledupReportedHouseholdPackagingWasteTonnage = 300,
                                        ScaledupReportedPublicBinTonnage = 400,
                                        ScaledupTotalReportedTonnage = 500,
                                        ScaledupReportedSelfManagedConsumerWasteTonnage = 100,
                                        ScaledupNetReportedTonnage = 100,
                                },
                            },
                      },
                 },
            };

            // Act
            var results = this.builder.Construct(resultsDto, calcResult);
            results.Wait();
            var lapcapDisposalCostResults = results.Result;

            // Assert
            var laDisposalCost = lapcapDisposalCostResults.CalcResultLaDisposalCostDetails?.Single(x => x.Name == MaterialNames.Plastic);

            Assert.IsNotNull(laDisposalCost);
            Assert.AreEqual(MaterialNames.Plastic, laDisposalCost.Name);
            Assert.IsTrue(laDisposalCost.England.Contains("23,000.00"));
            Assert.IsTrue(laDisposalCost.Wales.Contains("4,500.00"));
            Assert.IsTrue(laDisposalCost.Scotland.Contains("6,700.00"));
            Assert.IsTrue(laDisposalCost.NorthernIreland.Contains("2,100.00"));
            Assert.IsTrue(laDisposalCost.Total.Contains("36,300.00"));

            Assert.AreEqual("700", laDisposalCost.ProducerReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual("400", laDisposalCost.ReportedPublicBinTonnage);
            Assert.AreEqual(string.Empty, laDisposalCost.HouseholdDrinkContainers);
            Assert.AreEqual("2000.00", laDisposalCost.LateReportingTonnage);
            Assert.AreEqual("3100.00", laDisposalCost.ProducerReportedTotalTonnage);
        }

        [TestMethod]
        public void Should_Return_Material_Data_With_Household_Drink_Containers()
        {
            // Assign
            var resultsDto = new CalcResultsRequestDto { RunId = 1 };
            var calcResult = TestDataHelper.GetCalcResult();
            SeedDatabase(this.dbContext);

            calcResult.CalcResultScaledupProducers = new CalcResultScaledupProducers()
            {
                ScaledupProducers = new List<CalcResultScaledupProducer>()
                 {
                      new CalcResultScaledupProducer()
                      {
                          ProducerId = 1,
                          IsTotalRow = true,
                          ScaledupProducerTonnageByMaterial = new()
                            {
                                ["Glass"] = new CalcResultScaledupProducerTonnage
                                {
                                        ReportedHouseholdPackagingWasteTonnage = 1000,
                                        ReportedPublicBinTonnage = 0,
                                        TotalReportedTonnage = 3000,
                                        ReportedSelfManagedConsumerWasteTonnage = 1000,
                                        NetReportedTonnage = 5000,
                                        ScaledupReportedHouseholdPackagingWasteTonnage = 300,
                                        ScaledupReportedPublicBinTonnage = 400,
                                        ScaledupTotalReportedTonnage = 500,
                                        ScaledupReportedSelfManagedConsumerWasteTonnage = 100,
                                        ScaledupHouseholdDrinksContainersTonnageGlass = 1000,
                                        ScaledupNetReportedTonnage = 100,
                                },
                            },
                      },
                 },
            };

            // Act
            var results = this.builder.Construct(resultsDto, calcResult);
            results.Wait();
            var lapcapDisposalCostResults = results.Result;

            // Assert
            var laDisposalCost = lapcapDisposalCostResults.CalcResultLaDisposalCostDetails?.Single(x => x.Name == MaterialNames.Glass);
            Assert.IsNotNull(laDisposalCost);
            Assert.AreEqual(MaterialNames.Glass, laDisposalCost.Name);
            Assert.IsTrue(laDisposalCost.England.Contains("45,000.00"));
            Assert.IsTrue(laDisposalCost.Wales.Contains("0.00"));
            Assert.IsTrue(laDisposalCost.Scotland.Contains("20,700.00"));
            Assert.IsTrue(laDisposalCost.NorthernIreland.Contains("4,500.00"));
            Assert.IsTrue(laDisposalCost.Total.Contains("70,200.00"));
            Assert.IsTrue(laDisposalCost.ProducerReportedHouseholdPackagingWasteTonnage.Contains("300"));
            Assert.IsTrue(laDisposalCost.ReportedPublicBinTonnage.Contains("0"));
            Assert.IsTrue(laDisposalCost.HouseholdDrinkContainers.Contains("1500"));
            Assert.IsTrue(laDisposalCost.LateReportingTonnage.Contains("0"));
            Assert.IsTrue(laDisposalCost.ProducerReportedTotalTonnage.Contains("2200"));
        }

        [TestMethod]
        public void Should_Return_Material_Data_With_Household_Drink_Containers_NoScaledUpData()
        {
            // Assign
            var resultsDto = new CalcResultsRequestDto { RunId = 1 };
            var calcResult = TestDataHelper.GetCalcResult();
            SeedDatabase(this.dbContext);

            calcResult.CalcResultScaledupProducers = new CalcResultScaledupProducers()
            {
                ScaledupProducers = new List<CalcResultScaledupProducer>(),
            };

            // Act
            var results = this.builder.Construct(resultsDto, calcResult);
            results.Wait();
            var lapcapDisposalCostResults = results.Result;

            // Assert
            var culture = CultureInfo.GetCultureInfo("en-GB");
            var laDisposalCost = lapcapDisposalCostResults.CalcResultLaDisposalCostDetails?.Single(x => x.Name == MaterialNames.Glass);
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
            Assert.AreEqual(0, double.Parse(laDisposalCost.LateReportingTonnage));
            Assert.AreEqual(500, double.Parse(laDisposalCost.ProducerReportedTotalTonnage));
        }

        [TestMethod]
        public async Task Should_Calculate_ProducerDataTotal_For_Total_Material()
        {
            // Arrange
            // Assign
            var resultsDto = new CalcResultsRequestDto { RunId = 1 };
            var calcResult = TestDataHelper.GetCalcResult();
            SeedDatabase(this.dbContext);

            calcResult.CalcResultScaledupProducers = GetScaledUpProducers();

            // Act
            var lapcapDisposalCostResults = await this.builder.Construct(resultsDto, calcResult);

            // Assert
            var laDisposalCost = lapcapDisposalCostResults.CalcResultLaDisposalCostDetails?.Single(x => x.Name == CommonConstants.Total);
            Assert.IsNotNull(laDisposalCost);
            Assert.AreEqual("700", laDisposalCost.ProducerReportedHouseholdPackagingWasteTonnage);
        }

        [TestMethod]
        public async Task Should_Calculate_ProducerDataTotal_For_Specific_Material()
        {
             // Assign
            var resultsDto = new CalcResultsRequestDto { RunId = 1 };
            var calcResult = TestDataHelper.GetCalcResult();
            SeedDatabase(this.dbContext);

            calcResult.CalcResultScaledupProducers = new CalcResultScaledupProducers()
            {
                ScaledupProducers = new List<CalcResultScaledupProducer>()
                 {
                      new CalcResultScaledupProducer()
                      {
                          ProducerId = 1,
                          IsTotalRow = true,
                          ScaledupProducerTonnageByMaterial = new()
                            {
                                ["Plastic"] = new CalcResultScaledupProducerTonnage
                                {
                                        ReportedHouseholdPackagingWasteTonnage = 1000,
                                        ReportedPublicBinTonnage = 2000,
                                        TotalReportedTonnage = 3000,
                                        ReportedSelfManagedConsumerWasteTonnage = 1000,
                                        NetReportedTonnage = 5000,
                                        ScaledupReportedHouseholdPackagingWasteTonnage = 300,
                                        ScaledupReportedPublicBinTonnage = 400,
                                        ScaledupTotalReportedTonnage = 500,
                                        ScaledupReportedSelfManagedConsumerWasteTonnage = 100,
                                        ScaledupNetReportedTonnage = 100,
                                },
                            },
                      },
                 },
            };

            // Act
            var lapcapDisposalCostResults = await this.builder.Construct(resultsDto, calcResult);

            // Assert
            var laDisposalCost = lapcapDisposalCostResults.CalcResultLaDisposalCostDetails?.Single(x => x.Name == MaterialNames.Plastic);
            Assert.IsNotNull(laDisposalCost);
            Assert.AreEqual("700", laDisposalCost.ProducerReportedHouseholdPackagingWasteTonnage);
        }

        [TestMethod]
        public async Task Should_Calculate_ProducerDataTotal_For_Specific_Material_NoScaledUpData()
        {
            // Assign
            var resultsDto = new CalcResultsRequestDto { RunId = 1 };
            var calcResult = TestDataHelper.GetCalcResult();
            SeedDatabase(this.dbContext);
            calcResult.CalcResultScaledupProducers = new CalcResultScaledupProducers()
            {
                ScaledupProducers = new List<CalcResultScaledupProducer>(),
            };

            // Act
            var lapcapDisposalCostResults = await this.builder.Construct(resultsDto, calcResult);

            // Assert
            var laDisposalCost = lapcapDisposalCostResults.CalcResultLaDisposalCostDetails?.Single(x => x.Name == MaterialNames.Plastic);
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

        private static CalcResultScaledupProducers GetScaledUpProducers()
        {
            return new CalcResultScaledupProducers()
            {
                ScaledupProducers = new List<CalcResultScaledupProducer>()
                {
                     new CalcResultScaledupProducer()
                     {
                        ProducerId = 1,
                        IsTotalRow = true,
                        ScaledupProducerTonnageByMaterial = new()
                        {
                            ["Aluminium"] = new CalcResultScaledupProducerTonnage
                            {
                                ReportedHouseholdPackagingWasteTonnage = 1000,
                                ReportedPublicBinTonnage = 2000,
                                TotalReportedTonnage = 3000,
                                ReportedSelfManagedConsumerWasteTonnage = 1000,
                                NetReportedTonnage = 5000,
                                ScaledupReportedHouseholdPackagingWasteTonnage = 300,
                                ScaledupReportedPublicBinTonnage = 400,
                                ScaledupTotalReportedTonnage = 500,
                                ScaledupReportedSelfManagedConsumerWasteTonnage = 100,
                                ScaledupNetReportedTonnage = 100,
                            },
                        },
                     },
                     new CalcResultScaledupProducer()
                     {
                        ProducerId = 1,
                        IsTotalRow = true,
                        ScaledupProducerTonnageByMaterial = new()
                        {
                            ["Glass"] = new CalcResultScaledupProducerTonnage
                            {
                                ReportedHouseholdPackagingWasteTonnage = 1000,
                                ReportedPublicBinTonnage = 2000,
                                TotalReportedTonnage = 3000,
                                ReportedSelfManagedConsumerWasteTonnage = 1000,
                                NetReportedTonnage = 5000,
                                ScaledupReportedHouseholdPackagingWasteTonnage = 300,
                                ScaledupReportedPublicBinTonnage = 400,
                                ScaledupTotalReportedTonnage = 500,
                                ScaledupReportedSelfManagedConsumerWasteTonnage = 100,
                                ScaledupNetReportedTonnage = 100,
                            },
                        },
                     },
                     new CalcResultScaledupProducer()
                     {
                        ProducerId = 1,
                        IsTotalRow = true,
                        ScaledupProducerTonnageByMaterial = new()
                        {
                            ["Plastic"] = new CalcResultScaledupProducerTonnage
                            {
                                 ReportedHouseholdPackagingWasteTonnage = 1000,
                                 ReportedPublicBinTonnage = 2000,
                                 TotalReportedTonnage = 3000,
                                 ReportedSelfManagedConsumerWasteTonnage = 1000,
                                 NetReportedTonnage = 5000,
                                 ScaledupReportedHouseholdPackagingWasteTonnage = 300,
                                 ScaledupReportedPublicBinTonnage = 400,
                                 ScaledupTotalReportedTonnage = 500,
                                 ScaledupReportedSelfManagedConsumerWasteTonnage = 100,
                                 ScaledupNetReportedTonnage = 100,
                            },
                        },
                     },
                },
            };
        }

        private static void SeedDatabase(ApplicationDBContext context)
        {
            var calculatorRunFinancialYear = new CalculatorRunFinancialYear { Name = "2024-25" };

            var run = new CalculatorRun { Id = 1, Financial_Year = calculatorRunFinancialYear, Name = "CalculatorRunTest1" };
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

            var producerReportedMaterials = new List<ProducerReportedMaterial>
            {
                new ProducerReportedMaterial { Id = 1, ProducerDetailId = 3, MaterialId = 1, PackagingType = PackagingTypes.Household, PackagingTonnage = 100 },
                new ProducerReportedMaterial { Id = 5, ProducerDetailId = 2, MaterialId = 1, PackagingType = PackagingTypes.Household, PackagingTonnage = 300 },
                new ProducerReportedMaterial { Id = 2, ProducerDetailId = 3, MaterialId = 2, PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 200 },
                new ProducerReportedMaterial { Id = 4, ProducerDetailId = 2, MaterialId = 2, PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 200 },
                new ProducerReportedMaterial { Id = 3, ProducerDetailId = 1, MaterialId = 3, PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 300 },
                new ProducerReportedMaterial { Id = 6, ProducerDetailId = 2, MaterialId = 3, PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 500 },
            };
            context.ProducerReportedMaterial.AddRange(producerReportedMaterials);

            context.SaveChanges();
        }
    }
}