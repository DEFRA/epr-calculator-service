using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Builder;
using EPR.Calculator.Service.Function.Builder.ScaledupProducers;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Mappers;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EPR.Calculator.Service.Function.UnitTests.Builder
{
    using static CalcRunLaDisposalCostBuilderTests;

    [TestClass]
    public class CalcResultScaledupProducersBuilderTest
    {
        private readonly ApplicationDBContext dbContext;
        private readonly int runId = 1;
        private CalcResultScaledupProducersBuilder builder;

        private readonly Guid submitterId = Guid.NewGuid();
        private readonly Guid submitterId2 = Guid.NewGuid();

        private void PrepareNonScaledUpProducer()
        {
            var producerDetail = new ProducerDetail
            {
                Id = 1,
                CalculatorRunId = runId,
                ProducerId = 11,
                SubsidiaryId = "Subsidary 1",
                ProducerName = "Producer Name",
            };
            dbContext.ProducerDetail.Add(producerDetail);

            dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
            {
                Id = 1,
                PackagingType = "HH",
                ProducerDetail = producerDetail,
            });
            dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
            {
                Id = 2,
                PackagingType = "HDC",
                ProducerDetail = producerDetail,
            });

            var calcRunPomDataMaster = new CalculatorRunPomDataMaster
            {
                Id = 1,
                RelativeYear = new RelativeYear(2024),
                EffectiveFrom = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Test User",
            };
            dbContext.CalculatorRunPomDataMaster.Add(calcRunPomDataMaster);

            var calcRunOrganisationDataMaster = new CalculatorRunOrganisationDataMaster
            {
                Id = 11,
                RelativeYear = new RelativeYear(2025),
                EffectiveFrom = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Test User",
            };
            dbContext.CalculatorRunOrganisationDataMaster.Add(calcRunOrganisationDataMaster);

            dbContext.CalculatorRuns.Add(new CalculatorRun
            {
                Id = runId,
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                CalculatorRunOrganisationDataMaster = calcRunOrganisationDataMaster,
                CalculatorRunPomDataMaster = calcRunPomDataMaster,
            });

            dbContext.CalculatorRunOrganisationDataDetails.AddRange(
                new CalculatorRunOrganisationDataDetail
                {
                    Id = 1,
                    OrganisationId = 11,
                    SubsidiaryId = null,
                    SubmitterId  = submitterId,
                    OrganisationName = "Allied Packaging",
                    LoadTimeStamp = DateTime.UtcNow,
                    CalculatorRunOrganisationDataMaster = calcRunOrganisationDataMaster,
                    ObligationStatus = ObligationStates.Obligated
                },
                new CalculatorRunOrganisationDataDetail
                {
                    Id = 2,
                    OrganisationId = 11,
                    SubsidiaryId = null,
                    SubmitterId  = submitterId2,
                    OrganisationName = "Allied Packaging",
                    LoadTimeStamp = DateTime.UtcNow,
                    CalculatorRunOrganisationDataMaster = calcRunOrganisationDataMaster,
                    ObligationStatus = ObligationStates.NotObligated
                }
                );

            dbContext.CalculatorRunPomDataDetails.Add(
                new CalculatorRunPomDataDetail
                {
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriod = "2024-P1",
                    SubmissionPeriodDesc = "desc",
                    CalculatorRunPomDataMaster = calcRunPomDataMaster,
                    OrganisationId = 10,
                });

            dbContext.SubmissionPeriodLookup.Add(
                new SubmissionPeriodLookup
                {
                    DaysInSubmissionPeriod = 0,
                    DaysInWholePeriod = 0,
                    EndDate = DateTime.UtcNow,
                    StartDate = DateTime.UtcNow,
                    ScaleupFactor = 1,
                    SubmissionPeriod = "2024-P1",
                    SubmissionPeriodDesc = string.Empty,
                });

            dbContext.CalculatorRuns.Add(
                new CalculatorRun
                {
                    Id = 2,
                    RelativeYear = new RelativeYear(2024),
                    Name = "Name",
                });

            var producerDetail1 = new ProducerDetail
            {
                Id = 2,
                CalculatorRunId = 2,
                ProducerId = 11,
                ProducerName = "Producer Test",
            };
            dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
            {
                Id = 3,
                PackagingType = "HH",
                ProducerDetail = producerDetail1,
            });

            dbContext.Material.AddRange(
                new Material { Id = 1, Code = "AL", Name = "Aluminium", Description = "Aluminium" },
                new Material { Id = 2, Code = "FC", Name = "Fibre composite", Description = "Fibre composite" },
                new Material { Id = 3, Code = "GL", Name = "Glass", Description = "Glass" },
                new Material { Id = 4, Code = "PC", Name = "Paper or card", Description = "Paper or card" },
                new Material { Id = 5, Code = "PL", Name = "Plastic", Description = "Plastic" },
                new Material { Id = 6, Code = "ST", Name = "Steel", Description = "Steel" },
                new Material { Id = 7, Code = "WD", Name = "Wood", Description = "Wood" },
                new Material { Id = 8, Code = "OT", Name = "Other materials", Description = "Other materials" }
            );

            dbContext.SaveChanges();
        }

        private void PrepareScaledUpProducer()
        {
            dbContext.CalculatorRunPomDataDetails.AddRange(
            new CalculatorRunPomDataDetail
            {
                LoadTimeStamp = DateTime.UtcNow,
                SubmissionPeriod = "2024-P2",
                SubmissionPeriodDesc = "desc",
                CalculatorRunPomDataMaster = dbContext.CalculatorRunPomDataMaster.First(),
                OrganisationId = 11,
                SubmitterId = submitterId,
                PackagingType = "HH",
                PackagingClass = "O1",
                PackagingMaterial = "PC",
                PackagingMaterialWeight = 1000
            },
            new CalculatorRunPomDataDetail
            {
                LoadTimeStamp = DateTime.UtcNow,
                SubmissionPeriod = "2024-P2",
                SubmissionPeriodDesc = "desc",
                CalculatorRunPomDataMaster = dbContext.CalculatorRunPomDataMaster.First(),
                OrganisationId = 11,
                SubmitterId = submitterId2,
                PackagingType = "HH",
                PackagingClass = "O1",
                PackagingMaterial = "PC",
                PackagingMaterialWeight = 1000
            }
            );

            var producerDetail = new ProducerDetail
            {
                CalculatorRunId = 1,
                ProducerId = 11,
                ProducerName = "Producer Test",
            };

            dbContext.ProducerDetail.Add(producerDetail);

            dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
            {
                PackagingType = "HH",
                ProducerDetail = producerDetail,
                MaterialId = 4,
                PackagingTonnage = 1,

            });

            dbContext.SubmissionPeriodLookup.Add(
            new SubmissionPeriodLookup
            {
                DaysInSubmissionPeriod = 0,
                DaysInWholePeriod = 0,
                EndDate = DateTime.UtcNow,
                StartDate = DateTime.UtcNow,
                ScaleupFactor = 2.999M,
                SubmissionPeriod = "2024-P2",
                SubmissionPeriodDesc = string.Empty,
            });

            dbContext.SaveChanges();
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

            dbContext = new ApplicationDBContext(dbContextOptions);
            dbContext.Database.EnsureCreated();
            builder = new CalcResultScaledupProducersBuilder(dbContext);
        }

        [TestCleanup]
        public void Teardown()
        {
            dbContext.Database.EnsureDeleted();
            dbContext.Dispose();
        }

        /// <summary>
        /// Tests that the <see cref="ICalcResultScaledupProducersBuilder.ConstructAsync(CalcResultsRequestDto)"/>
        /// method returns the correct result when scaled up data is present.
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        [TestMethod]
        public async Task Construct_WhenScaledUpDataPresent()
        {
            // Arrange
            PrepareNonScaledUpProducer();
            PrepareScaledUpProducer();
            var requestDto = new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2025) };

            // Act
            var result = await builder.ConstructAsync(requestDto);

            // Assert
            Assert.AreEqual(2, result.ScaledupProducers!.Count());
            var tonnage = result.ScaledupProducers!.First(x => x.ProducerId == 11 && !x.IsTotalRow && !x.IsSubtotalRow).ScaledupProducerTonnageByMaterial["PC"];
            Assert.AreEqual(1, tonnage.TotalReportedTonnage);
            Assert.AreEqual(2.999m, tonnage.ScaledupTotalReportedTonnage);
            
        }

        /// <summary>
        /// Tests that the <see cref="ICalcResultScaledupProducersBuilder.ConstructAsync(CalcResultsRequestDto)"/>
        /// method returns the correct result when scaled up data is present.
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        [TestMethod]
        public async Task Construct_ScaledUpDataPresentForThisRunOnly()
        {
            // Arrange
            PrepareNonScaledUpProducer();
            PrepareScaledUpProducer();
            var requestDto = new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2025) };

            // Act
            var result = await builder.ConstructAsync(requestDto);

            // Assert
            var actualNumberScaledUpProducer = result.ScaledupProducers!.Where(t => !t.IsTotalRow);
            Assert.AreEqual(1, actualNumberScaledUpProducer.Count());
        }

        /// <summary>
        /// Tests that the <see cref="ICalcResultScaledupProducersBuilder.ConstructAsync(CalcResultsRequestDto)"/>
        /// method returns the correct result when scaled up data is not present.
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        [TestMethod]
        public async Task Construct_WhenNoScaledUpDataPresent()
        {
            // Arrange
            PrepareNonScaledUpProducer();
            var requestDto = new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2025) };

            // Act
            var result = await builder.ConstructAsync(requestDto);

            // Assert
            Assert.AreEqual(0, result.ScaledupProducers?.Count());
        }

        [TestMethod]
        public void GetScaledUpOrganisations_Test()
        {
            PrepareNonScaledUpProducer();
            PrepareScaledUpProducer();
            var task = builder.GetScaledUpOrganisationsAsync(runId);
            task.Wait();

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
            var filteredData = producerData.Where(t => !calcResult.CalcResultScaledupProducers.ScaledupProducers.Any(i => i.ProducerId == t.ProducerDetail?.ProducerId)).ToList();

            // Assert
            Assert.AreEqual(1, filteredData.Count);
            Assert.AreEqual(2, filteredData.First().ProducerDetail?.ProducerId);
        }

        [TestMethod]
        public void AddExtraRowsTest()
        {
            builder = new CalcResultScaledupProducersBuilder(dbContext);
            var runProducerMaterialDetails = new List<CalcResultScaledupProducer>();
            runProducerMaterialDetails.AddRange([
                new CalcResultScaledupProducer
                {
                    ProducerId = 1,
                },
                new CalcResultScaledupProducer
                {
                    ProducerId = 1,
                    SubsidiaryId = "Sub1",
                },
                new CalcResultScaledupProducer
                {
                    ProducerId = 1,
                    SubsidiaryId = "Sub2",
                },
                new CalcResultScaledupProducer
                {
                    ProducerId = 2,
                },
                new CalcResultScaledupProducer
                {
                    ProducerId = 2,
                    SubsidiaryId = "Sub3",
                },
                new CalcResultScaledupProducer
                {
                    ProducerId = 2,
                    SubsidiaryId = "Sub4",
                }
            ]);

            var scaledupOrganisations = new List<Organisation>();
            scaledupOrganisations.AddRange([
                new Organisation
                {
                    OrganisationId = 1,
                    OrganisationName = "Allied Packaging",
                },
                new Organisation
                {
                    OrganisationId = 2,
                    OrganisationName = "Beeline Materials",
                },
            ]);

            CalcResultScaledupProducersBuilder.AddExtraRows(runProducerMaterialDetails, scaledupOrganisations);

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
            builder = new CalcResultScaledupProducersBuilder(dbContext);
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
            builder = new CalcResultScaledupProducersBuilder(dbContext);
            var result = await builder.GetProducerReportedMaterialsAsync(1, new List<int> { 1, 2 });

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetScaledupOrganisationDetailsTest()
        {
            builder = new CalcResultScaledupProducersBuilder(dbContext);
            var result = await builder.GetScaledupOrganisationDetails(1, new List<int> { 1, 2 });

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
                LoadTimeStamp = DateTime.UtcNow,
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
            Assert.AreEqual(19, columnHeaders.Count);
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
            Assert.AreEqual(19, producers.ColumnHeaders?.Count());
            Assert.AreEqual(2, producers.MaterialBreakdownHeaders?.Count());
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
                LoadTimeStamp = DateTime.UtcNow,
                SubmissionPeriod = "2024-P1",
                SubmissionPeriodDesc = "desc",
                OrganisationId = 10,
            });
            var materials = new List<Material>();
            materials.Add(new Material { Code = "AL", Name = "Aluminium" });
            var materialDetails = MaterialMapper.Map(materials);
            builder = new CalcResultScaledupProducersBuilder(dbContext);
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
                LoadTimeStamp = DateTime.UtcNow,
                SubmissionPeriod = "2024-P1",
                SubmissionPeriodDesc = "desc",
                OrganisationId = 10,
            });
            var materials = new List<Material>();
            materials.Add(new Material { Code = "GL", Name = "Glass" });
            var materialDetails = MaterialMapper.Map(materials);
            builder = new CalcResultScaledupProducersBuilder(dbContext);
            CalcResultScaledupProducersBuilder.CalculateScaledupTonnage([scaledUpProducer], allPomDataDetails, materialDetails);

            var scaledUpTonnage = scaledUpProducer.ScaledupProducerTonnageByMaterial["GL"];
            Assert.IsNotNull(scaledUpTonnage);
            Assert.IsNotNull(scaledUpTonnage.HouseholdDrinksContainersTonnageGlass);
            Assert.IsNotNull(scaledUpTonnage.ScaledupHouseholdDrinksContainersTonnageGlass);
        }
    }
}