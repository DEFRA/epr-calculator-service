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
        private readonly CalculatorRunPomDataMaster calcRunPomDataMaster;
        private readonly CalculatorRunOrganisationDataMaster calcRunOrganisationDataMaster;
        private readonly int pcId;
        private readonly int runId = 1;

        private CalcResultScaledupProducersBuilder builder;

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
            dbContext.Database.EnsureCreated();
            this.builder = new CalcResultScaledupProducersBuilder(dbContext);

            this.calcRunPomDataMaster = new CalculatorRunPomDataMaster
            {
                Id = 1,
                RelativeYear = new RelativeYear(2024),
                EffectiveFrom = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Test User",
            };
            dbContext.CalculatorRunPomDataMaster.Add(calcRunPomDataMaster);

            this.calcRunOrganisationDataMaster = new CalculatorRunOrganisationDataMaster
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

            this.pcId = 4;
            dbContext.Material.AddRange(
                new Material { Id = 1, Code = "AL", Name = "Aluminium", Description = "Aluminium" },
                new Material { Id = 2, Code = "FC", Name = "Fibre composite", Description = "Fibre composite" },
                new Material { Id = 3, Code = "GL", Name = "Glass", Description = "Glass" },
                new Material { Id = pcId, Code = "PC", Name = "Paper or card", Description = "Paper or card" },
                new Material { Id = 5, Code = "PL", Name = "Plastic", Description = "Plastic" },
                new Material { Id = 6, Code = "ST", Name = "Steel", Description = "Steel" },
                new Material { Id = 7, Code = "WD", Name = "Wood", Description = "Wood" },
                new Material { Id = 8, Code = "OT", Name = "Other materials", Description = "Other materials" }
            );

            dbContext.SaveChanges();
        }

        [TestCleanup]
        public void Teardown()
        {
            dbContext.Database.EnsureDeleted();
            dbContext.Dispose();
        }

        private List<ProducerDetail> PrepareNonScaledUpProducer()
        {
            var result = new List<ProducerDetail>();

            var producerDetail = new ProducerDetail
            {
                Id = 1,
                CalculatorRunId = runId,
                ProducerId = 11,
                SubsidiaryId = "Subsidary 1",
                ProducerName = "Producer Name",
            };

            // TODO ProducerReportedMaterials is mutable, but cannot be defined on creation!?
            // It's also an ICollection, so can't add multiple (AddRAnge)
            producerDetail.ProducerReportedMaterials.Add(
                new ProducerReportedMaterial
                {
                  MaterialId = pcId,
                  PackagingType = "HH",
                  SubmissionPeriod = "2025-H1"
                }
            );
            producerDetail.ProducerReportedMaterials.Add(
                new ProducerReportedMaterial
                {
                  MaterialId = pcId,
                  PackagingType = "HDC",
                  SubmissionPeriod = "2025-H1"
                }
            );
            producerDetail.ProducerReportedMaterials.Add(
                new ProducerReportedMaterial
                {
                  MaterialId = pcId,
                  PackagingType = "HH",
                  SubmissionPeriod = "2025-H2"
                }
            );
            producerDetail.ProducerReportedMaterials.Add(
                new ProducerReportedMaterial
                {
                  MaterialId = pcId,
                  PackagingType = "HDC",
                  SubmissionPeriod = "2025-H2"
                }
            );

            result.Add(producerDetail);
            dbContext.ProducerDetail.Add(producerDetail);

            foreach (var subPeriod in new[] { "2025-H1", "2025-H2"}) {
                this.dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                {
                    PackagingType = "HH",
                    SubmissionPeriod = subPeriod,
                    ProducerDetail = producerDetail,
                });
                this.dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                {
                    PackagingType = "HDC",
                    SubmissionPeriod = subPeriod,
                    ProducerDetail = producerDetail,
                });
            }

            dbContext.CalculatorRunOrganisationDataDetails.AddRange(
                new CalculatorRunOrganisationDataDetail
                {
                    Id = 1,
                    OrganisationId = 11,
                    SubsidiaryId = null,
                    OrganisationName = "Allied Packaging",
                    LoadTimeStamp = DateTime.UtcNow,
                    CalculatorRunOrganisationDataMaster = calcRunOrganisationDataMaster,
                    ObligationStatus = ObligationStates.Obligated
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


            foreach (var subPeriod in new[] { "2025-H1", "2025-H2"}) {
                this.dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                {
                    PackagingType = "HH",
                    SubmissionPeriod = subPeriod,
                    ProducerDetail = producerDetail1,
                });
            }

            dbContext.SaveChanges();

            return result;
        }

        private List<ProducerDetail> PrepareScaledUpProducer()
        {
            dbContext.CalculatorRunOrganisationDataDetails.AddRange(
                new CalculatorRunOrganisationDataDetail
                {
                    Id = 2,
                    OrganisationId = 12,
                    SubsidiaryId = null,
                    OrganisationName = "Allied Packaging 2",
                    LoadTimeStamp = DateTime.UtcNow,
                    CalculatorRunOrganisationDataMaster = calcRunOrganisationDataMaster,
                    ObligationStatus = ObligationStates.Obligated
                }
            );

            dbContext.CalculatorRunPomDataDetails.AddRange(
                new CalculatorRunPomDataDetail
                {
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriod = "2024-P2",
                    SubmissionPeriodDesc = "desc",
                    CalculatorRunPomDataMaster = dbContext.CalculatorRunPomDataMaster.First(),
                    OrganisationId = 12
                },
                new CalculatorRunPomDataDetail
                {
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriod = "2024-P2",
                    SubmissionPeriodDesc = "desc",
                    CalculatorRunPomDataMaster = dbContext.CalculatorRunPomDataMaster.First(),
                    OrganisationId = 12
                }
            );

            var producerDetail = new ProducerDetail
            {
                CalculatorRunId = 1,
                ProducerId = 12,
                SubsidiaryId = null,
                ProducerName = "Producer 12",
            };
            foreach (var subPeriod in new[] { "2024-P2", "2024-P4"}) {
                producerDetail.ProducerReportedMaterials.Add(new ProducerReportedMaterial
                {
                    PackagingType = "HH",
                    ProducerDetail = producerDetail,
                    SubmissionPeriod = subPeriod,
                    MaterialId = 4,
                    PackagingTonnage = 1m,
                });
            }

            dbContext.ProducerDetail.Add(producerDetail);

            var result = new List<ProducerDetail>
            {
                producerDetail
            };

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
                }
            );

            dbContext.SaveChanges();

            return result;
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
            //var nonScaledupProducers = PrepareNonScaledUpProducer();
            var producers = PrepareScaledUpProducer();
            //var producers = nonScaledupProducers.Concat(scaledupProducers).ToList();
            var requestDto = new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2025) };

            // Act
            var result = (await builder.ConstructAsync(requestDto, producers)).Item2;

            // Assert
            Assert.AreEqual(2, result.ScaledupProducers!.Count());
            var tonnage = result.ScaledupProducers!.First(x => x.ProducerId == 12 && !x.IsTotalRow && !x.IsSubtotalRow).ScaledupProducerTonnageByMaterial["PC"];
            Assert.AreEqual(1, tonnage.TotalReportedTonnage);
            Assert.AreEqual(2.999m, tonnage.ScaledupTotalReportedTonnage);
        }

        [TestMethod]
        public async Task Construct_ReturnsModifiedProducerData()
        {
            // Arrange
            var nonScaledupProducers = PrepareNonScaledUpProducer();
            var scaledupProducers = PrepareScaledUpProducer();
            var producers = nonScaledupProducers.Concat(scaledupProducers).ToList();
            var requestDto = new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2025) };

            // Act
            var updatedProducers = (await builder.ConstructAsync(requestDto, producers)).Item1;

            // Assert
            Assert.AreEqual(producers.Count(), updatedProducers.Count());

            foreach (var producer in updatedProducers)
            {
                if (producer.ProducerId == 12 && producer.SubsidiaryId == null)
                {
                    var reportedAlHH = producer.ProducerReportedMaterials.First(rm => rm.MaterialId == 4 && rm.PackagingType == "HH" && rm.SubmissionPeriod == "2024-P2");
                    Assert.AreEqual(2.999m, reportedAlHH.PackagingTonnage);
                }
                else
                {
                    var expectedProducer = producers.First(p => p.ProducerId == producer.ProducerId && p.SubsidiaryId == producer.SubsidiaryId);
                    Assert.AreEqual(expectedProducer, producer);
                }
            }
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
            var producers = PrepareNonScaledUpProducer();
            var requestDto = new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2025) };

            // Act
            var result = (await builder.ConstructAsync(requestDto, producers)).Item2;

            // Assert
            Assert.AreEqual(0, result.ScaledupProducers?.Count());
        }

        [TestMethod]
        public async Task GetScaledUpOrganisations_Test()
        {
            PrepareScaledUpProducer();
            var result = await builder.GetScaledUpOrganisationsAsync(runId);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.IsNotNull(result.First(p => p.OrganisationId == 12));
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
        public async Task GetScaledUpProducersAsyncTest()
        {
            builder = new CalcResultScaledupProducersBuilder(dbContext!);
            var result = await builder.GetScaledUpProducersAsync(1, new List<int> { 1, 2 });

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetTonnagesTest()
        {
            var alId = 1;
            var materials = new List<Material>();
            materials.Add(new Material { Id = alId, Code = "AL", Name = "Aluminium" });
            var materialDetails = MaterialMapper.Map(materials);
            var reportedMaterials = new List<ProducerReportedMaterial>{
                new ProducerReportedMaterial
                {
                    SubmissionPeriod = "2024-P2",
                    PackagingType = "HH",
                    MaterialId = alId,
                    PackagingTonnage = 0.1m
                }
            };
            var tonnage = CalcResultScaledupProducersBuilder.GetTonnages(reportedMaterials, materialDetails, 2);
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
            var producerId = 2;
            var subsidiaryId = "Sub3";
            var submissionPeriod = "2024-P1";

            var scaledUpProducer = new CalcResultScaledupProducer
            {
                ProducerId = producerId,
                SubsidiaryId = subsidiaryId,
                SubmissionPeriodCode = submissionPeriod,
                ScaledupProducerTonnageByMaterial = new Dictionary<string, CalcResultScaledupProducerTonnage>(),
            };

            List<MaterialDetail> materials = new List<MaterialDetail>();
            var alId = 1;
            var glassMaterial = new MaterialDetail { Id = alId, Code = MaterialCodes.Aluminium, Name = "Aluminium", Description = ""};
            materials.Add(glassMaterial);

            var producer = new ProducerDetail {
                ProducerId = producerId,
                SubsidiaryId = subsidiaryId
            };
            producer.ProducerReportedMaterials.Add(
                new ProducerReportedMaterial
                {
                    MaterialId = alId,
                    PackagingType = "HH",
                    PackagingTonnage = 1.2m,
                    SubmissionPeriod = submissionPeriod
                }
            );
            producer.ProducerReportedMaterials.Add(
                new ProducerReportedMaterial
                {
                    MaterialId = alId,
                    PackagingType = "PB",
                    PackagingTonnage = 0.4m,
                    SubmissionPeriod = submissionPeriod
                }
            );
            var producerData = new List<ProducerDetail>
            {
                producer
            };

            builder = new CalcResultScaledupProducersBuilder(dbContext);
            CalcResultScaledupProducersBuilder.CalculateScaledupTonnage([scaledUpProducer], producerData, materials);

            Assert.IsNotNull(scaledUpProducer.ScaledupProducerTonnageByMaterial);
            var scaledUpTonnage = scaledUpProducer.ScaledupProducerTonnageByMaterial["AL"];
            Assert.IsNotNull(scaledUpTonnage);
            Assert.AreEqual(1.2m, scaledUpTonnage.ReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual(0.4m, scaledUpTonnage.ReportedPublicBinTonnage);
            Assert.AreEqual(1.6m, scaledUpTonnage.TotalReportedTonnage);
            Assert.AreEqual(0.0m, scaledUpTonnage.ReportedSelfManagedConsumerWasteTonnage);
            Assert.AreEqual(1.6m, scaledUpTonnage.NetReportedTonnage);
            Assert.AreEqual(0.0m, scaledUpTonnage.ScaledupReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual(0.0m, scaledUpTonnage.ScaledupReportedPublicBinTonnage);
            Assert.AreEqual(0.0m, scaledUpTonnage.ScaledupTotalReportedTonnage);
            Assert.AreEqual(0.0m, scaledUpTonnage.ScaledupReportedSelfManagedConsumerWasteTonnage);
            Assert.AreEqual(0.0m, scaledUpTonnage.ScaledupNetReportedTonnage);
            Assert.AreEqual(0.0m, scaledUpTonnage.HouseholdDrinksContainersTonnageGlass);
            Assert.AreEqual(0.0m, scaledUpTonnage.ScaledupHouseholdDrinksContainersTonnageGlass);
        }

        [TestMethod]
        public void CalculateScaledupTonnageTestForGlass()
        {
            var producerId = 2;
            var subsidiaryId = "Sub3";
            var submissionPeriod = "2024-P1";

            var scaledUpProducer = new CalcResultScaledupProducer
            {
                ProducerId = producerId,
                SubsidiaryId = subsidiaryId,
                SubmissionPeriodCode = submissionPeriod,
                ScaledupProducerTonnageByMaterial = new Dictionary<string, CalcResultScaledupProducerTonnage>()
            };

            List<MaterialDetail> materials = new List<MaterialDetail>();
            var glassId = 1;
            var glassMaterial = new MaterialDetail { Id = glassId, Code = MaterialCodes.Glass, Name = "Glass", Description = ""};
            materials.Add(glassMaterial);

            var producer = new ProducerDetail{
                ProducerId = producerId,
                SubsidiaryId = subsidiaryId
            };
            producer.ProducerReportedMaterials.Add(
                new ProducerReportedMaterial
                {
                    MaterialId = glassId,
                    PackagingType = "HH",
                    PackagingTonnage = 0.1m,
                    SubmissionPeriod = submissionPeriod
                }
            );
            producer.ProducerReportedMaterials.Add(
                new ProducerReportedMaterial
                {
                    MaterialId = glassId,
                    PackagingType = "HDC",
                    PackagingTonnage = 0.03m,
                    SubmissionPeriod = submissionPeriod
                }
            );

            var producerData = new List<ProducerDetail>
            {
                producer
            };

            builder = new CalcResultScaledupProducersBuilder(dbContext);
            CalcResultScaledupProducersBuilder.CalculateScaledupTonnage([scaledUpProducer], producerData, materials);

            var scaledUpTonnage = scaledUpProducer.ScaledupProducerTonnageByMaterial["GL"];
            Assert.IsNotNull(scaledUpTonnage);
            Assert.AreEqual(0.1m, scaledUpTonnage.ReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual(0.0m, scaledUpTonnage.ReportedPublicBinTonnage);
            Assert.AreEqual(0.13m, scaledUpTonnage.TotalReportedTonnage);
            Assert.AreEqual(0.0m, scaledUpTonnage.ReportedSelfManagedConsumerWasteTonnage);
            Assert.AreEqual(0.13m, scaledUpTonnage.NetReportedTonnage);
            Assert.AreEqual(0.0m, scaledUpTonnage.ScaledupReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual(0.0m, scaledUpTonnage.ScaledupReportedPublicBinTonnage);
            Assert.AreEqual(0.0m, scaledUpTonnage.ScaledupTotalReportedTonnage);
            Assert.AreEqual(0.0m, scaledUpTonnage.ScaledupReportedSelfManagedConsumerWasteTonnage);
            Assert.AreEqual(0.0m, scaledUpTonnage.ScaledupNetReportedTonnage);
            Assert.AreEqual(0.03m, scaledUpTonnage.HouseholdDrinksContainersTonnageGlass);
            Assert.AreEqual(0.0m, scaledUpTonnage.ScaledupHouseholdDrinksContainersTonnageGlass);
        }

        [TestMethod]
        public void GetTonnages_ShouldCalculateCorrectlyForGlass()
        {
            List<CalculatorRunPomDataDetail> pomData = new List<CalculatorRunPomDataDetail>();

            List<MaterialDetail> materials = new List<MaterialDetail>();
            var glassId = 1;
            var glassMaterial = new MaterialDetail { Id = glassId, Code = MaterialCodes.Glass, Name = "Glass", Description = ""};
            materials.Add(glassMaterial);

            var reportedMaterials = new List<ProducerReportedMaterial>{
                new ProducerReportedMaterial
                {
                    MaterialId = glassId,
                    PackagingType = "HH",
                    PackagingTonnage = 0.1m,
                    SubmissionPeriod = "2024-P1"
                },
                new ProducerReportedMaterial
                {
                    MaterialId = glassId,
                    PackagingType = "HDC",
                    PackagingTonnage = 0.03m,
                    SubmissionPeriod = "2024-P1"
                }
            };

            // Act
            var result = CalcResultScaledupProducersBuilder.GetTonnages(reportedMaterials, materials, 1);

            // Assert
            Assert.IsTrue(result.ContainsKey(MaterialCodes.Glass));
            var glassTonnage = result[MaterialCodes.Glass];

            Assert.AreEqual(0.1m, glassTonnage.ReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual(0, glassTonnage.ReportedPublicBinTonnage);
            Assert.AreEqual(0.03m, glassTonnage.HouseholdDrinksContainersTonnageGlass);
            Assert.AreEqual(0.13m, glassTonnage.TotalReportedTonnage);
            Assert.AreEqual(0.13m, glassTonnage.NetReportedTonnage);
            Assert.AreEqual(0.1m, glassTonnage.ScaledupReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual(0, glassTonnage.ScaledupReportedPublicBinTonnage);
            Assert.AreEqual(0.03m, glassTonnage.ScaledupHouseholdDrinksContainersTonnageGlass);
            Assert.AreEqual(0.13m, glassTonnage.ScaledupTotalReportedTonnage);
            Assert.AreEqual(0.13m, glassTonnage.ScaledupNetReportedTonnage);
        }
    }
}
