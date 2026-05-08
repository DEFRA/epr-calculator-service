using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.Lapcap;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Data;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Extensions;
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
            dbContext.DefaultParameterTemplateMasterList.AddRange(DummyData.GetDefaultParameterTemplateMasterData());
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
            var runContext = DummyData.RunContexts.CalculatorRun2024;

            const string aluminium = "Aluminium";
            const string plastic = "Plastic";
            var run = runContext.ToEntity(r => r.LapcapDataMasterId = 2);

            var lapcapDataMaster = new LapcapDataMaster
            {
                Id = 2,
                RelativeYear = runContext.RelativeYear,
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

            ImmutableList<MaterialDetail> materials =
            [
                new() { Id = 1, Name = aluminium, Code = "AL" },
                new() { Id = 2, Name = plastic, Code = "PL" }
            ];

            dbContext.CalculatorRuns.Add(run);
            await dbContext.SaveChangesAsync();

            var lapcapResults = await builder.ConstructAsync(runContext, materials);

            Assert.IsNotNull(lapcapResults);
            Assert.AreEqual(CalcResultLapcapDataBuilder.LapcapHeader, lapcapResults.Name);
            Assert.AreEqual(5, lapcapResults.CalcResultLapcapDataDetails.Count());

            var headerRow = lapcapResults.CalcResultLapcapDataDetails.Single(x => x.OrderId == 1);
            Assert.IsNotNull(headerRow);
            Assert.AreEqual(LapcapHeaderConstants.Name, headerRow.Name);
            Assert.AreEqual(LapcapHeaderConstants.EnglandDisposalCost, headerRow.EnglandDisposalCost);
            Assert.AreEqual(LapcapHeaderConstants.WalesDisposalCost, headerRow.WalesDisposalCost);
            Assert.AreEqual(LapcapHeaderConstants.ScotlandDisposalCost, headerRow.ScotlandDisposalCost);
            Assert.AreEqual(LapcapHeaderConstants.NorthernIrelandDisposalCost, headerRow.NorthernIrelandDisposalCost);
            Assert.AreEqual(LapcapHeaderConstants.TotalDisposalCost, headerRow.TotalDisposalCost);

            var aluminiumRow = lapcapResults.CalcResultLapcapDataDetails.Single(x => x.Name == aluminium);
            Assert.IsNotNull(aluminiumRow);
            Assert.AreEqual(aluminium, aluminiumRow.Name);
            Assert.AreEqual("£100.00", aluminiumRow.EnglandDisposalCost);
            Assert.AreEqual("£50.00", aluminiumRow.WalesDisposalCost);
            Assert.AreEqual("£75.00", aluminiumRow.ScotlandDisposalCost);
            Assert.AreEqual("£25.00", aluminiumRow.NorthernIrelandDisposalCost);
            Assert.AreEqual("£250.00", aluminiumRow.TotalDisposalCost);

            var plasticRow = lapcapResults.CalcResultLapcapDataDetails.Single(x => x.Name == plastic);
            Assert.IsNotNull(plasticRow);
            Assert.AreEqual(plastic, plasticRow.Name);
            Assert.AreEqual("£100.00", plasticRow.EnglandDisposalCost);
            Assert.AreEqual("£50.00", plasticRow.WalesDisposalCost);
            Assert.AreEqual("£75.00", plasticRow.ScotlandDisposalCost);
            Assert.AreEqual("£25.00", plasticRow.NorthernIrelandDisposalCost);
            Assert.AreEqual("£250.00", plasticRow.TotalDisposalCost);

            var totalRow = lapcapResults.CalcResultLapcapDataDetails.Single(x => x.OrderId == 4);
            Assert.IsNotNull(totalRow);
            Assert.AreEqual("Total", totalRow.Name);
            Assert.AreEqual("£200.00", totalRow.EnglandDisposalCost);
            Assert.AreEqual("£100.00", totalRow.WalesDisposalCost);
            Assert.AreEqual("£150.00", totalRow.ScotlandDisposalCost);
            Assert.AreEqual("£50.00", totalRow.NorthernIrelandDisposalCost);
            Assert.AreEqual("£500.00", totalRow.TotalDisposalCost);

            var countryApp = lapcapResults.CalcResultLapcapDataDetails.Single(x => x.OrderId == 5);
            Assert.IsNotNull(countryApp);
            Assert.AreEqual(CalcResultLapcapDataBuilder.CountryApportionment, countryApp.Name);
            Assert.AreEqual("40.00000000%", countryApp.EnglandDisposalCost);
            Assert.AreEqual("20.00000000%", countryApp.WalesDisposalCost);
            Assert.AreEqual("30.00000000%", countryApp.ScotlandDisposalCost);
            Assert.AreEqual("10.00000000%", countryApp.NorthernIrelandDisposalCost);
            Assert.AreEqual("100.00000000%", countryApp.TotalDisposalCost);

            mockService.Verify(x => x.SaveChangesAsync(It.IsAny<CalcCountryApportionmentServiceDto>()));
        }

        public static List<LapcapDataDetail> GetLapcapDetails(LapcapDataMaster master)
        {
            var details = new List<LapcapDataDetail>();

            foreach (var uniqueRef in LapcapDataUniqueReferences)
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

        public static readonly string[] LapcapDataUniqueReferences =
        {
            "ENG-AL", "ENG-FC", "ENG-GL", "ENG-OT",
            "ENG-PC", "ENG-PL", "ENG-ST", "ENG-WD",
            "NI-AL", "NI-FC", "NI-GL", "NI-OT",
            "NI-PC", "NI-PL", "NI-ST", "NI-WD",
            "SCT-AL", "SCT-FC", "SCT-GL", "SCT-OT",
            "SCT-PC", "SCT-PL", "SCT-ST", "SCT-WD",
            "WLS-AL", "WLS-FC", "WLS-GL", "WLS-OT",
            "WLS-PC", "WLS-PL", "WLS-ST", "WLS-WD"
        };
    }
}
