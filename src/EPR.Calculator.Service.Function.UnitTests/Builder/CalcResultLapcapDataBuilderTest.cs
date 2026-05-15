using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Builder.Lapcap;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Mappers;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EPR.Calculator.Service.Function.UnitTests.Builder
{
    [TestClass]
    public class CalcResultLapcapDataBuilderTest
    {
        public CalcResultLapcapDataBuilder builder;
        protected ApplicationDBContext dbContext;
        private Mock<ICalcCountryApportionmentService> mockService;

        public CalcResultLapcapDataBuilderTest()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "PayCal")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            dbContext = new ApplicationDBContext(dbContextOptions);
            dbContext.Database.EnsureCreated();
            dbContext.DefaultParameterTemplateMasterList.RemoveRange(dbContext.DefaultParameterTemplateMasterList);
            dbContext.SaveChanges();
            dbContext.DefaultParameterTemplateMasterList.AddRange(TestDataHelper.GetDefaultParameterTemplateMasterData().ToList());
            dbContext.SaveChanges();

            mockService = new Mock<ICalcCountryApportionmentService>();

            builder = new CalcResultLapcapDataBuilder(dbContext, mockService.Object);
        }

        [TestCleanup]
        public void TearDown()
        {
            dbContext.Database.EnsureDeleted();
            dbContext.Dispose();
        }

        [TestMethod]
        public async Task ConstructTest_For_Aluminuim_Plastic()
        {
            const string aluminium = "Aluminium";
            const string plastic = "Plastic";
            var run = new CalculatorRun
            {
                Id = 1,
                CalculatorRunClassificationId = (int)RunClassification.RUNNING,
                Name = "Test Run",
                RelativeYear = new RelativeYear(2024),
                CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                CreatedBy = "Test User",
                LapcapDataMasterId = 2,
            };

            var lapcapDataMaster = new LapcapDataMaster
            {
                Id = 2,
                RelativeYear = new RelativeYear(2024),
                CreatedBy = "Testuser",
                CreatedAt = DateTime.UtcNow,
                EffectiveFrom = DateTime.UtcNow,
            };
            var details = GetLapcapDetails(lapcapDataMaster);
            details.ForEach(detail => detail.LapcapDataMaster = lapcapDataMaster);

            dbContext.Country.Add(new Country { Code = "En", Name = "England", Description = "England" });
            dbContext.Country.Add(new Country { Code = "Wa", Name = "Wales", Description = "Wales" });
            dbContext.Country.Add(new Country { Code = "Sc", Name = "Scotland", Description = "Scotland" });
            dbContext.Country.Add(new Country { Code = "NI", Name = "Northern Ireland", Description = "Northern Ireland" });

            dbContext.CostType.Add(new CostType { Code = "1", Name = "Fee for LA Disposal Costs", Description = "Fee for LA Disposal Costs" });

            dbContext.LapcapDataMaster.Add(lapcapDataMaster);
            dbContext.LapcapDataDetail.AddRange(details);

            dbContext.Material.Add(new Material { Name = aluminium, Code = "AL", Description = "Some" });
            dbContext.Material.Add(new Material { Name = plastic, Code = "PL", Description = "Some" });

            dbContext.CalculatorRuns.Add(run);
            await dbContext.SaveChangesAsync();

            var materialDetails = MaterialMapper.Map(await dbContext.Material.ToListAsync());

            var resultsDto = new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2024) };
            var lapcapResults = await builder.ConstructAsync(materialDetails, resultsDto);

            Assert.IsNotNull(lapcapResults);
            Assert.AreEqual(3, lapcapResults.CalcResultLapcapDataDetails.Count());

            var aluminiumRow = lapcapResults.CalcResultLapcapDataDetails.Single(x => x.Name == aluminium);
            Assert.IsNotNull(aluminiumRow);
            Assert.AreEqual(aluminium, aluminiumRow.Name);
            Assert.AreEqual(100, aluminiumRow.EnglandCost);
            Assert.AreEqual(50, aluminiumRow.WalesCost);
            Assert.AreEqual(75, aluminiumRow.ScotlandCost);
            Assert.AreEqual(25, aluminiumRow.NorthernIrelandCost);
            Assert.AreEqual(250, aluminiumRow.TotalCost);

            var plasticRow = lapcapResults.CalcResultLapcapDataDetails.Single(x => x.Name == plastic);
            Assert.IsNotNull(plasticRow);
            Assert.AreEqual(plastic, plasticRow.Name);
            Assert.AreEqual(100, plasticRow.EnglandCost);
            Assert.AreEqual(50, plasticRow.WalesCost);
            Assert.AreEqual(75, plasticRow.ScotlandCost);
            Assert.AreEqual(25, plasticRow.NorthernIrelandCost);
            Assert.AreEqual(250, plasticRow.TotalCost);

            var totalRow = lapcapResults.CalcResultLapcapDataDetails.Single(x => x.OrderId == 4);
            Assert.IsNotNull(totalRow);
            Assert.AreEqual("Total", totalRow.Name);
            Assert.AreEqual(200, totalRow.EnglandCost);
            Assert.AreEqual(100, totalRow.WalesCost);
            Assert.AreEqual(150, totalRow.ScotlandCost);
            Assert.AreEqual(50, totalRow.NorthernIrelandCost);
            Assert.AreEqual(500, totalRow.TotalCost);

            var countryApportionment = lapcapResults.CountryApportionment;
            Assert.IsNotNull(countryApportionment);
            Assert.AreEqual(40, countryApportionment.England);
            Assert.AreEqual(20, countryApportionment.Wales);
            Assert.AreEqual(30, countryApportionment.Scotland);
            Assert.AreEqual(10, countryApportionment.NorthernIreland);

            mockService.Verify(x => x.SaveChangesAsync(It.IsAny<CalcCountryApportionmentServiceDto>()));
        }

        public static List<LapcapDataDetail> GetLapcapDetails(LapcapDataMaster master)
        {
            var details = new List<LapcapDataDetail>();

            foreach (var uniqueRef in LapcapDataUniqueReferences.UniqueReferences)
            {
                details.Add(
                    new LapcapDataDetail
                    {
                        LapcapDataMasterId = 2,
                        UniqueReference = uniqueRef,
                        TotalCost = GetTotalCostByCountry(uniqueRef),
                        LapcapDataMaster = master,
                    }
                );
            }

            return details;
        }

        public static decimal GetTotalCostByCountry(string uniqueRef)
        {
            if (uniqueRef.StartsWith("ENG-"))
            {
                return 100M;
            }

            if (uniqueRef.StartsWith("SCT-"))
            {
                return 75M;
            }

            if (uniqueRef.StartsWith("WLS-"))
            {
                return 50M;
            }

            return 25M;
        }
    }
}
