using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Builder.CancelledProducers;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Features.Billing.Constants;
using EPR.Calculator.Service.Function.Features.Billing.Contexts;
using EPR.Calculator.Service.Function.Features.Calculator.Contexts;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;
using Microsoft.Extensions.Logging;

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
            _dbContext = TestFixtures.New().Create<ApplicationDBContext>();

            _materialService = new Mock<IMaterialService>();
            _materialService.Setup(t => t.GetMaterialIdsByType(It.IsAny<CancellationToken>())).ReturnsAsync(TestDataHelper.Materials.ToImmutableDictionary(t => t.Name, t => t.Id));

            var invoicedProducerService = new InvoicedProducerService(_dbContext, new Mock<ILogger<InvoicedProducerService>>().Object);

            _sut = new CalcResultCancelledProducersBuilder(invoicedProducerService, _materialService.Object);
        }

        [TestCleanup]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [TestMethod]
        public async Task CanConstruct()
        {
            TestDataHelper.SeedDatabaseForInitialRun(_dbContext);

            // Arrange
            var runContext = TestFixtures.Legacy.Create<CalculatorRunContext>() with { RunId = 2 };

            var expected = _dbContext.ProducerDesignatedRunInvoiceInstruction.FirstOrDefault(t => t.ProducerId == 2 && t.CalculatorRunId == 1);

            // Act
            var result = await _sut.ConstructAsync(runContext);

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
            var runContext = TestFixtures.Legacy.Create<CalculatorRunContext>();

            _materialService.Setup(t => t.GetMaterials(It.IsAny<CancellationToken>())).ReturnsAsync(TestDataHelper.Materials);

            // Act
            var result = await _sut.ConstructAsync(runContext);

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
            var runContext = TestFixtures.Legacy.Create<CalculatorRunContext>() with { RunId = 2 };

            var expected = _dbContext.ProducerResultFileSuggestedBillingInstruction
                .FirstOrDefault(t =>
                    t.BillingInstructionAcceptReject == BillingConstants.Action.Accepted
                    && BillingConstants.Suggestion.Cancel.Equals(t.SuggestedBillingInstruction, StringComparison.OrdinalIgnoreCase)
                    && t.CalculatorRunId == 2);

            // Act
            var result = await _sut.ConstructAsync(runContext);

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
            var runContext = TestFixtures.Legacy.Create<BillingRunContext>();

            var expected = _dbContext.ProducerResultFileSuggestedBillingInstruction
                .FirstOrDefault(t =>
                    t.BillingInstructionAcceptReject == BillingConstants.Action.Accepted
                    && BillingConstants.Suggestion.Cancel.Equals(t.SuggestedBillingInstruction, StringComparison.OrdinalIgnoreCase)
                    && t.CalculatorRunId == runContext.RunId);

            // Act
            var result = await _sut.ConstructAsync(runContext);

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
            var runContext = TestFixtures.Legacy.Create<BillingRunContext>();

            var expected = _dbContext.ProducerResultFileSuggestedBillingInstruction
                .FirstOrDefault(t =>
                    t.BillingInstructionAcceptReject == BillingConstants.Action.Rejected
                    && BillingConstants.Suggestion.Cancel.Equals(t.SuggestedBillingInstruction, StringComparison.OrdinalIgnoreCase)
                    && t.CalculatorRunId == runContext.RunId);

            // Act
            var result = await _sut.ConstructAsync(runContext);

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
            var runContext = TestFixtures.Legacy.Create<BillingRunContext>();

            var expected = _dbContext.ProducerResultFileSuggestedBillingInstruction
                .FirstOrDefault(t =>
                    t.BillingInstructionAcceptReject == BillingConstants.Action.Rejected
                    && BillingConstants.Suggestion.Cancel.Equals(t.SuggestedBillingInstruction, StringComparison.OrdinalIgnoreCase)
                    && t.CalculatorRunId == 2);

            // Act
            var result = await _sut.ConstructAsync(runContext);

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
            var runContext = TestFixtures.Legacy.Create<CalculatorRunContext>();

            // Act
            var result = await _sut.ConstructAsync(runContext);

            // Assert
            Assert.IsNotNull(result);
            var cancelledProducer = result.CancelledProducers.FirstOrDefault(t => t.ProducerId == 3);
            Assert.IsNull(cancelledProducer);
        }

        [TestMethod]
        public async Task CancelledProducersWithNoMaterials()
        {
            TestDataHelper.SeedDatabaseForInitialRunCompleted(_dbContext);

            // Arrange
            var runContext = TestFixtures.Legacy.Create<CalculatorRunContext>() with { RunId = 3 };

            var expected = _dbContext.ProducerResultFileSuggestedBillingInstruction
                .FirstOrDefault(t =>
                    t.BillingInstructionAcceptReject == BillingConstants.Action.Rejected
                    && BillingConstants.Suggestion.Cancel.Equals(t.SuggestedBillingInstruction, StringComparison.OrdinalIgnoreCase)
                    && t.CalculatorRunId == 2);

            // Act
            var result = await _sut.ConstructAsync(runContext);

            // Assert
            Assert.IsNotNull(result);
            var cancelledProducer = result.CancelledProducers.LastOrDefault();
            Assert.IsNotNull(cancelledProducer);
            Assert.AreEqual(expected?.ProducerId, cancelledProducer.ProducerId);
            Assert.IsNull(cancelledProducer.LastTonnage?.AluminiumValue);
        }
    }
}
