using System.Collections.Immutable;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    [TestClass]
    public class InvoicedProducerServiceTest
    {
        private readonly ApplicationDBContext _dbContext;

        public InvoicedProducerServiceTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "PayCal")
                .Options;

            _dbContext = new ApplicationDBContext(options);
        }

        [TestCleanup]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
        }

        [TestMethod]
        public async Task GetProducerDetailsTest_Returns_CancelledProducers()
        {
            TestDataHelper.SeedDatabaseForInitialRun(_dbContext);

            // Arrange
            var invoicedProducerService = new InvoicedProducerService(_dbContext, new Mock<ILogger<InvoicedProducerService>>().Object);
            var producerIds = ImmutableHashSet.Create(1);

            // Act
            var result = await invoicedProducerService.GetInvoicedProducerRecordsForYear(new RelativeYear(2025), producerIds);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        public async Task GetProducerDetailsTest_DoesNotReturns_CancelledProducers()
        {
            TestDataHelper.SeedDatabaseForUnclassified(_dbContext);

            // Arrange
            var invoicedProducerService = new InvoicedProducerService(_dbContext, new Mock<ILogger<InvoicedProducerService>>().Object);
            var producerIds = ImmutableHashSet.Create(1);

            // Act
            var result = await invoicedProducerService.GetInvoicedProducerRecordsForYear(new RelativeYear(2025), producerIds);

            // Assert
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task GetProducers_Returns_Producers()
        {
            TestDataHelper.SeedDatabaseForUnclassified(_dbContext);

            // Arrange
            var invoicedProducerService = new InvoicedProducerService(_dbContext, new Mock<ILogger<InvoicedProducerService>>().Object);

            // Act
            var result = await invoicedProducerService.GetProducerIdsForRun(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count());
        }

        [TestMethod]
        public async Task GetProducerDetails_Empty()
        {

            // Arrange
            var invoicedProducerService = new InvoicedProducerService(_dbContext, new Mock<ILogger<InvoicedProducerService>>().Object);
            var producerIds = ImmutableHashSet.Create(1);

            // Act
            var result = await invoicedProducerService.GetInvoicedProducerRecordsForYear(new RelativeYear(2025), producerIds);

            // Assert
            Assert.IsNotNull(result);
        }

    }
}