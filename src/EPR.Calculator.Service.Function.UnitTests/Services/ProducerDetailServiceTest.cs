using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Common;
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
        public async Task GetProducerDetailsTest_Returns_CancelledProducers()
        {
            TestDataHelper.SeedDatabaseForInitialRun(this.context);

            // Arrange
            var requestDto = new CalcResultsRequestDto() { RunId = 1, RelativeYear = new RelativeYear(2025) };
            var producerDetailService = new ProducerDetailService(this.context);

            List<int> producerIds = new List<int> { 1 };

            // Act
            var result = await producerDetailService.GetProducerDetails(new RelativeYear(2025), producerIds);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        public async Task GetProducerDetailsTest_DoesNotReturns_CancelledProducers()
        {
            TestDataHelper.SeedDatabaseForUnclassified(this.context);

            // Arrange
            var producerDetailService = new ProducerDetailService(this.context);
            List<int> producerIds = new List<int> { 1 };
            // Act
            var result = await producerDetailService.GetProducerDetails(new RelativeYear(2025), producerIds);

            // Assert
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task GetProducers_Returns_Producers()
        {
            TestDataHelper.SeedDatabaseForUnclassified(this.context);

            // Arrange
            var producerDetailService = new ProducerDetailService(this.context);

            // Act
            var result = await producerDetailService.GetProducers(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count());
        }

    }
}
