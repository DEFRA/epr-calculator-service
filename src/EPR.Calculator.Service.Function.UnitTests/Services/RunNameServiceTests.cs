namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    using System.Threading.Tasks;
    using EPR.Calculator.Service.Common.Logging;
    using EPR.Calculator.Service.Function.Data;
    using EPR.Calculator.Service.Function.Data.DataModels;
    using EPR.Calculator.Service.Function.Interface;
    using EPR.Calculator.Service.Function.Models;
    using EPR.Calculator.Service.Function.Services;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Unit tests for the <see cref="RunNameService"/> class.
    /// </summary>
    [TestClass]
    public class RunNameServiceTests
    {
        private Mock<ICalculatorTelemetryLogger> mockTelemetryLogger;
        private Mock<IDbContextFactory<ApplicationDBContext>> dbContextFactory;
        private ApplicationDBContext dbContext;
        private RunNameService runNameService;

        /// <summary>
        /// Initializes the test setup.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            this.mockTelemetryLogger = new Mock<ICalculatorTelemetryLogger>();

            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            this.dbContextFactory = new Mock<IDbContextFactory<ApplicationDBContext>>();
            this.dbContext = new ApplicationDBContext(options);
            this.dbContextFactory.Setup(factory => factory.CreateDbContext()).Returns(this.dbContext);

            this.runNameService = new RunNameService(
                this.dbContextFactory.Object,
                this.mockTelemetryLogger.Object);
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

        /// <summary>
        /// Tests that <see cref="RunNameService.GetRunNameAsync(int)"/> logs an error when an exception is thrown.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [TestMethod]
        public async Task GetRunNameAsync_ShouldLogErrorWhenExceptionIsThrown()
        {
            // Arrange
            var runId = 1;
            var exception = new Exception("Test Exception");
            this.dbContextFactory.Setup(factory => factory.CreateDbContext()).Throws(exception);

            // Act
            var result = await this.runNameService.GetRunNameAsync(runId);

            // Assert
            Assert.IsNull(result);
        }
    }

    /// <summary>
    /// Factory class for creating instances of <see cref="ApplicationDBContext"/>.
    /// </summary>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    public class DbContextFactory<TContext> : IDbContextFactory<TContext>
        where TContext : DbContext
    {
        private readonly DbContextOptions<TContext> options;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbContextFactory{TContext}"/> class.
        /// </summary>
        /// <param name="options">The options to be used by the DbContext.</param>
        public DbContextFactory(DbContextOptions<TContext> options)
        {
            this.options = options;
        }

        /// <summary>
        /// Creates a new instance of the DbContext.
        /// </summary>
        /// <returns>A new instance of the DbContext.</returns>
        public TContext CreateDbContext()
        {
            return (TContext)Activator.CreateInstance(typeof(TContext), this.options);
        }
    }
}