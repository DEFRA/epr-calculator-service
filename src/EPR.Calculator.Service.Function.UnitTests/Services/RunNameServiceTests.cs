namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    using System.Threading.Tasks;
    using EPR.Calculator.Service.Common.Logging;
    using EPR.Calculator.Service.Function.Data;
    using EPR.Calculator.Service.Function.Data.DataModels;
    using EPR.Calculator.Service.Function.Services;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Unit tests for the <see cref="RunNameService"/> class.
    /// </summary>
    [TestClass]
    public class RunNameServiceTests
    {
        private Mock<IDbContextFactory<ApplicationDBContext>> dbContextFactory;
        private ApplicationDBContext dbContext;
        private RunNameService runNameService;

        RunNameServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            this.dbContextFactory = new Mock<IDbContextFactory<ApplicationDBContext>>();
            this.dbContext = new ApplicationDBContext(options);
            this.dbContextFactory.Setup(factory => factory.CreateDbContext()).Returns(this.dbContext);

            this.runNameService = new RunNameService(
                this.dbContextFactory.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            this.dbContext?.Dispose();
        }

        /// <summary>
        /// Tests that <see cref="RunNameService.GetRunNameAsync(int)"/> returns the run name when the run exists.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [TestMethod]
        public async Task GetRunNameAsync_ShouldReturnRunNameWhenRunExists()
        {
            // Arrange
            var runId = 1;
            var expectedRunName = "Test Run Name";

            this.dbContext.CalculatorRuns.Add(new CalculatorRun { Id = runId, Name = expectedRunName, Financial_Year = "2024-25" });
            await this.dbContext.SaveChangesAsync();

            // Act
            var result = await this.runNameService.GetRunNameAsync(runId);

            // Assert
            Assert.AreEqual(expectedRunName, result);
        }

        /// <summary>
        /// Tests that <see cref="RunNameService.GetRunNameAsync(int)"/> returns null when the run does not exist.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [TestMethod]
        public async Task GetRunNameAsync_ShouldReturnNullWhenRunDoesNotExist()
        {
            // Arrange
            var runId = 10;

            // Act
            var result = await this.runNameService.GetRunNameAsync(runId);

            // Assert
            Assert.AreEqual(null, result);
        }
    }
}