namespace EPR.Calculator.Service.Function.UnitTests.Builder
{
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.API.Data.Models;
    using EPR.Calculator.Service.Common;
    using EPR.Calculator.Service.Function.Builder.ProjectedProducers;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Dtos;
    using EPR.Calculator.Service.Function.Mappers;
    using EPR.Calculator.Service.Function.Models;
    using EPR.Calculator.Service.Function.Services;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using static EPR.Calculator.Service.Function.UnitTests.Builder.CalcRunLaDisposalCostBuilderTests;

    [TestClass]
    public class CalcResultProjectedProducersBuilderTest
    {
        private readonly ApplicationDBContext dbContext;
        private CalcResultProjectedProducersBuilder builder;

        private void PrepareData()
        {
            this.dbContext.Material.AddRange(
                new Material { Id = 1, Code = "AL", Name = "Aluminium", Description = "Aluminium" },
                new Material { Id = 2, Code = "FC", Name = "Fibre composite", Description = "Fibre composite" },
                new Material { Id = 3, Code = "GL", Name = "Glass", Description = "Glass" },
                new Material { Id = 4, Code = "PC", Name = "Paper or card", Description = "Paper or card" },
                new Material { Id = 5, Code = "PL", Name = "Plastic", Description = "Plastic" },
                new Material { Id = 6, Code = "ST", Name = "Steel", Description = "Steel" },
                new Material { Id = 7, Code = "WD", Name = "Wood", Description = "Wood" },
                new Material { Id = 8, Code = "OT", Name = "Other materials", Description = "Other materials" }
            );

            foreach (var run in new[] { 1, 2 }) {
                this.dbContext.CalculatorRuns.Add(new CalculatorRun
                {
                    Id = run,
                    RelativeYear = new RelativeYear(2026),
                    Name = "Run " + run
                });

                var prod11 = new ProducerDetail
                {
                    CalculatorRunId = run,
                    ProducerId = 11,
                    SubsidiaryId = null,
                    ProducerName = "Producer 11 - No Sub",
                };
                this.dbContext.ProducerDetail.Add(prod11);

                var prod22 = new ProducerDetail
                {
                    CalculatorRunId = run,
                    ProducerId = 11,
                    SubsidiaryId = "22",
                    ProducerName = "Producer 11 - With Sub 22",
                };
                this.dbContext.ProducerDetail.Add(prod22);

                foreach (var subPeriod in new[] { "2025-H1", "2025-H2"}) {
                    this.dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                    {
                        ProducerDetailId = prod11.Id,
                        MaterialId = 1, 
                        PackagingType = "HH",
                        PackagingTonnage = 100,
                        PackagingTonnageRed = 30,
                        PackagingTonnageRedMedical = 40,
                        PackagingTonnageAmber = 40,
                        PackagingTonnageAmberMedical = 0,
                        PackagingTonnageGreen = null,
                        PackagingTonnageGreenMedical = null,
                        SubmissionPeriod = subPeriod,
                    });
                    this.dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                    {
                        ProducerDetailId = prod22.Id,
                        MaterialId = 3, 
                        PackagingType = "HDC",
                        PackagingTonnage = 500,
                        PackagingTonnageRed = null,
                        PackagingTonnageRedMedical = null,
                        PackagingTonnageAmber = null,
                        PackagingTonnageAmberMedical = null,
                        PackagingTonnageGreen = null,
                        PackagingTonnageGreenMedical = null,
                        SubmissionPeriod = subPeriod,
                    });
                }
            }

            this.dbContext.SaveChanges();
        }

        public CalcResultProjectedProducersBuilderTest()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseInMemoryDatabase(databaseName: "PayCal")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

            this.dbContext = new ApplicationDBContext(dbContextOptions);
            this.dbContext.Database.EnsureCreated();
            this.builder = new CalcResultProjectedProducersBuilder(this.dbContext);
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
        public async Task Construct_H2Correctly()
        {
            // Arrange
            this.PrepareData();
            var requestDto = new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2026) };

            // Act
            var result = await this.builder.ConstructAsync(requestDto);

            // Assert
            Assert.AreEqual(2, result.H2ProjectedProducers!.Count());
            var h2Prod11Proj = result.H2ProjectedProducers!.FirstOrDefault(p => p.ProducerId == 11 && p.SubsidiaryId == null);
            Assert.IsTrue(h2Prod11Proj != null);
            Assert.IsTrue(h2Prod11Proj.ProducerId == 11);
            Assert.IsTrue(h2Prod11Proj.SubsidiaryId == null);
            Assert.IsTrue(h2Prod11Proj.Level == CommonConstants.LevelOne.ToString());
            Assert.IsTrue(h2Prod11Proj.SubmissionPeriodCode == "2025-H2");

            var prod11H2ProjMats = h2Prod11Proj.ProjectedTonnageByMaterial!;
            var expProd11AlmHHRam = new RAMTonnage{ Tonnage = 100, RedTonnage = 30, RedMedicalTonnage = 40, AmberTonnage = 40, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 };

            Assert.AreEqual(8, prod11H2ProjMats.Count());
            var alm = prod11H2ProjMats["AL"];
            Assert.AreEqual(expProd11AlmHHRam, alm.HouseholdRAMTonnage);
            Assert.AreEqual(0, alm.HouseholdTonnageDefaultedRed);

            var h2Sub22Proj = result.H2ProjectedProducers!.FirstOrDefault(p => p.ProducerId == 11 && p.SubsidiaryId == "22");
            Assert.IsTrue(h2Sub22Proj != null);
            Assert.IsTrue(h2Sub22Proj.ProducerId == 11);
            Assert.IsTrue(h2Sub22Proj.SubsidiaryId == "22");
            Assert.IsTrue(h2Sub22Proj.Level == CommonConstants.LevelTwo.ToString());
            Assert.IsTrue(h2Sub22Proj.SubmissionPeriodCode == "2025-H2");

            var sub22H2ProjMats = h2Sub22Proj.ProjectedTonnageByMaterial!;
            var expSub22GlassHDCRam = new RAMTonnage{ Tonnage = 500, RedTonnage = 0, RedMedicalTonnage = 0, AmberTonnage = 0, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 };

            Assert.AreEqual(8, sub22H2ProjMats.Count());
            var glass = sub22H2ProjMats["GL"];
            Assert.AreEqual(expSub22GlassHDCRam, glass.HouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(500, glass.HouseholdDrinksContainerDefaultedRed);
        }
    }
}