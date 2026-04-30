using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Builder.Lapcap;
using EPR.Calculator.Service.Function.Features.Calculator.Contexts;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.Service.Function.UnitTests.Builder
{
    [TestClass]
    public class CalcResultLapcapDataBuilderTest
    {
        private CalcResultLapcapDataBuilder builder = null!;
        private ApplicationDBContext dbContext = null!;
        private Mock<ICalcCountryApportionmentService> mockService = null!;

        [TestInitialize]
        public void Init()
        {
            dbContext = TestFixtures.New().Create<ApplicationDBContext>();
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
                Classification = RunClassification.Running,
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
            await dbContext.SaveChangesAsync();

            dbContext.Material.Add(new Material { Name = aluminium, Code = "AL", Description = "Some" });
            dbContext.Material.Add(new Material { Name = plastic, Code = "PL", Description = "Some" });
            dbContext.CalculatorRuns.Add(run);
            await dbContext.SaveChangesAsync();

            var runContext = TestFixtures.Legacy.Create<CalculatorRunContext>();
            var lapcapResults = await builder.ConstructAsync(runContext);

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
