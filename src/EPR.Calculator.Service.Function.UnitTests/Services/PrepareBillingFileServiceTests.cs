using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    /// <summary>
    /// Contains unit tests for the PrepareBillingFileService class.
    /// </summary>
    [TestClass]
    public class PrepareBillingFileServiceTests
    {
        private readonly ApplicationDBContext _context;
        private Mock<ILogger<PrepareBillingFileService>> MockLogger { get; }
        private Mock<IPrepareCalcService> PrepareCalcService { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrepareBillingFileServiceTests"/> class.
        /// </summary>
        public PrepareBillingFileServiceTests()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new ApplicationDBContext(dbContextOptions);
            var dbContextFactory = new Mock<IDbContextFactory<ApplicationDBContext>>();
            dbContextFactory.Setup(f => f.CreateDbContext()).Returns(_context);

            MockLogger = new Mock<ILogger<PrepareBillingFileService>>();

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
            var runParams = new BillingRunParams
            {
                Id = 99854,
                Name = "Test",
                ApprovedBy = "user"
            };

            var service = new PrepareBillingFileService(
                _context,
                PrepareCalcService.Object,
                MockLogger.Object);

            // Act
            var result = await service.PrepareBillingFileAsync(runParams);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task PrepareBillingFileAsync_ReturnsFalse_WhenNoBillingInstructions()
        {
            // Arrange
            var runParams = new BillingRunParams
            {
                Id = 99854,
                Name = "Test",
                ApprovedBy = "user"
            };

            _context.CalculatorRuns.Add(
                new CalculatorRun {
                    Id = runParams.Id,
                    CalculatorRunClassificationId = 8,
                    Name = runParams.Name,
                    RelativeYear = new RelativeYear(2025),
                    CreatedBy = "user",
                    CreatedAt = DateTime.UtcNow });
            await _context.SaveChangesAsync();
            var service = new PrepareBillingFileService(
                _context,
                PrepareCalcService.Object,
                MockLogger.Object);

            // Act
            var result = await service.PrepareBillingFileAsync(runParams);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task PrepareBillingFileAsync_ReturnsTrue_WhenRunAndAcceptedBillingInstructionsExist()
        {
            // Arrange
            var runParams = new BillingRunParams
            {
                Id = 122444,
                Name = "TestRun",
                ApprovedBy = "user"
            };

            var acceptedProducerId = 999;

            // Add a CalculatorRun to the context
            _context.CalculatorRuns.Add(new CalculatorRun
                {
                    Id = runParams.Id,
                    CalculatorRunClassificationId = 1,
                    Name = runParams.Name,
                    RelativeYear = new RelativeYear(2025),
                    CreatedBy = "user",
                    CreatedAt = DateTime.UtcNow,
                    IsBillingFileGenerating = true
            });

            // Add an accepted billing instruction
            _context.Add(new ProducerResultFileSuggestedBillingInstruction
            {
                CalculatorRunId = runParams.Id,
                ProducerId = acceptedProducerId,
                BillingInstructionAcceptReject = PrepareBillingFileConstants.BillingInstructionAccepted,
                SuggestedBillingInstruction = "TestInstruction",
            });

            await _context.SaveChangesAsync();

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

            // Act
            var result = await service.PrepareBillingFileAsync(runParams);

            // Assert
            Assert.IsTrue(result);
            PrepareCalcService.Verify(s => s.PrepareBillingResultsAsync(
                It.Is<CalcResultsRequestDto>(dto =>
                    dto.RunId == runParams.Id &&
                    dto.AcceptedProducerIds.Contains(acceptedProducerId) &&
                    dto.IsBillingFile),
                runParams.Name,
                CancellationToken.None), Times.Once);
        }

        [TestMethod]
        public async Task PrepareBillingFileAsync_ReturnsTrue_WhenRunAndNoAcceptedBillingInstructionsExist()
        {
            // Arrange
            var runParams = new BillingRunParams
            {
                Id = 122445,
                Name = "TestRun",
                ApprovedBy = "user"
            };

            var acceptedProducerId = 999;

            // Add a CalculatorRun to the context
            _context.CalculatorRuns.Add(new CalculatorRun
            {
                Id = runParams.Id,
                CalculatorRunClassificationId = 1,
                Name = runParams.Name,
                RelativeYear = new RelativeYear(2025),
                CreatedBy = "user",
                CreatedAt = DateTime.UtcNow,
                IsBillingFileGenerating = true
            });

            // Add an accepted billing instruction
            _context.Add(new ProducerResultFileSuggestedBillingInstruction
            {
                CalculatorRunId = runParams.Id,
                ProducerId = acceptedProducerId,
                BillingInstructionAcceptReject = "Rejected",
                SuggestedBillingInstruction = "TestInstruction",
            });

            await _context.SaveChangesAsync();

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

            // Act
            var result = await service.PrepareBillingFileAsync(runParams);

            // Assert
            Assert.IsFalse(result);
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