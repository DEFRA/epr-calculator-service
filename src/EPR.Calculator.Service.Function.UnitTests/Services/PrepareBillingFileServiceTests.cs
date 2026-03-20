using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Common.Logging;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    /// <summary>
    /// Contains unit tests for the PrepareBillingFileService class.
    /// </summary>
    [TestClass]
    public class PrepareBillingFileServiceTests
    {
        private readonly DbContextOptions<ApplicationDBContext> _dbContextOptions;
        private Mock<IDbContextFactory<ApplicationDBContext>> _dbContextFactory;
        private ApplicationDBContext _context;
        private Mock<ICalculatorTelemetryLogger> MockLogger { get; }
        private Mock<IPrepareCalcService> PrepareCalcService { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrepareBillingFileServiceTests"/> class.
        /// </summary>
        public PrepareBillingFileServiceTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new ApplicationDBContext(_dbContextOptions);
            _dbContextFactory = new Mock<IDbContextFactory<ApplicationDBContext>>();
            _dbContextFactory.Setup(f => f.CreateDbContext()).Returns(_context);

            MockLogger = new Mock<ICalculatorTelemetryLogger>();

            PrepareCalcService = new Mock<IPrepareCalcService>();
            PrepareCalcService.Setup(s => s.PrepareCalcResultsAsync(
                It.IsAny<CalcResultsRequestDto>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
        }

        [TestMethod]
        public async Task PrepareBillingFileAsync_ReturnsFalse_WhenRunNotFound()
        {
            // Arrange
            var calculatorRunId = 99854;
            var calculatorName = "Test";
            var approvedBy = "user";
            var service = new PrepareBillingFileService(
                _context,
                PrepareCalcService.Object,
                MockLogger.Object);

            // Act
            var result = await service.PrepareBillingFileAsync(calculatorRunId, calculatorName, approvedBy);

            // Assert
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public async Task PrepareBillingFileAsync_ReturnsFalse_WhenNoBillingInstructions()
        {
            // Arrange
            var calculatorRunId = 99854;
            var calculatorName = "Test";
            _context.CalculatorRuns.Add(
                new CalculatorRun {
                    Id = calculatorRunId,
                    CalculatorRunClassificationId = 8,
                    Name = calculatorName,
                    RelativeYear = new RelativeYear(2025),
                    CreatedBy = "user",
                    CreatedAt = DateTime.UtcNow });
            _context.SaveChanges();
            var service = new PrepareBillingFileService(
                _context,
                PrepareCalcService.Object,
                MockLogger.Object);
            var approvedBy = "user";

            // Act
            var result = await service.PrepareBillingFileAsync(calculatorRunId, calculatorName, approvedBy);

            // Assert
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public async Task PrepareBillingFileAsync_ReturnsTrue_WhenRunAndAcceptedBillingInstructionsExist()
        {
            // Arrange
            var calculatorRunId = 122444;
            var calculatorName = "TestRun";
            var acceptedProducerId = 999;

            // Add a CalculatorRun to the context
            _context.CalculatorRuns.Add(new CalculatorRun
                {
                    Id = calculatorRunId,
                    CalculatorRunClassificationId = 1,
                    Name = calculatorName,
                    RelativeYear = new RelativeYear(2025),
                    CreatedBy = "user",
                    CreatedAt = DateTime.UtcNow,
                    IsBillingFileGenerating = true
            });

            // Add an accepted billing instruction
            _context.Add(new ProducerResultFileSuggestedBillingInstruction
            {
                CalculatorRunId = calculatorRunId,
                ProducerId = acceptedProducerId,
                BillingInstructionAcceptReject = PrepareBillingFileConstants.BillingInstructionAccepted,
                SuggestedBillingInstruction = "TestInstruction",
            });

            _context.SaveChanges();

            // Setup PrepareCalcService to return true
            PrepareCalcService
                .Setup(s => s.PrepareBillingResultsAsync(
                    It.IsAny<CalcResultsRequestDto>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var service = new PrepareBillingFileService(
                _context,
                PrepareCalcService.Object,
                MockLogger.Object);
            var approvedBy = "user";

            // Act
            var result = await service.PrepareBillingFileAsync(calculatorRunId, calculatorName, approvedBy);

            // Assert
            Assert.IsTrue(result);
            PrepareCalcService.Verify(s => s.PrepareBillingResultsAsync(
                It.Is<CalcResultsRequestDto>(dto =>
                    dto.RunId == calculatorRunId &&
                    dto.AcceptedProducerIds.Contains(acceptedProducerId) &&
                    dto.IsBillingFile),
                calculatorName,
                CancellationToken.None), Times.Once);
        }

        [TestMethod]
        public async Task PrepareBillingFileAsync_ReturnsTrue_WhenRunAndNoAcceptedBillingInstructionsExist()
        {
            // Arrange
            var calculatorRunId = 122445;
            var calculatorName = "TestRun";
            var acceptedProducerId = 999;

            // Add a CalculatorRun to the context
            _context.CalculatorRuns.Add(new CalculatorRun
            {
                Id = calculatorRunId,
                CalculatorRunClassificationId = 1,
                Name = calculatorName,
                RelativeYear = new RelativeYear(2025),
                CreatedBy = "user",
                CreatedAt = DateTime.UtcNow,
                IsBillingFileGenerating = true
            });

            // Add an accepted billing instruction
            _context.Add(new ProducerResultFileSuggestedBillingInstruction
            {
                CalculatorRunId = calculatorRunId,
                ProducerId = acceptedProducerId,
                BillingInstructionAcceptReject = "Rejected",
                SuggestedBillingInstruction = "TestInstruction",
            });

            _context.SaveChanges();

            // Setup PrepareCalcService to return true
            PrepareCalcService
                .Setup(s => s.PrepareBillingResultsAsync(
                    It.IsAny<CalcResultsRequestDto>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var service = new PrepareBillingFileService(
                _context,
                PrepareCalcService.Object,
                MockLogger.Object);
            var approvedBy = "user";

            // Act
            var result = await service.PrepareBillingFileAsync(calculatorRunId, calculatorName, approvedBy);

            // Assert
            Assert.AreEqual(false, result);
        }
    }

    // Helper factory for in-memory db context
    public static class TestDbContextFactory
    {
        public static ApplicationDBContext Create()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "TestDb" + Guid.NewGuid())
                .Options;
            return new ApplicationDBContext(options);
        }
    }
}
