namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.Service.Common.Logging;
    using EPR.Calculator.Service.Function.Enums;
    using EPR.Calculator.Service.Function.Services;
    using Microsoft.EntityFrameworkCore;
    using Moq;

    /// <summary>
    /// Unit tests for the <see cref="ClassificationService"/> class.
    /// </summary>
    [TestClass]
    public class ClassificationServiceTests
    {
        private Mock<ICalculatorTelemetryLogger> TelemetryLogger { get; init; }
        private Mock<IDbContextFactory<ApplicationDBContext>> dbContextFactory;
        private ApplicationDBContext dbContext;
        private ClassificationService classificationService;

        public ClassificationServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            this.dbContextFactory = new Mock<IDbContextFactory<ApplicationDBContext>>();
            this.dbContext = new ApplicationDBContext(options);
            this.dbContextFactory.Setup(factory => factory.CreateDbContext()).Returns(this.dbContext);
            this.TelemetryLogger = new Mock<ICalculatorTelemetryLogger>();
            this.classificationService = new ClassificationService(this.dbContextFactory.Object, this.TelemetryLogger.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            this.dbContext?.Dispose();
        }

        [TestMethod]
        [DataRow(1, RunClassification.RUNNING, "2023-24")]
        [DataRow(2, RunClassification.UNCLASSIFIED, "2024-25")]
        [DataRow(3, RunClassification.ERROR, "2025-26")]
        public async Task ShouldUpdateRunClassification(int runId, RunClassification runClassification, string financialYear)
        {
            // Arrange
            var calculatorRunFinancialYear = new CalculatorRunFinancialYear { Name = financialYear };
            this.dbContext.CalculatorRuns.RemoveRange(dbContext.CalculatorRuns);
            this.dbContext.SaveChanges();
            this.dbContext.CalculatorRuns.Add(new CalculatorRun { Id = runId, Name = "Test Run 01", Financial_Year = calculatorRunFinancialYear });
            this.dbContext.SaveChanges();

            // Act
            await this.classificationService.UpdateRunClassification(runId, runClassification);

            // Assert
            var calculatorRun = this.dbContext.CalculatorRuns.FirstOrDefault(run => run.Id == runId);
            Assert.AreEqual(calculatorRun?.CalculatorRunClassificationId, (int)runClassification);
        }
    }
}
