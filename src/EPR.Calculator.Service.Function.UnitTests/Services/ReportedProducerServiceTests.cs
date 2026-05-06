using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.Builder;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    [TestClass]
    public class ReportedProducerServiceTests
    {
        private Mock<IDbContextFactory<ApplicationDBContext>> dbContextFactory;
        private ApplicationDBContext dbContext;
        private ReportedProducerService reportedProducerService;

        public ReportedProducerServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            dbContextFactory = new Mock<IDbContextFactory<ApplicationDBContext>>();
            dbContext = new ApplicationDBContext(options);

            dbContextFactory
                .Setup(factory => factory.CreateDbContext())
                .Returns(dbContext);

            reportedProducerService = new ReportedProducerService(dbContextFactory.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            dbContext.Dispose();
        }

        [TestMethod]
        public async Task ShouldReturnProducers_ForGivenRunId()
        {
            TestDataHelper.SeedDatabaseForInitialRun(dbContext);
            var result = await reportedProducerService.GetProducers(1);

            Assert.AreEqual(3, result.Count);

            var firstProducer = result.Single(p => p.Id == 1);

            Assert.AreEqual(2, firstProducer.ProducerReportedMaterials.Count);
            CollectionAssert.AreEquivalent(
                new List<string> { "2025-H1",  "2025-H2" },
                firstProducer
                    .ProducerReportedMaterials
                    .Select(m => m.SubmissionPeriod)
                    .ToList()
            );
        }
    }
}