using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.PartialObligations;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Data;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Extensions;
using EPR.Calculator.Service.Function.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EPR.Calculator.Service.Function.UnitTests.Builder
{
    [TestClass]
    public class CalcResultPartialObligationBuilderTest
    {
        private readonly ApplicationDBContext dbContext;
        private CalcResultPartialObligationBuilder builder;

        private (RunContext, ImmutableList<MaterialDetail>, List<ProducerDetail>) PrepareData()
        {
            var runContext = DummyData.RunContexts.CalculatorRun2025;
            var calcRunOrganisationDataMaster = new CalculatorRunOrganisationDataMaster
            {
                Id = 11,
                RelativeYear = runContext.RelativeYear,
                EffectiveFrom = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Test User",
            };
            dbContext.CalculatorRunOrganisationDataMaster.Add(calcRunOrganisationDataMaster);

            var run = runContext.ToEntity(r => r.CalculatorRunOrganisationDataMasterId = calcRunOrganisationDataMaster.Id);
            dbContext.CalculatorRuns.Add(run);

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
                CalculatorRunId = run.Id,
                ProducerId = 11,
                SubsidiaryId = null,
                ProducerName = "Allied Packaging",
            };

            var producerDetail2 = new ProducerDetail
            {
                Id = 2,
                CalculatorRunId = run.Id,
                ProducerId = 22,
                SubsidiaryId = null,
                ProducerName = "Partial Packaging",
            };

            foreach(var subPeriod in new[] { "2024-P1", "2024-P4"})
            {
                producerDetail.ProducerReportedMaterials.Add(
                    new ProducerReportedMaterial
                    {
                        PackagingType = "HH",
                        MaterialId = 1,
                        SubmissionPeriod = subPeriod,
                        ProducerDetailId = 1,
                    }
                );
                producerDetail.ProducerReportedMaterials.Add(
                    new ProducerReportedMaterial
                    {
                        PackagingType = "HDC",
                        MaterialId = 1,
                        SubmissionPeriod = subPeriod,
                        ProducerDetailId = 1,
                    }
                );
                producerDetail2.ProducerReportedMaterials.Add(
                    new ProducerReportedMaterial
                    {
                        PackagingType = "HH",
                        MaterialId = 1,
                        PackagingTonnage = 50,
                        SubmissionPeriod = subPeriod,
                        ProducerDetailId = 2,
                    }
                );
                producerDetail2.ProducerReportedMaterials.Add(
                    new ProducerReportedMaterial
                    {
                        PackagingType = "CW",
                        MaterialId = 1,
                        PackagingTonnage = 10,
                        SubmissionPeriod = subPeriod,
                        ProducerDetailId = 2
                    }
                );
            }

            var materials = DummyData.Materials;
            dbContext.Material.AddRange(materials.Select(md => new Material
            {
                Id = md.Id,
                Code = md.Code,
                Name = md.Name
            }));

            dbContext.ProducerDetail.AddRange(producerDetail, producerDetail2);

            dbContext.SaveChanges();

            // read from db to populate ids
            return (runContext, materials, (dbContext.ProducerDetail).ToList());
        }

        private (RunContext, ImmutableList<MaterialDetail>, List<ProducerDetail>) PrepareDataWithModulation()
        {
            var runContext = DummyData.RunContexts.CalculatorRun2026;
            var calcRunOrganisationDataMaster = new CalculatorRunOrganisationDataMaster
            {
                Id = 11,
                RelativeYear = runContext.RelativeYear,
                EffectiveFrom = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Test User",
            };
            dbContext.CalculatorRunOrganisationDataMaster.Add(calcRunOrganisationDataMaster);

            var run = runContext.ToEntity(r => r.CalculatorRunOrganisationDataMasterId = calcRunOrganisationDataMaster.Id);
            dbContext.CalculatorRuns.Add(run);

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
                CalculatorRunId = run.Id,
                ProducerId = 11,
                SubsidiaryId = null,
                ProducerName = "Allied Packaging",
            };

            var producerDetail2 = new ProducerDetail
            {
                Id = 2,
                CalculatorRunId = run.Id,
                ProducerId = 22,
                SubsidiaryId = null,
                ProducerName = "Partial Packaging",
            };


            foreach(var subPeriod in new[] { "2025-H1", "2025-H2"})
            {
                producerDetail.ProducerReportedMaterials.Add(
                    new ProducerReportedMaterial
                    {
                        PackagingType = "HH",
                        MaterialId = 1,
                        SubmissionPeriod = subPeriod,
                        ProducerDetailId = 1,
                    }
                );
                producerDetail.ProducerReportedMaterials.Add(
                    new ProducerReportedMaterial
                    {
                        PackagingType = "HDC",
                        MaterialId = 1,
                        SubmissionPeriod = subPeriod,
                        ProducerDetailId = 1,
                    }
                );
                producerDetail2.ProducerReportedMaterials.Add(
                    new ProducerReportedMaterial
                    {
                        PackagingType = "HH",
                        MaterialId = 1,
                        PackagingTonnage = 50,
                        PackagingTonnageRed = 10,
                        PackagingTonnageGreenMedical = 30,
                        PackagingTonnageAmber = 10,
                        SubmissionPeriod = subPeriod,
                        ProducerDetailId = 2,
                    }
                );
                producerDetail2.ProducerReportedMaterials.Add(
                    new ProducerReportedMaterial
                    {
                        PackagingType = "CW",
                        MaterialId = 1,
                        PackagingTonnage = 10,
                        SubmissionPeriod = subPeriod,
                        ProducerDetailId = 2
                    }
                );
            }

            var materials = DummyData.Materials;
            dbContext.Material.AddRange(materials.Select(md => new Material
            {
                Id = md.Id,
                Code = md.Code,
                Name = md.Name
            }));

            dbContext.ProducerDetail.AddRange(producerDetail, producerDetail2);

            dbContext.SaveChanges();

            // read from db to populate ids
            return (runContext, DummyData.Materials, dbContext.ProducerDetail.ToList());
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
            var (runContext, materials, producers) = PrepareData();

            // Act
            var result = await builder.ConstructAsync(runContext, materials, producers);

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
            var aluResult = parOrgMats.Where(mat => mat.Key == MaterialCodes.Aluminium).First().Value;
            Assert.AreEqual(100, aluResult.HouseholdTonnage);
            Assert.IsNull(aluResult.HouseholdRAMTonnage);
            Assert.AreEqual(0, aluResult.PublicBinTonnage);
            Assert.IsNull(aluResult.PublicBinRAMTonnage);
            Assert.AreEqual(20, aluResult.SelfManagedConsumerWasteTonnage);
            Assert.AreEqual(100, aluResult.TotalTonnage);
            Assert.AreEqual(50.137m, aluResult.PartialHouseholdTonnage);
            Assert.IsNull(aluResult.PartialHouseholdRAMTonnage);
            Assert.AreEqual(0, aluResult.PartialPublicBinTonnage);
            Assert.IsNull(aluResult.PartialPublicBinRAMTonnage);
            Assert.AreEqual(10.027m, aluResult.PartialSelfManagedConsumerWasteTonnage);
            Assert.AreEqual(50.137m, aluResult.PartialTotalTonnage);
            var glResult = parOrgMats.Where(mat => mat.Key == MaterialCodes.Glass).First().Value;
            Assert.AreEqual(0, glResult.HouseholdTonnage);
            Assert.IsNull(glResult.HouseholdRAMTonnage);
            Assert.AreEqual(0, glResult.PublicBinTonnage);
            Assert.IsNull(glResult.PublicBinRAMTonnage);
            Assert.AreEqual(0, glResult.SelfManagedConsumerWasteTonnage);
            Assert.AreEqual(0, glResult.HouseholdDrinksContainersTonnage);
            Assert.IsNull(glResult.HouseholdDrinksContainersRAMTonnage);
            Assert.AreEqual(0, glResult.TotalTonnage);
            Assert.AreEqual(0, glResult.PartialHouseholdTonnage);
            Assert.IsNull(glResult.PartialHouseholdRAMTonnage);
            Assert.AreEqual(0, glResult.PartialPublicBinTonnage);
            Assert.IsNull(glResult.PartialPublicBinRAMTonnage);
            Assert.AreEqual(0, glResult.PartialHouseholdDrinksContainersTonnage);
            Assert.IsNull(glResult.PartialHouseholdDrinksContainersRAMTonnage);
            Assert.AreEqual(0, glResult.PartialSelfManagedConsumerWasteTonnage);
            Assert.AreEqual(0, glResult.PartialTotalTonnage);
        }

        [TestMethod]
        public async Task Construct_WhenPartialObligationsExists_producers()
        {
            // Arrange
            var (runContext, materials, producers) = PrepareData();

            // Act
            var updatedProducers = (await builder.ConstructAsync(runContext, materials, producers)).Item1;

            // Assert
            Assert.AreEqual(producers.Count(), updatedProducers.Count());

            foreach (var producer in updatedProducers)
            {
                if (producer.ProducerId == 22 && producer.SubsidiaryId == null)
                {
                    var reportedAlHH = producer.ProducerReportedMaterials.First(rm => rm.MaterialId == 1 && rm.PackagingType == "HH" && rm.SubmissionPeriod == "2024-P1");
                    Assert.AreEqual(25.068m, reportedAlHH.PackagingTonnage);

                    var reportedAlCW = producer.ProducerReportedMaterials.First(rm => rm.MaterialId == 1 && rm.PackagingType == "CW" && rm.SubmissionPeriod == "2024-P1");
                    Assert.AreEqual(5.014m, reportedAlCW.PackagingTonnage);
                }
                else if (producer.ProducerId == 22 && producer.SubsidiaryId == null)
                {
                    var reportedAl = producer.ProducerReportedMaterials.First(rm => rm.MaterialId == 1 && rm.PackagingType == "HH" && rm.SubmissionPeriod == "2024-P4");
                    Assert.AreEqual(25.068m, reportedAl.PackagingTonnage);

                    var reportedAlCW = producer.ProducerReportedMaterials.First(rm => rm.MaterialId == 1 && rm.PackagingType == "CW" && rm.SubmissionPeriod == "2024-P4");
                    Assert.AreEqual(5.014m, reportedAlCW.PackagingTonnage);
                }
                else
                {
                    var expectedProducer = producers.First(p => p.ProducerId == producer.ProducerId && p.SubsidiaryId == producer.SubsidiaryId);
                    Assert.AreEqual(expectedProducer, producer);
                }
            }
        }

        [TestMethod]
        public async Task Construct_WhenPartialObligationsExists_Modulation()
        {
            // Arrange
            var (runContext, materials, producers) = PrepareDataWithModulation();

            // Act
            var result = await builder.ConstructAsync(runContext, materials, producers);

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
            var aluResult = parOrgMats.Where(mat => mat.Key == MaterialCodes.Aluminium).First().Value;
            Assert.AreEqual(100, aluResult.HouseholdTonnage);
            Assert.AreEqual(new RAMTonnage {
                 RedTonnage = 20, AmberTonnage = 20, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 60
            }, aluResult.HouseholdRAMTonnage);
            Assert.AreEqual(0, aluResult.PublicBinTonnage);
            Assert.AreEqual(new RAMTonnage(), aluResult.PublicBinRAMTonnage);
            Assert.AreEqual(20, aluResult.SelfManagedConsumerWasteTonnage);
            Assert.AreEqual(100, aluResult.TotalTonnage);
            Assert.AreEqual(50.136m, aluResult.PartialHouseholdTonnage);
            Assert.AreEqual(new RAMTonnage {
                 RedTonnage = 10.027m, AmberTonnage = 10.027m, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 30.082m
            }, aluResult.PartialHouseholdRAMTonnage);
            Assert.AreEqual(0, aluResult.PartialPublicBinTonnage);
            Assert.AreEqual(new RAMTonnage(), aluResult.PartialPublicBinRAMTonnage);
            Assert.AreEqual(10.027m, aluResult.PartialSelfManagedConsumerWasteTonnage);
            Assert.AreEqual(50.136m, aluResult.PartialTotalTonnage);
            var glResult = parOrgMats.Where(mat => mat.Key == MaterialCodes.Glass).First().Value;
            Assert.AreEqual(0, glResult.HouseholdTonnage);
            Assert.AreEqual(new RAMTonnage(), glResult.HouseholdRAMTonnage);
            Assert.AreEqual(0, glResult.PublicBinTonnage);
            Assert.AreEqual(new RAMTonnage(), glResult.PublicBinRAMTonnage);
            Assert.AreEqual(0, glResult.SelfManagedConsumerWasteTonnage);
            Assert.AreEqual(0, glResult.HouseholdDrinksContainersTonnage);
            Assert.AreEqual(new RAMTonnage(), glResult.HouseholdDrinksContainersRAMTonnage);
            Assert.AreEqual(0, glResult.TotalTonnage);
            Assert.AreEqual(0, glResult.PartialHouseholdTonnage);
            Assert.AreEqual(new RAMTonnage(), glResult.PartialHouseholdRAMTonnage);
            Assert.AreEqual(0, glResult.PartialPublicBinTonnage);
            Assert.AreEqual(new RAMTonnage(), glResult.PartialPublicBinRAMTonnage);
            Assert.AreEqual(0, glResult.PartialHouseholdDrinksContainersTonnage);
            Assert.AreEqual(new RAMTonnage(), glResult.PartialHouseholdDrinksContainersRAMTonnage);
            Assert.AreEqual(0, glResult.PartialSelfManagedConsumerWasteTonnage);
            Assert.AreEqual(0, glResult.PartialTotalTonnage);
        }

        [TestMethod]
        public async Task Construct_WhenPartialObligationsExists_producers_Modulation()
        {
            // Arrange
            var (runContext, materialDetails, producers) = PrepareDataWithModulation();

            // Act
            var updatedProducers = (await builder.ConstructAsync(runContext, materialDetails, producers)).Item1;

            // Assert
            Assert.AreEqual(producers.Count(), updatedProducers.Count());

            foreach (var producer in updatedProducers)
            {
                if (producer.ProducerId == 22 && producer.SubsidiaryId == null)
                {
                    var reportedAlHH = producer.ProducerReportedMaterials.First(rm => rm.MaterialId == 1 && rm.PackagingType == "HH" && rm.SubmissionPeriod == "2025-H1");
                    Assert.AreEqual(25.069m, reportedAlHH.PackagingTonnage);
                    Assert.AreEqual(5.014m, reportedAlHH.PackagingTonnageRed);
                    Assert.AreEqual(5.014m, reportedAlHH.PackagingTonnageAmber);
                    Assert.AreEqual(0, reportedAlHH.PackagingTonnageGreen);
                    Assert.AreEqual(0, reportedAlHH.PackagingTonnageRedMedical);
                    Assert.AreEqual(0, reportedAlHH.PackagingTonnageAmberMedical);
                    Assert.AreEqual(15.041m, reportedAlHH.PackagingTonnageGreenMedical);

                    var reportedAlCW = producer.ProducerReportedMaterials.First(rm => rm.MaterialId == 1 && rm.PackagingType == "CW" && rm.SubmissionPeriod == "2025-H1");
                    Assert.AreEqual(5.014m, reportedAlCW.PackagingTonnage);
                    Assert.IsNull(reportedAlCW.PackagingTonnageRed);
                    Assert.IsNull(reportedAlCW.PackagingTonnageAmber);
                    Assert.IsNull(reportedAlCW.PackagingTonnageGreen);
                    Assert.IsNull(reportedAlCW.PackagingTonnageRedMedical);
                    Assert.IsNull(reportedAlCW.PackagingTonnageAmberMedical);
                    Assert.IsNull(reportedAlCW.PackagingTonnageGreenMedical);
                }
                else if (producer.ProducerId == 22 && producer.SubsidiaryId == null)
                {
                    var reportedAlHH = producer.ProducerReportedMaterials.First(rm => rm.MaterialId == 1 && rm.PackagingType == "HH" && rm.SubmissionPeriod == "2025-H2");
                    Assert.AreEqual(25.068m, reportedAlHH.PackagingTonnage);
                    Assert.AreEqual(5.014m, reportedAlHH.PackagingTonnageRed);
                    Assert.AreEqual(5.014m, reportedAlHH.PackagingTonnageAmber);
                    Assert.AreEqual(0, reportedAlHH.PackagingTonnageGreen);
                    Assert.AreEqual(0, reportedAlHH.PackagingTonnageRedMedical);
                    Assert.AreEqual(0, reportedAlHH.PackagingTonnageAmberMedical);
                    Assert.AreEqual(15.041m, reportedAlHH.PackagingTonnageGreenMedical);

                    var reportedAlCW = producer.ProducerReportedMaterials.First(rm => rm.MaterialId == 1 && rm.PackagingType == "CW" && rm.SubmissionPeriod == "2025-H2");
                    Assert.AreEqual(5.014m, reportedAlCW.PackagingTonnage);
                    Assert.IsNull(reportedAlCW.PackagingTonnageRed);
                    Assert.IsNull(reportedAlCW.PackagingTonnageAmber);
                    Assert.IsNull(reportedAlCW.PackagingTonnageGreen);
                    Assert.IsNull(reportedAlCW.PackagingTonnageRedMedical);
                    Assert.IsNull(reportedAlCW.PackagingTonnageAmberMedical);
                    Assert.IsNull(reportedAlCW.PackagingTonnageGreenMedical);
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
