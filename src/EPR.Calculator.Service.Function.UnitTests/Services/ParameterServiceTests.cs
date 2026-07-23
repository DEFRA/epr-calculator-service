using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Helpers;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    /// <summary>
    /// Unit tests for the <see cref="ParameterService"/> class.
    /// </summary>
    [TestClass]
    public class ParameterServiceTests
    {
        private readonly Mock<IDbContextFactory<ApplicationDBContext>> dbContextFactory;
        private readonly ApplicationDBContext dbContext;
        private readonly ParameterService parameterService;

        public ParameterServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            dbContextFactory = new Mock<IDbContextFactory<ApplicationDBContext>>();
            dbContext = new ApplicationDBContext(options);

            dbContextFactory
                .Setup(factory => factory.CreateDbContext())
                .Returns(dbContext);

            parameterService = new ParameterService(dbContextFactory.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            dbContext.Dispose();
        }

        [TestMethod]
        public async Task ShouldReturnDefaultParameters()
        {
            // Arrange
            const int runId = 1;
            const int masterId = 100;

            var empty = DefaultParametersHelper.Empty();

            var expected = empty with
            {
                BadDebtProvision = 0.15m,
                RedModulationFactor = 1.5m,
                CommunicationCosts = empty.CommunicationCosts with
                {
                    ByMaterialCode = new Dictionary<string, decimal>(empty.CommunicationCosts.ByMaterialCode)
                    {
                        ["AL"] = 123.45m
                    },

                    ByCountry = empty.CommunicationCosts.ByCountry with
                    {
                        UnitedKingdom = 50m
                    }
                },

                LateReportingTonnageByMaterialCode =
                    new Dictionary<string, RamTonnageGroup>(empty.LateReportingTonnageByMaterialCode)
                    {
                        ["AL"] = new RamTonnageGroup
                        {
                            Amber = 10m,
                            Green = 20m,
                            Red = 30m,
                            Total = 60m
                        }
                    }
            };

            dbContext.CalculatorRuns.Add(new CalculatorRun
            {
                Id = runId,
                Name = "Some name",
                DefaultParameterSettingMasterId = masterId
            });

            dbContext.DefaultParameterSettings.Add(new DefaultParameterSettingMaster
            {
                Id = masterId
            });

            dbContext.DefaultParameterSettingDetail.AddRange(DefaultParametersHelper.ToDetails(expected, masterId));

            await dbContext.SaveChangesAsync();

            // Act
            var result = await parameterService.GetDefaultParameters(runId);

            // Assert
            Assert.AreEqual(123.45m, result.CommunicationCosts.ByMaterialCode["AL"]);
            Assert.AreEqual(50m,     result.CommunicationCosts.ByCountry.UnitedKingdom);
            Assert.AreEqual(0.15m,   result.BadDebtProvision);
            Assert.AreEqual(1.5m,    result.RedModulationFactor);
            Assert.AreEqual(10m,     result.LateReportingTonnageByMaterialCode["AL"].Amber);
            Assert.AreEqual(20m,     result.LateReportingTonnageByMaterialCode["AL"].Green);
            Assert.AreEqual(30m,     result.LateReportingTonnageByMaterialCode["AL"].Red);
            Assert.AreEqual(60m,     result.LateReportingTonnageByMaterialCode["AL"].Total);
        }

    }
}
