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
using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.UnitTests.Builder
{
    [TestClass]
    public class CalcResultPartialObligationBuilderTest
    {
        private readonly ApplicationDBContext dbContext;
        private readonly int runId = 2;
        private readonly int alId = 1;
        private CalcResultPartialObligationBuilder builder;

        private List<ProducerDetail> PrepareData()
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

            var producerDetail2 = new ProducerDetail
            {
                Id = 2,
                CalculatorRunId = 1,
                ProducerId = 22,
                SubsidiaryId = null,
                ProducerName = "Partial Packaging",
            };

            var alm = new Material { Id = alId, Code = "AL", Name = "Aluminium", Description = "Aluminium" };

            foreach(var subPeriod in new[] { "2024-P1", "2024-P4"})
            {
                producerDetail.ProducerReportedMaterials.Add(
                    new ProducerReportedMaterial
                    {
                        PackagingType = "HH",
                        MaterialId = alm.Id,
                        Material = alm,
                        SubmissionPeriod = subPeriod,
                        ProducerDetail = producerDetail,
                    }
                );
                producerDetail.ProducerReportedMaterials.Add(
                    new ProducerReportedMaterial
                    {
                        PackagingType = "HH",
                        MaterialId = alm.Id,
                        Material = alm,
                        SubmissionPeriod = subPeriod,
                        ProducerDetail = producerDetail,
                    }
                );
                producerDetail.ProducerReportedMaterials.Add(
                    new ProducerReportedMaterial
                    {
                        PackagingType = "HDC",
                        MaterialId = alm.Id,
                        Material = alm,
                        SubmissionPeriod = subPeriod,
                        ProducerDetail = producerDetail,
                    }
                );
                producerDetail2.ProducerReportedMaterials.Add(
                    new ProducerReportedMaterial
                    {
                        PackagingType = "HH",
                        MaterialId = alm.Id,
                        Material = alm,
                        PackagingTonnage = 50,
                        SubmissionPeriod = subPeriod,
                        ProducerDetail = producerDetail2,
                    }
                );
                producerDetail2.ProducerReportedMaterials.Add(
                    new ProducerReportedMaterial
                    {
                        PackagingType = "CW",
                        MaterialId = alm.Id,
                        Material = alm,
                        PackagingTonnage = 10,
                        SubmissionPeriod = subPeriod,
                        ProducerDetail = producerDetail2,
                    }
                );
            }

            //Run 2
            var calcRunOrganisationDataMaster2 = new CalculatorRunOrganisationDataMaster
            {
                Id = 22,
                RelativeYear = new RelativeYear(2025),
                EffectiveFrom = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Test User",
            };
            dbContext.CalculatorRunOrganisationDataMaster.Add(calcRunOrganisationDataMaster2);

            dbContext.CalculatorRuns.Add(new CalculatorRun
            {
                Id = runId,
                RelativeYear = new RelativeYear(2025),
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
                ProducerId = 33,
                SubsidiaryId = null,
                ProducerName = "Allied Packaging",
            };

            var producerDetail4 = new ProducerDetail
            {
                Id = 4,
                CalculatorRunId = runId,
                ProducerId = 44,
                SubsidiaryId = null,
                ProducerName = "Partial Packaging",
            };

            foreach(var subPeriod in new[] { "2024-P1", "2024-P4"})
            {
                producerDetail3.ProducerReportedMaterials.Add(
                    new ProducerReportedMaterial
                    {
                        PackagingType = "HH",
                        MaterialId = alm.Id,
                        Material = alm,
                        SubmissionPeriod = subPeriod,
                        ProducerDetail = producerDetail3,
                    }
                );
                producerDetail3.ProducerReportedMaterials.Add(
                    new ProducerReportedMaterial
                    {
                        PackagingType = "HDC",
                        MaterialId = alm.Id,
                        Material = alm,
                        SubmissionPeriod = subPeriod,
                        ProducerDetail = producerDetail3,
                    }
                );
                producerDetail4.ProducerReportedMaterials.Add(
                    new ProducerReportedMaterial
                    {
                        PackagingType = "HH",
                        MaterialId = alm.Id,
                        Material = alm,
                        PackagingTonnage = 50,
                        SubmissionPeriod = subPeriod,
                        ProducerDetail = producerDetail4,
                    }
                );
                producerDetail4.ProducerReportedMaterials.Add(
                    new ProducerReportedMaterial
                    {
                        PackagingType = "CW",
                        MaterialId = alm.Id,
                        Material = alm,
                        PackagingTonnage = 10,
                        SubmissionPeriod = subPeriod,
                        ProducerDetail = producerDetail4,
                    }
                );
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

            dbContext.ProducerDetail.AddRange(producerDetail, producerDetail2, producerDetail3, producerDetail4);

            dbContext.SaveChanges();

            // read from db to populate ids
            return (dbContext.ProducerDetail).ToList();
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
            var producers = PrepareData();
            var requestDto = new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2025) };

            // Act
            var result = await builder.ConstructAsync(requestDto, producers);

            // Assert
            Assert.AreEqual(1, result.Item2.PartialObligations!.Count());
            var parOrg = result.Item2.PartialObligations!.First();
            Assert.AreEqual(22, parOrg.ProducerId);
            Assert.IsNull(parOrg.SubsidiaryId);
            Assert.AreEqual("Partial Packaging", parOrg.ProducerName);
            Assert.AreEqual(CommonConstants.LevelOne.ToString(), parOrg.Level);
            Assert.AreEqual("15/07/2025", parOrg.JoiningDate);
            Assert.AreEqual(183, parOrg.DaysObligated);
            Assert.AreEqual(365, parOrg.DaysInSubmissionYear);
            Assert.AreEqual(0.5013698630136986301369863014m, parOrg.ObligatedFactor);
            Assert.AreEqual("50.14%", parOrg.ObligatedPercentage);

            var parOrgMats = parOrg.PartialObligationTonnageByMaterial;
            Assert.AreEqual(8, parOrgMats.Count());
            var aluResult = parOrgMats.Where(mat => mat.Key == MaterialCodes.Aluminium).First();
            Assert.AreEqual(100, aluResult.Value.ReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual(0, aluResult.Value.ReportedPublicBinTonnage);
            Assert.AreEqual(20, aluResult.Value.ReportedSelfManagedConsumerWasteTonnage);
            Assert.AreEqual(80, aluResult.Value.NetReportedTonnage);
            Assert.AreEqual(100, aluResult.Value.TotalReportedTonnage);
            Assert.AreEqual(50.137m, aluResult.Value.PartialReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual(0, aluResult.Value.PartialReportedPublicBinTonnage);
            Assert.AreEqual(10.027m, aluResult.Value.PartialReportedSelfManagedConsumerWasteTonnage);
            Assert.AreEqual(40.110m, aluResult.Value.PartialNetReportedTonnage);
            Assert.AreEqual(50.137m, aluResult.Value.PartialTotalReportedTonnage);
            Assert.IsTrue(parOrgMats.Any(mat =>
                mat.Key == MaterialCodes.Glass &&
                mat.Value.ReportedHouseholdPackagingWasteTonnage == 0 &&
                mat.Value.ReportedPublicBinTonnage == 0 &&
                mat.Value.ReportedSelfManagedConsumerWasteTonnage == 0 &&
                mat.Value.HouseholdDrinksContainersTonnageGlass == 0 &&
                mat.Value.NetReportedTonnage == 0 &&
                mat.Value.TotalReportedTonnage == 0 &&
                mat.Value.PartialReportedHouseholdPackagingWasteTonnage == 0 &&
                mat.Value.PartialReportedPublicBinTonnage == 0 &&
                mat.Value.PartialReportedSelfManagedConsumerWasteTonnage == 0 &&
                mat.Value.PartialNetReportedTonnage == 0 &&
                mat.Value.PartialTotalReportedTonnage == 0 &&
                mat.Value.PartialHouseholdDrinksContainersTonnageGlass == 0
            ));
        }

        [TestMethod]
        public async Task Construct_WhenPartialObligationsExists_producers()
        {
            // Arrange
            var producers = PrepareData();
            var requestDto = new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2025) };

            // Act
            var updatedProducers = (await builder.ConstructAsync(requestDto, producers)).Item1;

            // Assert
            Assert.AreEqual(producers.Count(), updatedProducers.Count());

            foreach (var producer in updatedProducers)
            {
                if (producer.ProducerId == 22 && producer.SubsidiaryId == null)
                {
                    var reportedAlHH = producer.ProducerReportedMaterials.First(rm => rm.MaterialId == alId && rm.PackagingType == "HH" && rm.SubmissionPeriod == "2024-P1");
                    Assert.AreEqual(25.068m, reportedAlHH.PackagingTonnage);

                    var reportedAlCW = producer.ProducerReportedMaterials.First(rm => rm.MaterialId == alId && rm.PackagingType == "CW" && rm.SubmissionPeriod == "2024-P1");
                    Assert.AreEqual(5.014m, reportedAlCW.PackagingTonnage);
                }
                else if (producer.ProducerId == 22 && producer.SubsidiaryId == null)
                {
                    var reportedAl = producer.ProducerReportedMaterials.First(rm => rm.MaterialId == alId && rm.PackagingType == "HH" && rm.SubmissionPeriod == "2024-P4");
                    Assert.AreEqual(25.068m, reportedAl.PackagingTonnage);

                    var reportedAlCW = producer.ProducerReportedMaterials.First(rm => rm.MaterialId == alId && rm.PackagingType == "CW" && rm.SubmissionPeriod == "2024-P4");
                    Assert.AreEqual(5.014m, reportedAlCW.PackagingTonnage);
                }
                else
                {
                    var expectedProducer = producers.First(p => p.ProducerId == producer.ProducerId && p.SubsidiaryId == producer.SubsidiaryId);
                    Assert.AreEqual(expectedProducer, producer);
                }
            }
        }
    }
}
