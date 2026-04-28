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
        private CalcResultPartialObligationBuilder builder;

        private List<ProducerReportedMaterialsForSubmissionPeriod> PrepareData()
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

            var producerReportedMaterials =
                new[] { "2025-H1", "2025-H2"}.SelectMany(subPeriod =>
                    new List<ProducerReportedMaterial> {
                        new ProducerReportedMaterial
                        {
                            PackagingType = "HH",
                            MaterialId = alm.Id,
                            Material = alm,
                            SubmissionPeriod = subPeriod,
                            ProducerDetail = producerDetail,
                        },
                        new ProducerReportedMaterial
                        {
                            PackagingType = "HDC",
                            MaterialId = alm.Id,
                            Material = alm,
                            SubmissionPeriod = subPeriod,
                            ProducerDetail = producerDetail,
                        },
                        new ProducerReportedMaterial
                        {
                            PackagingType = "HH",
                            MaterialId = alm.Id,
                            Material = alm,
                            PackagingTonnage = 50,
                            SubmissionPeriod = subPeriod,
                            ProducerDetail = producerDetail2,
                        },
                        new ProducerReportedMaterial
                        {
                            PackagingType = "CW",
                            MaterialId = alm.Id,
                            Material = alm,
                            PackagingTonnage = 10,
                            SubmissionPeriod = subPeriod,
                            ProducerDetail = producerDetail2,
                        }
                    }
                );
            this.dbContext.ProducerReportedMaterial.AddRange(producerReportedMaterials);

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
                ProducerId = 44,
                SubsidiaryId = null,
                ProducerName = "Partial Packaging",
            };
            dbContext.ProducerDetail.Add(producerDetail4);

            var producerReportedMaterials2 =
                new[] { "2025-H1", "2025-H2"}.SelectMany(subPeriod =>
                    new List<ProducerReportedMaterial> {
                        new ProducerReportedMaterial
                        {
                            PackagingType = "HH",
                            MaterialId = alm.Id,
                            Material = alm,
                            SubmissionPeriod = subPeriod,
                            ProducerDetail = producerDetail3,
                        },
                        new ProducerReportedMaterial
                        {
                            PackagingType = "HDC",
                            MaterialId = alm.Id,
                            Material = alm,
                            SubmissionPeriod = subPeriod,
                            ProducerDetail = producerDetail3,
                        },
                        new ProducerReportedMaterial
                        {
                            PackagingType = "HH",
                            MaterialId = alm.Id,
                            Material = alm,
                            PackagingTonnage = 50,
                            SubmissionPeriod = subPeriod,
                            ProducerDetail = producerDetail4,
                        },
                        new ProducerReportedMaterial
                        {
                            PackagingType = "CW",
                            MaterialId = alm.Id,
                            Material = alm,
                            PackagingTonnage = 10,
                            SubmissionPeriod = subPeriod,
                            ProducerDetail = producerDetail4,
                        }
                    }
                ).ToList();
            this.dbContext.ProducerReportedMaterial.AddRange(producerReportedMaterials2);

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

            return (producerReportedMaterials.Concat(producerReportedMaterials2)).GroupBy(rm => (rm.ProducerDetail, rm.SubmissionPeriod)).Select(rms =>
                new ProducerReportedMaterialsForSubmissionPeriod(
                    producerId : rms.Key.Item1.ProducerId,
                    subsidiaryId : rms.Key.Item1.SubsidiaryId,
                    submissionPeriod : rms.Key.Item2,
                    reportedMaterials : rms.ToList()
                )
            ).ToList();
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

            Console.WriteLine($"result.Item2: {JsonConvert.SerializeObject(result.Item2, Formatting.Indented)}");

            // Assert
            Assert.AreEqual(1, result.Item2.PartialObligations!.Count());
            var parOrg = result.Item2.PartialObligations!.First();
            Assert.IsTrue(parOrg.ProducerId == 22);
            Assert.IsTrue(parOrg.SubsidiaryId == null);
            Assert.IsTrue(parOrg.ProducerName == "Partial Packaging");
            Assert.IsTrue(parOrg.Level == CommonConstants.LevelOne.ToString());
            Assert.IsTrue(parOrg.JoiningDate == "15/07/2025");
            Assert.IsTrue(parOrg.DaysObligated == 183);
            Assert.IsTrue(parOrg.DaysInSubmissionYear == 365);
            Assert.IsTrue(parOrg.ObligatedPercentage == "50.14%");

            var parOrgMats = parOrg.PartialObligationTonnageByMaterial;
            //Assert.AreEqual(8, parOrgMats.Count()); // TODO expects one per material
            //Console.WriteLine("" + parOrgMats.Select(mat => mat.Key == MaterialCodes.Aluminium));
            var aluResult = parOrgMats.Where(mat => mat.Key == MaterialCodes.Aluminium).First();
            //Console.WriteLine($">> AL: {JsonConvert.SerializeObject(parOrgMats.Where(mat => mat.Key == MaterialCodes.Aluminium).First(), Formatting.Indented)}");
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
            // TODO requires adding empty materials
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
    }
}
