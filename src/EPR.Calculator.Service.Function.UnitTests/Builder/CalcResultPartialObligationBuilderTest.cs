using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Builder.PartialObligations;
using EPR.Calculator.Service.Function.Builder.ProjectedProducers;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EPR.Calculator.Service.Function.UnitTests.Builder
{
    [TestClass]
    public class CalcResultPartialObligationBuilderTest
    {
        private readonly ApplicationDBContext dbContext;
        private readonly int runId = 2;
        private CalcResultPartialObligationBuilder builder;

        private void PrepareData()
        {

            //Run 1
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
                Id = 1,
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                CalculatorRunOrganisationDataMaster = calcRunOrganisationDataMaster
            });

            dbContext.CalculatorRunOrganisationDataDetails.Add(
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

            dbContext.CalculatorRunOrganisationDataDetails.Add(
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
            dbContext.ProducerDetail.Add(producerDetail);

            var producerDetail2 = new ProducerDetail
            {
                Id = 2,
                CalculatorRunId = 1,
                ProducerId = 22,
                SubsidiaryId = null,
                ProducerName = "Partial Packaging",
            };
            dbContext.ProducerDetail.Add(producerDetail2);

            var alm = new Material { Id = 1, Code = "AL", Name = "Aluminium", Description = "Aluminium" };

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
                this.dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                {
                    PackagingType = "HH",
                    Material = alm,
                    PackagingTonnage = 50,
                    SubmissionPeriod = subPeriod,
                    ProducerDetail = producerDetail2,
                });
                this.dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                {
                    PackagingType = "CW",
                    Material = alm,
                    PackagingTonnage = 10,
                    SubmissionPeriod = subPeriod,
                    ProducerDetail = producerDetail2,
                });
            }

            //Run 2
            var calcRunOrganisationDataMaster2 = new CalculatorRunOrganisationDataMaster
            {
                Id = 22,
                RelativeYear = new RelativeYear(2024),
                EffectiveFrom = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Test User",
            };
            dbContext.CalculatorRunOrganisationDataMaster.Add(calcRunOrganisationDataMaster2);

            dbContext.CalculatorRuns.Add(new CalculatorRun
            {
                Id = runId,
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                CalculatorRunOrganisationDataMaster = calcRunOrganisationDataMaster2
            });

            dbContext.CalculatorRunOrganisationDataDetails.Add(
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

            dbContext.CalculatorRunOrganisationDataDetails.Add(
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
                CalculatorRunId = runId,
                ProducerId = 11,
                SubsidiaryId = null,
                ProducerName = "Allied Packaging",
            };
            dbContext.ProducerDetail.Add(producerDetail3);

            var producerDetail4 = new ProducerDetail
            {
                Id = 4,
                CalculatorRunId = runId,
                ProducerId = 22,
                SubsidiaryId = null,
                ProducerName = "Partial Packaging",
            };
            dbContext.ProducerDetail.Add(producerDetail4);

            foreach (var subPeriod in new[] { "2025-H1", "2025-H2"}) {
                this.dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                {
                    PackagingType = "HH",
                    SubmissionPeriod = subPeriod,
                    ProducerDetail = producerDetail3,
                });
                this.dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                {
                    PackagingType = "HDC",
                    SubmissionPeriod = subPeriod,
                    ProducerDetail = producerDetail3,
                });
                this.dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                {
                    PackagingType = "HH",
                    Material = alm,
                    PackagingTonnage = 50,
                    SubmissionPeriod = subPeriod,
                    ProducerDetail = producerDetail4,
                });
                this.dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                {
                    PackagingType = "CW",
                    Material = alm,
                    PackagingTonnage = 10,
                    SubmissionPeriod = subPeriod,
                    ProducerDetail = producerDetail4,
                });
            }

            dbContext.Material.AddRange(
                alm,
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

        public CalcResultPartialObligationBuilderTest()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseInMemoryDatabase(databaseName: "PayCal")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

            dbContext = new ApplicationDBContext(dbContextOptions);
            dbContext.Database.EnsureCreated();
            builder = new CalcResultPartialObligationBuilder(dbContext);
        }

        [TestCleanup]
        public void Teardown()
        {
            dbContext.Database.EnsureDeleted();
            dbContext.Dispose();
        }

        [TestMethod]
        public async Task Construct_WhenPartialObligationsExists()
        {
            // Arrange
            PrepareData();
            var requestDto = new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2025) };

            // Act
            var result = await builder.ConstructAsync(requestDto, new List<ProducerReportedMaterialsForSubmissionPeriod>());

            // Assert
            //Assert.AreEqual(1, result.PartialObligations!.Count());
            //var parOrg = result.PartialObligations!.First();
            //Assert.IsTrue(parOrg.ProducerId == 22);
            //Assert.IsTrue(parOrg.SubsidiaryId == null);
            //Assert.IsTrue(parOrg.ProducerName == "Partial Packaging");
            //Assert.IsTrue(parOrg.Level == CommonConstants.LevelOne.ToString());
            //Assert.IsTrue(parOrg.JoiningDate == "15/07/2025");
            //Assert.IsTrue(parOrg.DaysObligated == 183);
            //Assert.IsTrue(parOrg.DaysInSubmissionYear == 365);
            //Assert.IsTrue(parOrg.ObligatedPercentage == "50.14%");


            //var parOrgMats = parOrg.PartialObligationTonnageByMaterial;
            //Assert.AreEqual(8, parOrgMats.Count());
            //Assert.IsTrue(parOrgMats.Any(mat =>
            //    mat.Key == MaterialCodes.Aluminium &&
            //    mat.Value.ReportedHouseholdPackagingWasteTonnage == 100 &&
            //    mat.Value.ReportedPublicBinTonnage == 0 &&
            //    mat.Value.ReportedSelfManagedConsumerWasteTonnage == 20 &&
            //    mat.Value.NetReportedTonnage == 80 &&
            //    mat.Value.TotalReportedTonnage == 100 &&
            //    mat.Value.PartialReportedHouseholdPackagingWasteTonnage == 50.137m &&
            //    mat.Value.PartialReportedPublicBinTonnage == 0 &&
            //    mat.Value.PartialReportedSelfManagedConsumerWasteTonnage == 10.027m &&
            //    mat.Value.PartialNetReportedTonnage == 40.110m &&
            //    mat.Value.PartialTotalReportedTonnage == 50.137m
            //));
            //Assert.IsTrue(parOrgMats.Any(mat =>
            //    mat.Key == MaterialCodes.Glass &&
            //    mat.Value.ReportedHouseholdPackagingWasteTonnage == 0 &&
            //    mat.Value.ReportedPublicBinTonnage == 0 &&
            //    mat.Value.ReportedSelfManagedConsumerWasteTonnage == 0 &&
            //    mat.Value.HouseholdDrinksContainersTonnageGlass == 0 &&
            //    mat.Value.NetReportedTonnage == 0 &&
            //    mat.Value.TotalReportedTonnage == 0 &&
            //    mat.Value.PartialReportedHouseholdPackagingWasteTonnage == 0 &&
            //    mat.Value.PartialReportedPublicBinTonnage == 0 &&
            //    mat.Value.PartialReportedSelfManagedConsumerWasteTonnage == 0 &&
            //    mat.Value.PartialNetReportedTonnage == 0 &&
            //    mat.Value.PartialTotalReportedTonnage == 0 &&
            //    mat.Value.PartialHouseholdDrinksContainersTonnageGlass == 0
            //));
        }

        [TestMethod]
        public async Task Construct_WhenPartialObligationsExists_WithScaledUpTonnage()
        {
            // Arrange
            PrepareData();
            var requestDto = new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2025) };

            var scaledUpProducers = new List<ProducerReportedMaterialsForSubmissionPeriod> {
                /*new CalcResultScaledupProducer {
                    ProducerId = 22,
                    SubsidiaryId = null,
                    ScaledupProducerTonnageByMaterial = new Dictionary<string, CalcResultScaledupProducerTonnage>{
                        {
                            MaterialCodes.Aluminium,
                            new CalcResultScaledupProducerTonnage
                            {
                                ReportedHouseholdPackagingWasteTonnage = 100,
                                ReportedPublicBinTonnage = 0,
                                ReportedSelfManagedConsumerWasteTonnage = 20,
                                NetReportedTonnage = 80,
                                TotalReportedTonnage = 100,
                                ScaledupReportedHouseholdPackagingWasteTonnage = 300,
                                ScaledupReportedPublicBinTonnage = 0,
                                ScaledupReportedSelfManagedConsumerWasteTonnage = 60,
                                ScaledupNetReportedTonnage = 240,
                                ScaledupTotalReportedTonnage = 300
                            }
                        }
                    }
                }*/
            };

            // Act
            var result = await builder.ConstructAsync(requestDto, scaledUpProducers);

            // Assert
            //Assert.AreEqual(1, result.PartialObligations!.Count());
            //var parOrg = result.PartialObligations!.First();
            //Assert.IsTrue(parOrg.ProducerId == 22);
            //Assert.IsTrue(parOrg.SubsidiaryId == null);
            //Assert.IsTrue(parOrg.ProducerName == "Partial Packaging");
            //Assert.IsTrue(parOrg.Level == CommonConstants.LevelOne.ToString());
            //Assert.IsTrue(parOrg.JoiningDate == "15/07/2025");
            //Assert.IsTrue(parOrg.DaysObligated == 183);
            //Assert.IsTrue(parOrg.DaysInSubmissionYear == 365);
            //Assert.IsTrue(parOrg.ObligatedPercentage == "50.14%");

            //var parOrgMats = parOrg.PartialObligationTonnageByMaterial;
            //Assert.AreEqual(8, parOrgMats.Count());
            //Assert.IsTrue(parOrgMats.Any(mat =>
            //    mat.Key == MaterialCodes.Aluminium &&
            //    mat.Value.ReportedHouseholdPackagingWasteTonnage == 100 &&
            //    mat.Value.ReportedPublicBinTonnage == 0 &&
            //    mat.Value.ReportedSelfManagedConsumerWasteTonnage == 20 &&
            //    mat.Value.NetReportedTonnage == 80 &&
            //    mat.Value.TotalReportedTonnage == 100 &&
            //    mat.Value.PartialReportedHouseholdPackagingWasteTonnage == 150.411m &&
            //    mat.Value.PartialReportedPublicBinTonnage == 0 &&
            //    mat.Value.PartialReportedSelfManagedConsumerWasteTonnage == 30.082m &&
            //    mat.Value.PartialNetReportedTonnage == 120.329m &&
            //    mat.Value.PartialTotalReportedTonnage == 150.411m
            //));
            //Assert.IsTrue(parOrgMats.Any(mat =>
            //    mat.Key == MaterialCodes.Glass &&
            //    mat.Value.ReportedHouseholdPackagingWasteTonnage == 0 &&
            //    mat.Value.ReportedPublicBinTonnage == 0 &&
            //    mat.Value.ReportedSelfManagedConsumerWasteTonnage == 0 &&
            //    mat.Value.HouseholdDrinksContainersTonnageGlass == 0 &&
            //    mat.Value.NetReportedTonnage == 0 &&
            //    mat.Value.TotalReportedTonnage == 0 &&
            //    mat.Value.PartialReportedHouseholdPackagingWasteTonnage == 0 &&
            //    mat.Value.PartialReportedPublicBinTonnage == 0 &&
            //    mat.Value.PartialReportedSelfManagedConsumerWasteTonnage == 0 &&
            //    mat.Value.PartialNetReportedTonnage == 0 &&
            //    mat.Value.PartialTotalReportedTonnage == 0 &&
            //    mat.Value.PartialHouseholdDrinksContainersTonnageGlass == 0
            //));

        }
    }
}
