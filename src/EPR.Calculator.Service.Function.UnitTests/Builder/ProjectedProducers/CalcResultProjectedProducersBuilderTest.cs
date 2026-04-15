namespace EPR.Calculator.Service.Function.UnitTests.Builder.ProjectedProducers
{
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.API.Data.Models;
    using EPR.Calculator.Service.Common;
    using EPR.Calculator.Service.Function.Builder.ProjectedProducers;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Mappers;
    using EPR.Calculator.Service.Function.Misc;

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
            dbContext.Material.AddRange(
                new Material { Id = 1, Code = "AL", Name = "Aluminium", Description = "Aluminium" },
                new Material { Id = 2, Code = "GL", Name = "Glass", Description = "Glass" },
                new Material { Id = 3, Code = "OT", Name = "Other materials", Description = "Other materials" }
            );

            foreach (var run in new[] { 1, 2 }) {
                dbContext.CalculatorRuns.Add(new CalculatorRun
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
                    ProducerName = "Producer 11 - Parent",
                };
                dbContext.ProducerDetail.Add(prod11);

                var prod22 = new ProducerDetail
                {
                    CalculatorRunId = run,
                    ProducerId = 11,
                    SubsidiaryId = "22",
                    ProducerName = "Producer 11 - Sub 22",
                };
                dbContext.ProducerDetail.Add(prod22);

                var prod33 = new ProducerDetail
                {
                    CalculatorRunId = run,
                    ProducerId = 33,
                    SubsidiaryId = null,
                    ProducerName = "Producer 33 - No subs",
                };
                dbContext.ProducerDetail.Add(prod33);

                var prod44 = new ProducerDetail
                {
                    CalculatorRunId = run,
                    ProducerId = 44,
                    SubsidiaryId = "444",
                    ProducerName = "Producer 44 - Sub 444 - No parent",
                };
                dbContext.ProducerDetail.Add(prod44);

                foreach (var subPeriod in new[] { "2025-H1", "2025-H2"}) {
                    dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
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
                    dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                    {
                        ProducerDetailId = prod22.Id,
                        MaterialId = 1, 
                        PackagingType = "HH",
                        PackagingTonnage = 500,
                        PackagingTonnageRed = null,
                        PackagingTonnageRedMedical = null,
                        PackagingTonnageAmber = null,
                        PackagingTonnageAmberMedical = null,
                        PackagingTonnageGreen = null,
                        PackagingTonnageGreenMedical = null,
                        SubmissionPeriod = subPeriod,
                    });
                    dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                    {
                        ProducerDetailId = prod22.Id,
                        MaterialId = 2, 
                        PackagingType = "HDC",
                        PackagingTonnage = 500,
                        PackagingTonnageRed = 100,
                        PackagingTonnageRedMedical = null,
                        PackagingTonnageAmber = null,
                        PackagingTonnageAmberMedical = null,
                        PackagingTonnageGreen = null,
                        PackagingTonnageGreenMedical = null,
                        SubmissionPeriod = subPeriod,
                    });
                    dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                    {
                        ProducerDetailId = prod33.Id,
                        MaterialId = 1, 
                        PackagingType = "PB",
                        PackagingTonnage = 150,
                        PackagingTonnageRed = 10,
                        PackagingTonnageRedMedical = 40,
                        PackagingTonnageAmber = 20,
                        PackagingTonnageAmberMedical = 30,
                        PackagingTonnageGreen = 5,
                        PackagingTonnageGreenMedical = 45,
                        SubmissionPeriod = subPeriod,
                    });
                    dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                    {
                        ProducerDetailId = prod44.Id,
                        MaterialId = 1, 
                        PackagingType = "HH",
                        PackagingTonnage = 150,
                        SubmissionPeriod = subPeriod,
                    });
                }
            }

            dbContext.SaveChanges();
        }

        public CalcResultProjectedProducersBuilderTest()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseInMemoryDatabase(databaseName: "PayCal")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

            dbContext = new ApplicationDBContext(dbContextOptions);
            dbContext.Database.EnsureCreated();
            builder = new CalcResultProjectedProducersBuilder(dbContext);
        }

        [TestCleanup]
        public void Teardown()
        {
            if (dbContext != null)
            {
                dbContext.Database.EnsureDeleted();
                dbContext.Dispose();
            }
        }

        [TestMethod]
        public async Task Construct_WorksCorrectly()
        {
            // Arrange
            PrepareData();
            var requestDto = new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2026) };

            // Act
            var result = await builder.ConstructAsync(requestDto);

            // Assert H2
            Assert.IsNotNull(result.H2ProjectedProducersHeaders);
            Assert.AreEqual(6, result.H2ProjectedProducers!.Count());

            var h2Prod11Subtotal = result.H2ProjectedProducers!.First(p => p.ProducerId == 11 && p.SubsidiaryId == null && p.IsSubtotal == true && p.Level == CommonConstants.LevelOne.ToString() && p.SubmissionPeriodCode == "2025-H2");
            var h2Prod11Parent = result.H2ProjectedProducers!.First(p => p.ProducerId == 11 && p.SubsidiaryId == null && p.IsSubtotal == false && p.Level == CommonConstants.LevelTwo.ToString() && p.SubmissionPeriodCode == "2025-H2");
            var h2Prod11Subsidiary = result.H2ProjectedProducers!.First(p => p.ProducerId == 11 && p.SubsidiaryId == "22" && p.IsSubtotal == false && p.Level == CommonConstants.LevelTwo.ToString() && p.SubmissionPeriodCode == "2025-H2");
            var h2Prod33Individual = result.H2ProjectedProducers!.First(p => p.ProducerId == 33 && p.SubsidiaryId == null && p.IsSubtotal == false && p.Level == CommonConstants.LevelOne.ToString() && p.SubmissionPeriodCode == "2025-H2");
            var h2Prod44Subtotal = result.H2ProjectedProducers!.First(p => p.ProducerId == 44 && p.SubsidiaryId == null && p.IsSubtotal == true && p.Level == CommonConstants.LevelOne.ToString() && p.SubmissionPeriodCode == "2025-H2");
            var h2Prod44IndividualSub = result.H2ProjectedProducers!.First(p => p.ProducerId == 44 && p.SubsidiaryId == "444" && p.IsSubtotal == false && p.Level == CommonConstants.LevelTwo.ToString() && p.SubmissionPeriodCode == "2025-H2");
        
            Assert.AreEqual(
                h2Prod11Parent.ProjectedTonnageByMaterial[MaterialCodes.Aluminium].HouseholdRAMTonnage.AmberTonnage + h2Prod11Subsidiary.ProjectedTonnageByMaterial[MaterialCodes.Aluminium].HouseholdRAMTonnage.AmberTonnage, 
                h2Prod11Subtotal.ProjectedTonnageByMaterial[MaterialCodes.Aluminium].HouseholdRAMTonnage.AmberTonnage
            );
            Assert.AreEqual(
                h2Prod11Parent.ProjectedTonnageByMaterial[MaterialCodes.Glass].HouseholdDrinksContainerRAMTonnage?.RedTonnage + h2Prod11Subsidiary.ProjectedTonnageByMaterial[MaterialCodes.Glass].HouseholdDrinksContainerRAMTonnage?.RedTonnage, 
                h2Prod11Subtotal.ProjectedTonnageByMaterial[MaterialCodes.Glass].HouseholdDrinksContainerRAMTonnage?.RedTonnage
            );
            Assert.AreEqual(
                h2Prod44IndividualSub.ProjectedTonnageByMaterial[MaterialCodes.Aluminium].HouseholdRAMTonnage.Tonnage, 
                h2Prod44Subtotal.ProjectedTonnageByMaterial[MaterialCodes.Aluminium].HouseholdRAMTonnage.Tonnage
            );

            // Assert H1
            Assert.IsNotNull(result.H1ProjectedProducersHeaders);
            Assert.AreEqual(6, result.H1ProjectedProducers!.Count());

            var h1Prod11Subtotal = result.H1ProjectedProducers!.First(p => p.ProducerId == 11 && p.SubsidiaryId == null && p.IsSubtotal == true && p.Level == CommonConstants.LevelOne.ToString() && p.SubmissionPeriodCode == "2025-H1");
            var h1Prod11Parent = result.H1ProjectedProducers!.First(p => p.ProducerId == 11 && p.SubsidiaryId == null && p.IsSubtotal == false && p.Level == CommonConstants.LevelTwo.ToString() && p.SubmissionPeriodCode == "2025-H1");
            var h1Prod11Subsidiary = result.H1ProjectedProducers!.First(p => p.ProducerId == 11 && p.SubsidiaryId == "22" && p.IsSubtotal == false && p.Level == CommonConstants.LevelTwo.ToString() && p.SubmissionPeriodCode == "2025-H1");
            var h1Prod33Individual = result.H1ProjectedProducers!.First(p => p.ProducerId == 33 && p.SubsidiaryId == null && p.IsSubtotal == false && p.Level == CommonConstants.LevelOne.ToString() && p.SubmissionPeriodCode == "2025-H1");
            var h1Prod44Subtotal = result.H1ProjectedProducers!.First(p => p.ProducerId == 44 && p.SubsidiaryId == null && p.IsSubtotal == true && p.Level == CommonConstants.LevelOne.ToString() && p.SubmissionPeriodCode == "2025-H1");
            var h1Prod44IndividualSub = result.H1ProjectedProducers!.First(p => p.ProducerId == 44 && p.SubsidiaryId == "444" && p.IsSubtotal == false && p.Level == CommonConstants.LevelTwo.ToString() && p.SubmissionPeriodCode == "2025-H1");
        
            var h1h1Prod11ParentAlm = h1Prod11Parent.ProjectedTonnageByMaterial[MaterialCodes.Aluminium];
            var h1Prod11SubsidiaryAlm = h1Prod11Subsidiary.ProjectedTonnageByMaterial[MaterialCodes.Aluminium];

            Assert.AreEqual(
                ((h1h1Prod11ParentAlm.RedH2Proportion * h1h1Prod11ParentAlm.H2TotalTonnage) + (h1Prod11SubsidiaryAlm.RedH2Proportion * h1Prod11SubsidiaryAlm.H2TotalTonnage)) / (h1h1Prod11ParentAlm.H2TotalTonnage + h1Prod11SubsidiaryAlm.H2TotalTonnage), 
                h1Prod11Subtotal.ProjectedTonnageByMaterial[MaterialCodes.Aluminium].RedH2Proportion
            );
            Assert.AreEqual(
                h1Prod11Parent.ProjectedTonnageByMaterial[MaterialCodes.Glass].HouseholdDrinksContainerTonnageWithoutRAM + h1Prod11Subsidiary.ProjectedTonnageByMaterial[MaterialCodes.Glass].HouseholdDrinksContainerTonnageWithoutRAM, 
                h1Prod11Subtotal.ProjectedTonnageByMaterial[MaterialCodes.Glass].HouseholdDrinksContainerTonnageWithoutRAM
            );
            Assert.AreEqual(
                h1Prod44IndividualSub.ProjectedTonnageByMaterial[MaterialCodes.Aluminium].ProjectedHouseholdRAMTonnage.Tonnage, 
                h1Prod44Subtotal.ProjectedTonnageByMaterial[MaterialCodes.Aluminium].ProjectedHouseholdRAMTonnage.Tonnage
            );
        }
    }
}