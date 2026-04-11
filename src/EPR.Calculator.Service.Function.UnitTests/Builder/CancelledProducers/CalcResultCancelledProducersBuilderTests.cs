using System.Collections.Immutable;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Builder.CancelledProducers;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.CancelledProducers
{
    [TestClass]
    public class CalcResultCancelledProducersBuilderTests
    {
        private readonly ApplicationDBContext _dbContext;
        private readonly Mock<IMaterialService> _materialService;
        private readonly CalcResultCancelledProducersBuilder _sut;

        public CalcResultCancelledProducersBuilderTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "PayCal")
                .Options;

            _dbContext = new ApplicationDBContext(options);

            _materialService = new Mock<IMaterialService>();
            var invoicedProducerService = new InvoicedProducerService(_dbContext, new Mock<ILogger<InvoicedProducerService>>().Object);

            _sut = new CalcResultCancelledProducersBuilder(invoicedProducerService, _materialService.Object);
        }

        [TestCleanup]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
        }

        [TestMethod]
        public async Task CanConstruct()
        {
            TestDataHelper.SeedDatabaseForInitialRun(_dbContext);

            // Arrange
            var requestDto = new CalcResultsRequestDto { RunId = 2, RelativeYear = new RelativeYear(2025) };

            var expected = _dbContext.ProducerDesignatedRunInvoiceInstruction.FirstOrDefault(t => t.ProducerId == 2 && t.CalculatorRunId == 1);

            _materialService.Setup(t => t.GetMaterialIdsByType()).ReturnsAsync(TestDataHelper.GetMaterials().ToImmutableDictionary(t => t.Name, t => t.Id));

            // Act
            var result = await _sut.ConstructAsync(requestDto);

            // Assert
            Assert.IsNotNull(result);
            var cancelledProducer = result.CancelledProducers.LastOrDefault();
            Assert.IsNotNull(cancelledProducer);
            Assert.AreEqual(2, cancelledProducer.ProducerId);
            Assert.AreEqual("TestOrgName|Master_1|Producer_2", cancelledProducer.ProducerOrSubsidiaryNameValue);
            Assert.AreEqual(expected?.BillingInstructionId, cancelledProducer.LatestInvoice?.BillingInstructionIdValue);
            Assert.AreEqual(expected?.CalculatorRunId.ToString(), cancelledProducer.LatestInvoice?.RunNumberValue);
            Assert.AreEqual(100, cancelledProducer.LatestInvoice?.CurrentYearInvoicedTotalToDateValue);
        }


        [TestMethod]
        public async Task CanConstructWithOutCancelledProducers()
        {
            TestDataHelper.SeedDatabaseForUnclassified(_dbContext);

            // Arrange
            var requestDto = new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2025) };

            _materialService.Setup(t => t.GetMaterials()).ReturnsAsync(TestDataHelper.GetMaterials().ToList());

            // Act
            var result = await _sut.ConstructAsync(requestDto);

            // Assert
            Assert.IsNotNull(result);
            var cancelledProducer = result.CancelledProducers.Count();
            Assert.AreEqual(0, cancelledProducer);
        }



        [TestMethod]
        public async Task CancelledProducersShouldNotGetAcceptedProducers()
        {
            TestDataHelper.SeedDatabaseForInitialRunCompleted(_dbContext);

            // Arrange
            var requestDto = new CalcResultsRequestDto { RunId = 2, RelativeYear = new RelativeYear(2025) };

            var expected = _dbContext.ProducerResultFileSuggestedBillingInstruction.FirstOrDefault(t => t.BillingInstructionAcceptReject == CommonConstants.Accepted && t.SuggestedBillingInstruction.ToLowerInvariant() == CommonConstants.Cancel.ToLowerInvariant() && t.CalculatorRunId == 2);

            _materialService.Setup(t => t.GetMaterialIdsByType()).ReturnsAsync(TestDataHelper.GetMaterials().ToImmutableDictionary(t => t.Name, t => t.Id));

            // Act
            var result = await _sut.ConstructAsync(requestDto);

            // Assert
            Assert.IsNotNull(result);
            var cancelledProducer = result.CancelledProducers.LastOrDefault();
            Assert.IsNotNull(cancelledProducer);
            Assert.AreNotEqual(expected?.ProducerId, cancelledProducer.ProducerId);
        }

        [TestMethod]
        public async Task CancelledProducersShouldGetAcceptedProducersForBillingFile()
        {
            TestDataHelper.SeedDatabaseForInitialRunCompleted(_dbContext);

            // Arrange
            var requestDto = new CalcResultsRequestDto { RunId = 3, RelativeYear = new RelativeYear(2025), IsBillingFile = true };

            var expected = _dbContext.ProducerResultFileSuggestedBillingInstruction.FirstOrDefault(t => t.BillingInstructionAcceptReject == CommonConstants.Accepted && t.SuggestedBillingInstruction.ToLowerInvariant() == CommonConstants.Cancel.ToLowerInvariant() && t.CalculatorRunId == 3);

            _materialService.Setup(t => t.GetMaterialIdsByType()).ReturnsAsync(TestDataHelper.GetMaterials().ToImmutableDictionary(t => t.Name, t => t.Id));

            // Act
            var result = await _sut.ConstructAsync(requestDto);

            // Assert
            Assert.IsNotNull(result);
            var cancelledProducer = result.CancelledProducers.LastOrDefault();
            Assert.IsNotNull(cancelledProducer);
            Assert.AreEqual(expected?.ProducerId, cancelledProducer.ProducerId);
        }

        [TestMethod]
        public async Task CancelledProducersShouldNotGetRejectedProducersForBillingFile()
        {
            TestDataHelper.SeedDatabaseForInitialRunCompleted(_dbContext);

            // Arrange
            var requestDto = new CalcResultsRequestDto { RunId = 3, RelativeYear = new RelativeYear(2025), IsBillingFile = true };

            var expected = _dbContext.ProducerResultFileSuggestedBillingInstruction.FirstOrDefault(t => t.BillingInstructionAcceptReject == CommonConstants.Rejected && t.SuggestedBillingInstruction.ToLowerInvariant() == CommonConstants.Cancel.ToLowerInvariant() && t.CalculatorRunId == 3);

            _materialService.Setup(t => t.GetMaterialIdsByType()).ReturnsAsync(TestDataHelper.GetMaterials().ToImmutableDictionary(t => t.Name, t => t.Id));

            // Act
            var result = await _sut.ConstructAsync(requestDto);

            // Assert
            Assert.IsNotNull(result);
            var cancelledProducer = result.CancelledProducers.LastOrDefault();
            Assert.IsNotNull(cancelledProducer);
            Assert.AreNotEqual(expected?.ProducerId, cancelledProducer.ProducerId);
        }


        [TestMethod]
        public async Task CancelledProducersShouldGetRejectedProducers()
        {
            TestDataHelper.SeedDatabaseForInitialRunCompleted(_dbContext);

            // Arrange
            var requestDto = new CalcResultsRequestDto { RunId = 3, RelativeYear = new RelativeYear(2025) };

            var expected = _dbContext.ProducerResultFileSuggestedBillingInstruction.FirstOrDefault(t => t.BillingInstructionAcceptReject ==
            CommonConstants.Rejected && t.SuggestedBillingInstruction.ToLowerInvariant() == CommonConstants.Cancel.ToLowerInvariant()
            && t.CalculatorRunId == 2);

            _materialService.Setup(t => t.GetMaterialIdsByType()).ReturnsAsync(TestDataHelper.GetMaterials().ToImmutableDictionary(t => t.Name, t => t.Id));

            // Act
            var result = await _sut.ConstructAsync(requestDto);

            // Assert
            Assert.IsNotNull(result);
            var cancelledProducer = result.CancelledProducers.LastOrDefault();
            Assert.IsNotNull(cancelledProducer);
            Assert.AreEqual(expected?.ProducerId, cancelledProducer.ProducerId);
        }

        [TestMethod]
        public async Task CancelledProducersShouldNotGetRejectedInitialProducer()
        {
            TestDataHelper.SeedDatabaseForInitialRunCompleted(_dbContext);

            // Arrange
            var requestDto = new CalcResultsRequestDto { RunId = 3, RelativeYear = new RelativeYear(2025) };

            _materialService.Setup(t => t.GetMaterialIdsByType()).ReturnsAsync(TestDataHelper.GetMaterials().ToImmutableDictionary(t => t.Name, t => t.Id));

            // Act
            var result = await _sut.ConstructAsync(requestDto);

            // Assert
            Assert.IsNotNull(result);
            var cancelledProducer = result.CancelledProducers.Where(t=>t.ProducerId == 3).FirstOrDefault();
            Assert.IsNull(cancelledProducer);
        }

        [TestMethod]
        public async Task CancelledProducersWithNoMaterials()
        {
            TestDataHelper.SeedDatabaseForInitialRunCompleted(_dbContext);

            // Arrange
            var requestDto = new CalcResultsRequestDto { RunId = 3, RelativeYear = new RelativeYear(2025) };

            _materialService.Setup(t => t.GetMaterialIdsByType()).ReturnsAsync(TestDataHelper.GetMaterials().ToImmutableDictionary(t => t.Name, t => t.Id));

            var expected = _dbContext.ProducerResultFileSuggestedBillingInstruction.FirstOrDefault(t => t.BillingInstructionAcceptReject ==
            CommonConstants.Rejected && t.SuggestedBillingInstruction.ToLowerInvariant() == CommonConstants.Cancel.ToLowerInvariant()
            && t.CalculatorRunId == 2);


            // Act
            var result = await _sut.ConstructAsync(requestDto);

            // Assert
            Assert.IsNotNull(result);
            var cancelledProducer = result.CancelledProducers.LastOrDefault();
            Assert.IsNotNull(cancelledProducer);
            Assert.AreEqual(expected?.ProducerId, cancelledProducer.ProducerId);
            Assert.IsNull(cancelledProducer.LastTonnage?.AluminiumValue);
        }
    }
}