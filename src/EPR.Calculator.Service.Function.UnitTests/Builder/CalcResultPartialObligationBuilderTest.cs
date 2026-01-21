namespace EPR.Calculator.Service.Function.UnitTests.Builder
{
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.Service.Function.Builder.PartialObligations;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Dtos;
    using EPR.Calculator.Service.Function.Mappers;
    using EPR.Calculator.Service.Function.Models;
    using EPR.Calculator.Service.Function.Services;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using static EPR.Calculator.Service.Function.UnitTests.Builder.CalcRunLaDisposalCostBuilderTests;

    [TestClass]
    public class CalcResultPartialObligationBuilderTest
    {
        private readonly ApplicationDBContext dbContext;
        private readonly int runId = 2;
        private CalcResultPartialObligationBuilder builder;

        private void PrepareData()
        {
            var year = new CalculatorRunFinancialYear { Name = "2024-25" };

            //Run 1
            var calcRunOrganisationDataMaster = new CalculatorRunOrganisationDataMaster
            {
                Id = 11,
                CalendarYear = "2024",
                EffectiveFrom = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Test User",
            };
            this.dbContext.CalculatorRunOrganisationDataMaster.Add(calcRunOrganisationDataMaster);

            this.dbContext.CalculatorRuns.Add(new CalculatorRun
            {
                Id = 1,
                Financial_Year = year,
                Name = "Name",
                CalculatorRunOrganisationDataMaster = calcRunOrganisationDataMaster
            });

            this.dbContext.CalculatorRunOrganisationDataDetails.Add(
                new CalculatorRunOrganisationDataDetail
                {
                    Id = 1,
                    OrganisationId = 11,
                    SubsidiaryId = null,
                    OrganisationName = "Allied Packaging",
                    LoadTimeStamp = DateTime.UtcNow,
                    CalculatorRunOrganisationDataMaster = calcRunOrganisationDataMaster,
                    ObligationStatus = ObligationStates.Obligated
                });

            this.dbContext.CalculatorRunOrganisationDataDetails.Add(
                new CalculatorRunOrganisationDataDetail
                {
                    Id = 2,
                    OrganisationId = 22,
                    SubsidiaryId = null,
                    OrganisationName = "Partial packaging",
                    LoadTimeStamp = DateTime.UtcNow,
                    CalculatorRunOrganisationDataMaster = calcRunOrganisationDataMaster,
                    ObligationStatus = ObligationStates.Obligated,
                    DaysObligated = 183,
                    JoinerDate = "15/07/2025"
                });

            var producerDetail = new ProducerDetail
            {
                Id = 1,
                CalculatorRunId = 1,
                ProducerId = 11,
                SubsidiaryId = null,
                ProducerName = "Allied Packaging",
            };
            this.dbContext.ProducerDetail.Add(producerDetail);

            var producerDetail2 = new ProducerDetail
            {
                Id = 2,
                CalculatorRunId = 1,
                ProducerId = 22,
                SubsidiaryId = null,
                ProducerName = "Partial Packaging",
            };
            this.dbContext.ProducerDetail.Add(producerDetail2);

            var alm = new Material { Id = 1, Code = "AL", Name = "Aluminium", Description = "Aluminium" };
            var glass = new Material { Id = 3, Code = "GL", Name = "Glass", Description = "Glass" };

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
            this.dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
            {
                Id = 3,
                PackagingType = "HH",
                Material = alm,
                PackagingTonnage = 100,
                ProducerDetail = producerDetail2,
            });
            this.dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
            {
                Id = 4,
                PackagingType = "CW",
                Material = alm,
                PackagingTonnage = 20,
                ProducerDetail = producerDetail2,
            });
            this.dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
            {
                Id = 9,
                PackagingType = "HDC",
                Material = glass,
                PackagingTonnage = 50,
                ProducerDetail = producerDetail2,
            });

            //Run 2
            var calcRunOrganisationDataMaster2 = new CalculatorRunOrganisationDataMaster
            {
                Id = 22,
                CalendarYear = "2024",
                EffectiveFrom = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Test User",
            };
            this.dbContext.CalculatorRunOrganisationDataMaster.Add(calcRunOrganisationDataMaster2);

            this.dbContext.CalculatorRuns.Add(new CalculatorRun
            {
                Id = this.runId,
                Financial_Year = year,
                Name = "Name",
                CalculatorRunOrganisationDataMaster = calcRunOrganisationDataMaster2
            });

            this.dbContext.CalculatorRunOrganisationDataDetails.Add(
                new CalculatorRunOrganisationDataDetail
                {
                    Id = 3,
                    OrganisationId = 11,
                    SubsidiaryId = null,
                    OrganisationName = "Allied Packaging",
                    LoadTimeStamp = DateTime.UtcNow,
                    CalculatorRunOrganisationDataMaster = calcRunOrganisationDataMaster2,
                    ObligationStatus = ObligationStates.Obligated
                });

            this.dbContext.CalculatorRunOrganisationDataDetails.Add(
                new CalculatorRunOrganisationDataDetail
                {
                    Id = 4,
                    OrganisationId = 22,
                    SubsidiaryId = null,
                    OrganisationName = "Partial packaging",
                    LoadTimeStamp = DateTime.UtcNow,
                    CalculatorRunOrganisationDataMaster = calcRunOrganisationDataMaster2,
                    ObligationStatus = ObligationStates.Obligated,
                    DaysObligated = 183,
                    JoinerDate = "15/07/2025"
                });

            var producerDetail3 = new ProducerDetail
            {
                Id = 3,
                CalculatorRunId = this.runId,
                ProducerId = 11,
                SubsidiaryId = null,
                ProducerName = "Allied Packaging",
            };
            this.dbContext.ProducerDetail.Add(producerDetail3);

            var producerDetail4 = new ProducerDetail
            {
                Id = 4,
                CalculatorRunId = this.runId,
                ProducerId = 22,
                SubsidiaryId = null,
                ProducerName = "Partial Packaging",
            };
            this.dbContext.ProducerDetail.Add(producerDetail4);

            this.dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
            {
                Id = 5,
                PackagingType = "HH",
                ProducerDetail = producerDetail3,
            });
            this.dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
            {
                Id = 6,
                PackagingType = "HDC",
                ProducerDetail = producerDetail3,
            });
            this.dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
            {
                Id = 7,
                PackagingType = "HH",
                Material = alm,
                PackagingTonnage = 100,
                ProducerDetail = producerDetail4,
            });
            this.dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
            {
                Id = 8,
                PackagingType = "CW",
                Material = alm,
                PackagingTonnage = 20,
                ProducerDetail = producerDetail4,
            });

            this.dbContext.Material.AddRange(
                alm,
                new Material { Id = 2, Code = "FC", Name = "Fibre composite", Description = "Fibre composite" },
                glass,
                new Material { Id = 4, Code = "PC", Name = "Paper or card", Description = "Paper or card" },
                new Material { Id = 5, Code = "PL", Name = "Plastic", Description = "Plastic" },
                new Material { Id = 6, Code = "ST", Name = "Steel", Description = "Steel" },
                new Material { Id = 7, Code = "WD", Name = "Wood", Description = "Wood" },
                new Material { Id = 8, Code = "OT", Name = "Other materials", Description = "Other materials" }
            );

            this.dbContext.SaveChanges();
        }

        public CalcResultPartialObligationBuilderTest()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseInMemoryDatabase(databaseName: "PayCal")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

            this.dbContext = new ApplicationDBContext(dbContextOptions);
            this.dbContext.Database.EnsureCreated();
            this.builder = new CalcResultPartialObligationBuilder(this.dbContext);
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

        [TestMethod]
        public async Task Construct_WhenPartialObligationsExists()
        {
            // Arrange
            this.PrepareData();
            var requestDto = new CalcResultsRequestDto { RunId = 1 };

            // Act
            var result = await this.builder.ConstructAsync(requestDto, new List<CalcResultScaledupProducer>());

            // Assert
            Assert.AreEqual(1, result.PartialObligations!.Count());
            var parOrg = result.PartialObligations!.First();
            Assert.IsTrue(parOrg.ProducerId == 22);
            Assert.IsTrue(parOrg.SubsidiaryId == null);
            Assert.IsTrue(parOrg.ProducerName == "Partial Packaging");
            Assert.IsTrue(parOrg.Level == CommonConstants.LevelOne.ToString());
            Assert.IsTrue(parOrg.JoiningDate == "15/07/2025");
            Assert.IsTrue(parOrg.DaysObligated == 183);
            Assert.IsTrue(parOrg.DaysInSubmissionYear == 365);
            Assert.IsTrue(parOrg.ObligatedPercentage == "50.14%");


            var parOrgMats = parOrg.PartialObligationTonnageByMaterial!;
            Assert.AreEqual(8, parOrgMats.Count());
            Assert.IsTrue(parOrgMats.Any(mat =>
                mat.Key == MaterialCodes.Aluminium &&
                mat.Value.ReportedHouseholdPackagingWasteTonnage == 100 &&
                mat.Value.ReportedPublicBinTonnage == 0 &&
                mat.Value.ReportedSelfManagedConsumerWasteTonnage == 20 &&
                mat.Value.NetReportedTonnage == 80 &&
                mat.Value.TotalReportedTonnage == 100 &&
                mat.Value.PartialReportedHouseholdPackagingWasteTonnage == 50.137m &&
                mat.Value.PartialReportedPublicBinTonnage == 0 &&
                mat.Value.PartialReportedSelfManagedConsumerWasteTonnage == 10.027m &&
                mat.Value.PartialNetReportedTonnage == 40.110m &&
                mat.Value.PartialTotalReportedTonnage == 50.137m
            ));
            Assert.IsTrue(parOrgMats.Any(mat =>
                mat.Key == MaterialCodes.Glass &&
                mat.Value.ReportedHouseholdPackagingWasteTonnage == 0 &&
                mat.Value.ReportedPublicBinTonnage == 0 &&
                mat.Value.ReportedSelfManagedConsumerWasteTonnage == 0 &&
                mat.Value.HouseholdDrinksContainersTonnageGlass == 50 &&
                mat.Value.NetReportedTonnage == 50 &&
                mat.Value.TotalReportedTonnage == 50 &&
                mat.Value.PartialReportedHouseholdPackagingWasteTonnage == 0 &&
                mat.Value.PartialReportedPublicBinTonnage == 0 &&
                mat.Value.PartialReportedSelfManagedConsumerWasteTonnage == 0 &&
                mat.Value.PartialHouseholdDrinksContainersTonnageGlass == 25.068m &&
                mat.Value.PartialNetReportedTonnage == 25.068m &&
                mat.Value.PartialTotalReportedTonnage == 25.068m
            ));
        }

        [TestMethod]
        public async Task Construct_WhenPartialObligationsExists_WithScaledUpTonnage()
        {
            // Arrange
            this.PrepareData();
            var requestDto = new CalcResultsRequestDto { RunId = 1 };

            var scaledUpProducers = new List<CalcResultScaledupProducer>() {
                new CalcResultScaledupProducer() {
                    ProducerId = 22, 
                    SubsidiaryId = null,
                    SubmissionPeriodCode = "2025-P3",
                    ScaledupProducerTonnageByMaterial = new Dictionary<string, CalcResultScaledupProducerTonnage>{
                        { 
                            MaterialCodes.Aluminium, 
                            new CalcResultScaledupProducerTonnage()
                            {
                                ReportedHouseholdPackagingWasteTonnage = 25,
                                ReportedPublicBinTonnage = 0,
                                ReportedSelfManagedConsumerWasteTonnage = 10,
                                NetReportedTonnage = 15,
                                TotalReportedTonnage = 25,
                                ScaledupReportedHouseholdPackagingWasteTonnage = 75,
                                ScaledupReportedPublicBinTonnage = 0,
                                ScaledupReportedSelfManagedConsumerWasteTonnage = 30,
                                ScaledupNetReportedTonnage = 45,
                                ScaledupTotalReportedTonnage = 75
                            }
                        },
                        { 
                            MaterialCodes.Glass, 
                            new CalcResultScaledupProducerTonnage()
                            {
                                ReportedHouseholdPackagingWasteTonnage = 0,
                                ReportedPublicBinTonnage = 0,
                                ReportedSelfManagedConsumerWasteTonnage = 0,
                                HouseholdDrinksContainersTonnageGlass = 25,
                                NetReportedTonnage = 25,
                                TotalReportedTonnage = 25,
                                ScaledupReportedHouseholdPackagingWasteTonnage = 0,
                                ScaledupReportedPublicBinTonnage = 0,
                                ScaledupReportedSelfManagedConsumerWasteTonnage = 0,
                                ScaledupHouseholdDrinksContainersTonnageGlass = 75,
                                ScaledupNetReportedTonnage = 75,
                                ScaledupTotalReportedTonnage = 75
                            }
                        }
                    }
                },
                new CalcResultScaledupProducer() {
                    ProducerId = 22, 
                    SubsidiaryId = null,
                    SubmissionPeriodCode = "2025-P4",
                    ScaledupProducerTonnageByMaterial = new Dictionary<string, CalcResultScaledupProducerTonnage>{
                        { 
                            MaterialCodes.Aluminium, 
                            new CalcResultScaledupProducerTonnage()
                            {
                                ReportedHouseholdPackagingWasteTonnage = 75,
                                ReportedPublicBinTonnage = 0,
                                ReportedSelfManagedConsumerWasteTonnage = 10,
                                NetReportedTonnage = 65,
                                TotalReportedTonnage = 75,
                                ScaledupReportedHouseholdPackagingWasteTonnage = 225,
                                ScaledupReportedPublicBinTonnage = 0,
                                ScaledupReportedSelfManagedConsumerWasteTonnage = 30,
                                ScaledupNetReportedTonnage = 195,
                                ScaledupTotalReportedTonnage = 225
                            }
                        },
                        { 
                            MaterialCodes.Glass, 
                            new CalcResultScaledupProducerTonnage()
                            {
                                ReportedHouseholdPackagingWasteTonnage = 0,
                                ReportedPublicBinTonnage = 0,
                                ReportedSelfManagedConsumerWasteTonnage = 0,
                                HouseholdDrinksContainersTonnageGlass = 25,
                                NetReportedTonnage = 25,
                                TotalReportedTonnage = 25,
                                ScaledupReportedHouseholdPackagingWasteTonnage = 0,
                                ScaledupReportedPublicBinTonnage = 0,
                                ScaledupReportedSelfManagedConsumerWasteTonnage = 0,
                                ScaledupHouseholdDrinksContainersTonnageGlass = 75,
                                ScaledupNetReportedTonnage = 75,
                                ScaledupTotalReportedTonnage = 75
                            }
                        }
                    }
                }
            };

            // Act
            var result = await this.builder.ConstructAsync(requestDto, scaledUpProducers);

            // Assert
            Assert.AreEqual(1, result.PartialObligations!.Count());
            var parOrg = result.PartialObligations!.First();
            Assert.IsTrue(parOrg.ProducerId == 22);
            Assert.IsTrue(parOrg.SubsidiaryId == null);
            Assert.IsTrue(parOrg.ProducerName == "Partial Packaging");
            Assert.IsTrue(parOrg.Level == CommonConstants.LevelOne.ToString());
            Assert.IsTrue(parOrg.JoiningDate == "15/07/2025");
            Assert.IsTrue(parOrg.DaysObligated == 183);
            Assert.IsTrue(parOrg.DaysInSubmissionYear == 365);
            Assert.IsTrue(parOrg.ObligatedPercentage == "50.14%");

            var parOrgMats = parOrg.PartialObligationTonnageByMaterial!;
            Assert.AreEqual(8, parOrgMats.Count());
            Assert.IsTrue(parOrgMats.Any(mat =>
                mat.Key == MaterialCodes.Aluminium &&
                mat.Value.ReportedHouseholdPackagingWasteTonnage == 100 &&
                mat.Value.ReportedPublicBinTonnage == 0 &&
                mat.Value.ReportedSelfManagedConsumerWasteTonnage == 20 &&
                mat.Value.NetReportedTonnage == 80 &&
                mat.Value.TotalReportedTonnage == 100 &&
                mat.Value.PartialReportedHouseholdPackagingWasteTonnage == 150.411m &&
                mat.Value.PartialReportedPublicBinTonnage == 0 &&
                mat.Value.PartialReportedSelfManagedConsumerWasteTonnage == 30.082m &&
                mat.Value.PartialNetReportedTonnage == 120.329m &&
                mat.Value.PartialTotalReportedTonnage == 150.411m
            ));
            Assert.IsTrue(parOrgMats.Any(mat =>
                mat.Key == MaterialCodes.Glass &&
                mat.Value.ReportedHouseholdPackagingWasteTonnage == 0 &&
                mat.Value.ReportedPublicBinTonnage == 0 &&
                mat.Value.ReportedSelfManagedConsumerWasteTonnage == 0 &&
                mat.Value.HouseholdDrinksContainersTonnageGlass == 50 &&
                mat.Value.NetReportedTonnage == 50 &&
                mat.Value.TotalReportedTonnage == 50 &&
                mat.Value.PartialReportedHouseholdPackagingWasteTonnage == 0 &&
                mat.Value.PartialReportedPublicBinTonnage == 0 &&
                mat.Value.PartialReportedSelfManagedConsumerWasteTonnage == 0 && 
                mat.Value.PartialHouseholdDrinksContainersTonnageGlass == 75.205m &&
                mat.Value.PartialNetReportedTonnage == 75.205m &&
                mat.Value.PartialTotalReportedTonnage == 75.205m
            ));

        }
    }
}