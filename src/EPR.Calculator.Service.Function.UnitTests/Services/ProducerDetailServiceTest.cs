using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.Builder;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    [TestClass]
    public class ProducerDetailServiceTest
    {
        private readonly ApplicationDBContext context;

        public ProducerDetailServiceTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "PayCal")
                .Options;
            this.context = new ApplicationDBContext(options);
        }

        [TestCleanup]
        public void TearDown()
        {
            this.context?.Database.EnsureDeleted();
        }

        [TestMethod]
        public async Task GetLatestProducerDetailsForThisFinancialYearTest_Returns_CancelledProducers()
        {
            TestDataHelper.SeedDatabaseForInitialRun(this.context);

            // Arrange
            var requestDto = new CalcResultsRequestDto() { RunId = 1 };
            var producerDetailService = new ProducerDetailService(this.context);

            List<int> producerIds = new List<int> { 1 };

            // Act
            var result = await producerDetailService.GetLatestProducerDetailsForThisFinancialYear("2025-26", producerIds);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        public async Task GetLatestProducerDetailsForThisFinancialYearTest_DoesNotReturns_CancelledProducers()
        {
            TestDataHelper.SeedDatabaseForUnclassified(this.context);

            // Arrange
            var producerDetailService = new ProducerDetailService(this.context);
            List<int> producerIds = new List<int> { 1 };
            // Act
            var result = await producerDetailService.GetLatestProducerDetailsForThisFinancialYear("2025-26", producerIds);

            // Assert
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public void GetProducers_Returns_Producers()
        {
            TestDataHelper.SeedDatabaseForUnclassified(this.context);

            // Arrange
            var producerDetailService = new ProducerDetailService(this.context);

            // Act
            var result = producerDetailService.GetProducers(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count());
        }
    }
}