namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.Service.Function.Enums;
    using EPR.Calculator.Service.Function.Interface;
    using EPR.Calculator.Service.Function.Models;
    using EPR.Calculator.Service.Function.Services;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class RpdStatusServiceTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RpdStatusServiceTests"/> class.
        /// </summary>
        public RpdStatusServiceTests()
        {
            this.Fixture = new Fixture();

            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            this.Context = new ApplicationDBContext(dbContextOptions);
            this.SetupRunClassifications();
            this.Context.SaveChanges();
            var contextFactory = new Mock<IDbContextFactory<ApplicationDBContext>>();
            contextFactory.Setup(f => f.CreateDbContext()).Returns(this.Context);

            this.Validator = new Mock<IRpdStatusDataValidator>();
            this.Validator.Setup(v => v.IsValidSuccessfulRun(It.IsAny<int>()))
                .Returns(new RpdStatusValidation { isValid = true });
            this.Validator.Setup(v => v.IsValidRun(
                It.IsAny<CalculatorRun>(),
                It.IsAny<int>(),
                It.IsAny<IEnumerable<CalculatorRunClassification>>()))
                .Returns(new RpdStatusValidation { isValid = true });

            this.Wrapper = new Mock<IOrgAndPomWrapper>();
            this.CommandTimeoutService = new Mock<ICommandTimeoutService>();

            this.Configuration = new Mock<IConfigurationService>();
            this.Configuration.Setup(s => s.BlobContainerName)
                .Returns(this.Fixture.Create<string>());

            this.TestClass = new RpdStatusService(
                this.Configuration.Object,
                contextFactory.Object,
                this.CommandTimeoutService.Object,
                this.Validator.Object,
                this.Wrapper.Object);
        }

        private RpdStatusService TestClass { get; init; }

        private IFixture Fixture { get; init; }

        private ApplicationDBContext Context { get; init; }

        private Mock<IConfigurationService> Configuration { get; init; }

        private Mock<IRpdStatusDataValidator> Validator { get; init; }

        private Mock<IOrgAndPomWrapper> Wrapper { get; init; }

        private Mock<ICommandTimeoutService> CommandTimeoutService { get; set; }

        private void SetupRunClassifications()
        {
            this.Context.CalculatorRunClassifications
                 .Add(new CalculatorRunClassification
                 {
                     Id = (int)RunClassification.RUNNING,
                     Status = RunClassification.RUNNING.ToString(),
                 });
            this.Context.CalculatorRunClassifications
                .Add(new CalculatorRunClassification
                {
                    Id = (int)RunClassification.ERROR,
                    Status = RunClassification.ERROR.ToString(),
                });
        }

        [TestMethod]
        public async Task UpdateRpdStatus_With_RunId_When_Not_Successful()
        {
            // Arrange
            var runId = this.Fixture.Create<int>();
            var run = this.Fixture.Create<CalculatorRun>();
            run.Id = runId;
            this.Context.CalculatorRuns.Add(run);
            await this.Context.SaveChangesAsync();

            // Act
            await this.TestClass.UpdateRpdStatus(
                runId,
                this.Fixture.Create<string>(),
                false,
                CancellationToken.None);

            // Assert
            var updatedRun = await this.Context.CalculatorRuns.SingleAsync(x => x.Id == runId);
            Assert.IsNotNull(updatedRun);
            Assert.AreEqual((int)RunClassification.ERROR, updatedRun.CalculatorRunClassificationId);
        }

        [TestMethod]
        public async Task UpdateRpdStatus_With_RunId_When_Successful()
        {
            // Arrange
            var runId = this.Fixture.Create<int>();
            var run = this.Fixture.Create<CalculatorRun>();
            var financialYear = this.Fixture.Create<DateTime>();
            run.Id = runId;
            run.Financial_Year = financialYear.ToString("yyyy-yy");
            this.Context.CalculatorRuns.Add(run);
            await this.Context.SaveChangesAsync();

            // Act
            await this.TestClass.UpdateRpdStatus(
                runId,
                this.Fixture.Create<string>(),
                true,
                CancellationToken.None);

            // Assert
            var calcRun = await this.Context.CalculatorRuns.SingleAsync(x => x.Id == runId);
            var expectedCalendarYear = financialYear.AddYears(-1).ToString("yyyy");
            Assert.IsNotNull(calcRun);
            Assert.AreEqual((int)RunClassification.RUNNING, calcRun.CalculatorRunClassificationId);
            this.Wrapper.Verify(
                x => x.ExecuteSqlAsync(
                It.Is<FormattableString>(s => s.ToString().Contains($"calendarYear = {expectedCalendarYear}")),
                It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }

        [TestMethod]
        public async Task UpdateRpdStatus_IsNotValidRun()
        {
            // Arrange
            var runId = this.Fixture.Create<int>();
            var run = this.Fixture.Create<CalculatorRun>();
            run.Id = runId;
            run.Financial_Year = this.Fixture.Create<DateTime>().ToString("yyyy-yy");
            this.Context.CalculatorRuns.Add(run);
            await this.Context.SaveChangesAsync();

            this.Validator.Setup(v => v.IsValidRun(
                It.IsAny<CalculatorRun>(),
                It.IsAny<int>(),
                It.IsAny<IEnumerable<CalculatorRunClassification>>()))
                .Returns(new RpdStatusValidation { isValid = false });

            // Act
            Exception? result = null;
            try
            {
                await this.TestClass.UpdateRpdStatus(
                    runId,
                    this.Fixture.Create<string>(),
                    true,
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                result = ex;
            }

            // Assert
            Assert.IsInstanceOfType<FluentValidation.ValidationException>(result);
        }

        [TestMethod]
        public async Task UpdateRpdStatus_IsNotValidSuccessfulRun()
        {
            // Arrange
            var runId = this.Fixture.Create<int>();
            var run = this.Fixture.Create<CalculatorRun>();
            run.Id = runId;
            run.Financial_Year = this.Fixture.Create<DateTime>().ToString("yyyy-yy");
            this.Context.CalculatorRuns.Add(run);
            await this.Context.SaveChangesAsync();

            this.Validator.Setup(v => v.IsValidSuccessfulRun(It.IsAny<int>()))
                .Returns(new RpdStatusValidation { isValid = false });

            // Act
            Exception? result = null;
            try
            {
                await this.TestClass.UpdateRpdStatus(
                    runId,
                    this.Fixture.Create<string>(),
                    true,
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                result = ex;
            }

            // Assert
            Assert.IsInstanceOfType<FluentValidation.ValidationException>(result);
        }

        [TestMethod]
        public async Task UpdateRpdStatus_WrapperThrowsException()
        {
            // Arrange
            var runId = this.Fixture.Create<int>();
            var run = this.Fixture.Create<CalculatorRun>();
            run.Id = runId;
            run.Financial_Year = this.Fixture.Create<DateTime>().ToString("yyyy-yy");
            this.Context.CalculatorRuns.Add(run);
            await this.Context.SaveChangesAsync();

            this.Wrapper.Setup(w => w.ExecuteSqlAsync(
                It.IsAny<FormattableString>(),
                It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            // Act
            Exception? result = null;
            try
            {
                await this.TestClass.UpdateRpdStatus(
                    runId,
                    this.Fixture.Create<string>(),
                    true,
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                result = ex;
            }

            // Assert
            Assert.IsInstanceOfType<Exception>(result);
        }
    }
}