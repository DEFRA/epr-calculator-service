﻿namespace EPR.Calculator.Service.Function.UnitTests.Builder
{
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.Service.Function.Builder.ScaledupProducers;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Dtos;
    using EPR.Calculator.Service.Function.Mappers;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using static EPR.Calculator.Service.Function.UnitTests.Builder.CalcRunLaDisposalCostBuilderTests;

    [TestClass]
    public class CalcResultScaledupProducersBuilderTest
    {
        private readonly ApplicationDBContext dbContext;
        private readonly int runId = 1;
        private CalcResultScaledupProducersBuilder builder;

        private void PrepareNonScaledUpProducer()
        {
            var producerDetail = new ProducerDetail
            {
                Id = 1,
                CalculatorRunId = this.runId,
                ProducerId = 11,
                SubsidiaryId = "Subsidary 1",
                ProducerName = "Producer Name",
            };
            this.dbContext.ProducerDetail.Add(producerDetail);
            this.dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
            {
                Id = 1,
                PackagingType = "HH",
                ProducerDetail = producerDetail,
            });
            this.dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
            {
                Id = 2,
                PackagingType = "HDC",
                ProducerDetail = producerDetail,
            });
            var calcRunPomDataMaster = new CalculatorRunPomDataMaster
            {
                Id = 1,
                CalendarYear = "2024",
                EffectiveFrom = DateTime.Now,
                CreatedAt = DateTime.Now,
                CreatedBy = "Test User",
            };
            this.dbContext.CalculatorRunPomDataMaster.Add(calcRunPomDataMaster);
            this.dbContext.CalculatorRuns.Add(new CalculatorRun
            {
                Id = runId,
                Financial_Year = "2024-25",
                Name = "Name",
                CalculatorRunPomDataMaster = calcRunPomDataMaster,
            });
            this.dbContext.CalculatorRunPomDataDetails.Add(
                new CalculatorRunPomDataDetail
                {
                    LoadTimeStamp = DateTime.Now,
                    SubmissionPeriod = "2024-P1",
                    SubmissionPeriodDesc = "desc",
                    CalculatorRunPomDataMaster = calcRunPomDataMaster,
                    OrganisationId = 10,
                });

            this.dbContext.SubmissionPeriodLookup.Add(
                new SubmissionPeriodLookup
                {
                    DaysInSubmissionPeriod = 0,
                    DaysInWholePeriod = 0,
                    EndDate = DateTime.Now,
                    StartDate = DateTime.Now,
                    ScaleupFactor = 1,
                    SubmissionPeriod = "2024-P1",
                    SubmissionPeriodDesc = string.Empty,
                });

            this.dbContext.CalculatorRuns.Add(
                new CalculatorRun
                {
                    Id = 2,
                    Financial_Year = "2024-25",
                    Name = "Name",
                });

            var producerDetail1 = new ProducerDetail
            {
                Id = 2,
                CalculatorRunId = 2,
                ProducerId = 11,
                ProducerName = "Producer Test",
            };
            this.dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
            {
                Id = 3,
                PackagingType = "HH",
                ProducerDetail = producerDetail1,
            });

            this.dbContext.SaveChanges();
        }

        private void PrepareScaledUpProducer()
        {
            this.dbContext.CalculatorRunPomDataDetails.Add(
            new CalculatorRunPomDataDetail
            {
                LoadTimeStamp = DateTime.Now,
                SubmissionPeriod = "2024-P2",
                SubmissionPeriodDesc = "desc",
                CalculatorRunPomDataMaster = this.dbContext.CalculatorRunPomDataMaster.First(),
                OrganisationId = 11,
            });

            this.dbContext.SubmissionPeriodLookup.Add(
            new SubmissionPeriodLookup
            {
                DaysInSubmissionPeriod = 0,
                DaysInWholePeriod = 0,
                EndDate = DateTime.Now,
                StartDate = DateTime.Now,
                ScaleupFactor = 2.999M,
                SubmissionPeriod = "2024-P2",
                SubmissionPeriodDesc = string.Empty,
            });

            this.dbContext.SaveChanges();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CalcResultScaledupProducersBuilderTest"/> class.
        /// </summary>
        public CalcResultScaledupProducersBuilderTest()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseInMemoryDatabase(databaseName: "PayCal")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

            this.dbContext = new ApplicationDBContext(dbContextOptions);
            this.dbContext.Database.EnsureCreated();
            this.builder = new CalcResultScaledupProducersBuilder(this.dbContext);
        }

        [TestCleanup]
        public void Teardown()
        {
            if (this.dbContext != null)
            {
                this.dbContext.Database.EnsureDeleted();
                this.dbContext.Dispose();
            }
        }

        /// <summary>
        /// Tests that the <see cref="ICalcResultScaledupProducersBuilder.Construct(CalcResultsRequestDto)"/>
        /// method returns the correct result when scaled up data is present.
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        [TestMethod]
        public async Task Construct_WhenScaledUpDataPresent()
        {
            // Arrange
            this.PrepareNonScaledUpProducer();
            this.PrepareScaledUpProducer();
            var requestDto = new CalcResultsRequestDto { RunId = 1 };

            // Act
            var result = await this.builder.Construct(requestDto);

            // Assert
            var expectedNumberOfRuns = await this.dbContext.CalculatorRuns.CountAsync(); // The +1 is the totals row.
            Assert.AreEqual(expectedNumberOfRuns, result.ScaledupProducers.Count());
        }

        /// <summary>
        /// Tests that the <see cref="ICalcResultScaledupProducersBuilder.Construct(CalcResultsRequestDto)"/>
        /// method returns the correct result when scaled up data is present.
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        [TestMethod]
        public async Task Construct_ScaledUpDataPresentForThisRunOnly()
        {
            // Arrange
            this.PrepareNonScaledUpProducer();
            this.PrepareScaledUpProducer();
            var requestDto = new CalcResultsRequestDto { RunId = 1 };

            // Act
            var result = await this.builder.Construct(requestDto);

            // Assert
            var actualNumberScaledUpProducer = result.ScaledupProducers.Where(t => !t.IsTotalRow);
            Assert.AreEqual(1, actualNumberScaledUpProducer.Count());
        }

        /// <summary>
        /// Tests that the <see cref="ICalcResultScaledupProducersBuilder.Construct(CalcResultsRequestDto)"/>
        /// method returns the correct result when scaled up data is not present.
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        [TestMethod]
        public async Task Construct_WhenNoScaledUpDataPresent()
        {
            // Arrange
            this.PrepareNonScaledUpProducer();
            var requestDto = new CalcResultsRequestDto { RunId = 1 };

            // Act
            var result = await this.builder.Construct(requestDto);

            // Assert
            Assert.AreEqual(0, result.ScaledupProducers.Count());
        }

        [TestMethod]
        public void GetScaledUpProducerIds_Test()
        {
            this.PrepareNonScaledUpProducer();
            this.PrepareScaledUpProducer();
            var task = this.builder?.GetScaledUpOrganisationIdsAsync(this.runId);
            task?.Wait();

            var result = task?.Result;
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
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
        public void TestProducerDataFilteringForGlass()
        {
            // Arrange
            var producerData = new List<ProducerData>
            {
                new ProducerData { ProducerDetail = new ProducerDetail { ProducerId = 1 }, MaterialName = "Aluminum", PackagingType = "AL" },
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
            Assert.AreEqual(2, filteredData.First().ProducerDetail?.ProducerId);
        }

        [TestMethod]
        public void AddExtraRowsTest()
        {
            this.builder = new CalcResultScaledupProducersBuilder(this.dbContext!);
            var runProducerMaterialDetails = new List<CalcResultScaledupProducer>();
            runProducerMaterialDetails.Add(new CalcResultScaledupProducer
            {
                ProducerId = 1,
            });
            runProducerMaterialDetails.Add(new CalcResultScaledupProducer
            {
                ProducerId = 1,
                SubsidiaryId = "Sub1",
            });
            runProducerMaterialDetails.Add(new CalcResultScaledupProducer
            {
                ProducerId = 1,
                SubsidiaryId = "Sub2",
            });
            runProducerMaterialDetails.Add(new CalcResultScaledupProducer
            {
                ProducerId = 2,
            });
            runProducerMaterialDetails.Add(new CalcResultScaledupProducer
            {
                ProducerId = 2,
                SubsidiaryId = "Sub3",
            });
            runProducerMaterialDetails.Add(new CalcResultScaledupProducer
            {
                ProducerId = 2,
                SubsidiaryId = "Sub4",
            });
            CalcResultScaledupProducersBuilder.AddExtraRows(runProducerMaterialDetails);

            Assert.AreEqual(8, runProducerMaterialDetails.Count);
            var allProducersWithLevel2 = runProducerMaterialDetails.Where(x => x.SubsidiaryId == null);
            Assert.IsTrue(allProducersWithLevel2.All(x => x.Level == CommonConstants.LevelTwo.ToString()));

            var extraRows = runProducerMaterialDetails.Skip(Math.Max(0, runProducerMaterialDetails.Count - 2));
            Assert.AreEqual(2, extraRows.Count());
            Assert.IsTrue(extraRows.All(x => x.IsSubtotalRow));
            Assert.AreEqual(2, runProducerMaterialDetails.Count(x => x.IsSubtotalRow));
        }

        [TestMethod]
        public void GetOverallTotalRowTest()
        {
            this.builder = new CalcResultScaledupProducersBuilder(this.dbContext!);
            var runProducerMaterialDetails = new List<CalcResultScaledupProducer>();
            var dictionary = new Dictionary<string, CalcResultScaledupProducerTonnage>();
            dictionary.Add("AL", new CalcResultScaledupProducerTonnage
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
                ScaledupNetReportedTonnage = 10,
            });
            runProducerMaterialDetails.Add(new CalcResultScaledupProducer
            {
                ProducerId = 1,
                ScaledupProducerTonnageByMaterial = dictionary,
            });
            runProducerMaterialDetails.Add(new CalcResultScaledupProducer
            {
                ProducerId = 1,
                SubsidiaryId = "Sub1",
                ScaledupProducerTonnageByMaterial = dictionary,
            });
            runProducerMaterialDetails.Add(new CalcResultScaledupProducer
            {
                ProducerId = 1,
                SubsidiaryId = "Sub2",
                ScaledupProducerTonnageByMaterial = dictionary,
            });
            runProducerMaterialDetails.Add(new CalcResultScaledupProducer
            {
                ProducerId = 2,
                ScaledupProducerTonnageByMaterial = dictionary,
            });
            runProducerMaterialDetails.Add(new CalcResultScaledupProducer
            {
                ProducerId = 2,
                SubsidiaryId = "Sub3",
                ScaledupProducerTonnageByMaterial = dictionary,
            });
            runProducerMaterialDetails.Add(new CalcResultScaledupProducer
            {
                ProducerId = 2,
                SubsidiaryId = "Sub4",
                ScaledupProducerTonnageByMaterial = dictionary,
            });

            var materials = new List<Material>();
            materials.Add(new Material { Code = "AL", Name = "Aluminium" });
            var materialDetails = MaterialMapper.Map(materials);
            var totalRow = CalcResultScaledupProducersBuilder.GetOverallTotalRow(runProducerMaterialDetails, materialDetails);
            Assert.IsNotNull(totalRow);
            var aluminium = totalRow.ScaledupProducerTonnageByMaterial["Aluminium"];
            Assert.IsNotNull(aluminium);
            Assert.AreEqual(60, aluminium.NetReportedTonnage);
            Assert.AreEqual(60, aluminium.ScaledupTotalReportedTonnage);
            Assert.AreEqual(60, aluminium.ScaledupNetReportedTonnage);
            Assert.AreEqual(60, aluminium.ScaledupReportedPublicBinTonnage);
            Assert.AreEqual(60, aluminium.ScaledupReportedPublicBinTonnage);
        }

        [TestMethod]
        public void GetProducerReportedMaterialsAsyncTest()
        {
            this.builder = new CalcResultScaledupProducersBuilder(this.dbContext!);
            var task = this.builder.GetProducerReportedMaterialsAsync(1, new List<int> { 1, 2 });
            task.Wait();
            var result = task.Result;
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetScaledupOrganisationDetailsTest()
        {
            this.builder = new CalcResultScaledupProducersBuilder(this.dbContext!);
            var task = this.builder.GetScaledupOrganisationDetails(1, new List<int> { 1, 2 });
            task.Wait();
            var result = task.Result;
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public void GetTonnagesTest()
        {
            var materials = new List<Material>();
            materials.Add(new Material { Code = "AL", Name = "Aluminium" });
            var materialDetails = MaterialMapper.Map(materials);
            var pomDateDetails = new List<CalculatorRunPomDataDetail>();
            pomDateDetails.Add(new CalculatorRunPomDataDetail
            {
                LoadTimeStamp = DateTime.Now,
                SubmissionPeriod = "2024-P2",
                SubmissionPeriodDesc = "desc",
                OrganisationId = 11,
                PackagingMaterialWeight = 100,
                PackagingMaterial = "AL",
                PackagingType = "HH",
            });
            var tonnage = CalcResultScaledupProducersBuilder.GetTonnages(pomDateDetails, materialDetails, "2024-P2", 2);
            Assert.IsNotNull(tonnage);
            var aluminium = tonnage["AL"];
            Assert.AreEqual(0.1m, aluminium.ReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual(0.2m, aluminium.ScaledupReportedHouseholdPackagingWasteTonnage);
        }

        [TestMethod]
        public void GetColumnHeadersTest()
        {
            var materials = new List<Material>();
            materials.Add(new Material { Code = "AL", Name = "Aluminium" });
            var materialDetails = MaterialMapper.Map(materials);
            var columnHeaders = CalcResultScaledupProducersBuilder.GetColumnHeaders(materialDetails);
            Assert.IsNotNull(columnHeaders);
            Assert.AreEqual(18, columnHeaders.Count);
        }

        [TestMethod]
        public void GetMaterialsBreakdownHeaderTest()
        {
            var materials = new List<Material>();
            materials.Add(new Material { Code = "AL", Name = "Aluminium" });
            var materialDetails = MaterialMapper.Map(materials);
            var materialsBreakDown = CalcResultScaledupProducersBuilder.GetMaterialsBreakdownHeader(materialDetails);
            Assert.IsNotNull(materialsBreakDown);
            Assert.AreEqual(2, materialsBreakDown.Count);
        }

        [TestMethod]
        public void SetHeadersTest()
        {
            var producers = new CalcResultScaledupProducers
            {
                ScaledupProducers = new List<CalcResultScaledupProducer>(),
            };
            var materials = new List<Material>();
            materials.Add(new Material { Code = "AL", Name = "Aluminium" });
            var materialDetails = MaterialMapper.Map(materials);
            CalcResultScaledupProducersBuilder.SetHeaders(producers, materialDetails);
            Assert.AreEqual(18, producers?.ColumnHeaders?.Count());
            Assert.AreEqual(2, producers?.MaterialBreakdownHeaders?.Count());
        }

        [TestMethod]
        public void CalculateScaledupTonnageTest()
        {
            var dictionary = new Dictionary<string, CalcResultScaledupProducerTonnage>();
            dictionary.Add("AL", new CalcResultScaledupProducerTonnage
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
                ScaledupNetReportedTonnage = 10,
            });
            var scaledUpProducer = new CalcResultScaledupProducer
            {
                ProducerId = 2,
                SubsidiaryId = "Sub3",
                ScaledupProducerTonnageByMaterial = dictionary,
            };

            var allPomDataDetails = new List<CalculatorRunPomDataDetail>();
            allPomDataDetails.Add(new CalculatorRunPomDataDetail
            {
                LoadTimeStamp = DateTime.Now,
                SubmissionPeriod = "2024-P1",
                SubmissionPeriodDesc = "desc",
                OrganisationId = 10,
            });
            var materials = new List<Material>();
            materials.Add(new Material { Code = "AL", Name = "Aluminium" });
            var materialDetails = MaterialMapper.Map(materials);
            this.builder = new CalcResultScaledupProducersBuilder(this.dbContext!);
            CalcResultScaledupProducersBuilder.CalculateScaledupTonnage([scaledUpProducer], allPomDataDetails, materialDetails);
            Assert.IsNotNull(scaledUpProducer.ScaledupProducerTonnageByMaterial);
            var scaledUpTonnage = scaledUpProducer.ScaledupProducerTonnageByMaterial["AL"];
            Assert.IsNotNull(scaledUpTonnage);
        }

        [TestMethod]
        public void CalculateScaledupTonnageTestForGlass()
        {
            var dictionary = new Dictionary<string, CalcResultScaledupProducerTonnage>();
            dictionary.Add("GL", new CalcResultScaledupProducerTonnage
            {
                ReportedHouseholdPackagingWasteTonnage = 10,
                ReportedPublicBinTonnage = 10,
                HouseholdDrinksContainersTonnageGlass = 10,
                TotalReportedTonnage = 10,
                ReportedSelfManagedConsumerWasteTonnage = 10,
                NetReportedTonnage = 10,
                ScaledupReportedHouseholdPackagingWasteTonnage = 10,
                ScaledupReportedPublicBinTonnage = 10,
                ScaledupHouseholdDrinksContainersTonnageGlass = 10,
                ScaledupTotalReportedTonnage = 10,
                ScaledupReportedSelfManagedConsumerWasteTonnage = 10,
                ScaledupNetReportedTonnage = 10,
            });
            var scaledUpProducer = new CalcResultScaledupProducer
            {
                ProducerId = 2,
                SubsidiaryId = "Sub3",
                ScaledupProducerTonnageByMaterial = dictionary,
            };

            var allPomDataDetails = new List<CalculatorRunPomDataDetail>();
            allPomDataDetails.Add(new CalculatorRunPomDataDetail
            {
                LoadTimeStamp = DateTime.Now,
                SubmissionPeriod = "2024-P1",
                SubmissionPeriodDesc = "desc",
                OrganisationId = 10,
            });
            var materials = new List<Material>();
            materials.Add(new Material { Code = "GL", Name = "Glass" });
            var materialDetails = MaterialMapper.Map(materials);
            this.builder = new CalcResultScaledupProducersBuilder(this.dbContext);
            CalcResultScaledupProducersBuilder.CalculateScaledupTonnage([scaledUpProducer], allPomDataDetails, materialDetails);

            var scaledUpTonnage = scaledUpProducer.ScaledupProducerTonnageByMaterial["GL"];
            Assert.IsNotNull(scaledUpTonnage);
            Assert.IsNotNull(scaledUpTonnage.HouseholdDrinksContainersTonnageGlass);
            Assert.IsNotNull(scaledUpTonnage.ScaledupHouseholdDrinksContainersTonnageGlass);
        }
    }
}