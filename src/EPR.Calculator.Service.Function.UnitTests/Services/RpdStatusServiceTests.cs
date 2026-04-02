using AutoFixture;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    [TestClass]
    public class RpdStatusServiceTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RpdStatusServiceTests"/> class.
        /// </summary>
        public RpdStatusServiceTests()
        {
            Fixture = new Fixture();

            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            Context = new ApplicationDBContext(dbContextOptions);
            SetupRunClassifications();
            Context.SaveChanges();
            var contextFactory = new Mock<IDbContextFactory<ApplicationDBContext>>();
            contextFactory.Setup(f => f.CreateDbContext()).Returns(Context);

            Validator = new Mock<IRpdStatusDataValidator>();
            Validator.Setup(v => v.IsValidSuccessfulRun(It.IsAny<int>()))
                .Returns(new RpdStatusValidation { isValid = true });
            Validator.Setup(v => v.IsValidRun(
                It.IsAny<CalculatorRun>(),
                It.IsAny<int>(),
                It.IsAny<IEnumerable<CalculatorRunClassification>>()))
                .Returns(new RpdStatusValidation { isValid = true });

            CommandTimeoutService = new Mock<ICommandTimeoutService>();

            Configuration = new Mock<IConfigurationService>();
            Configuration.Setup(s => s.ResultFileCSVContainerName)
                .Returns(Fixture.Create<string>());
            Logger = new Mock<ILogger<RpdStatusService>>();

            CalculatorRunOrgData = new Mock<ICalculatorRunOrgData>();

            CalculatorRunPomData = new Mock<ICalculatorRunPomData>();

            TestClass = new RpdStatusService(
                Configuration.Object,
                contextFactory.Object,
                CommandTimeoutService.Object,
                Validator.Object,
                Logger.Object,
                CalculatorRunOrgData.Object,
                CalculatorRunPomData.Object);
        }

        private RpdStatusService TestClass { get; init; }

        private IFixture Fixture { get; init; }

        private ApplicationDBContext Context { get; init; }

        private Mock<IConfigurationService> Configuration { get; init; }

        private Mock<IRpdStatusDataValidator> Validator { get; init; }

        private Mock<ICommandTimeoutService> CommandTimeoutService { get; set; }

        private Mock<ILogger<RpdStatusService>> Logger { get; init; }

        private Mock<ICalculatorRunOrgData> CalculatorRunOrgData { get; init; }

        private Mock<ICalculatorRunPomData> CalculatorRunPomData { get; init; }

        private void SetupRunClassifications()
        {
            Context.CalculatorRunClassifications
                 .Add(new CalculatorRunClassification
                 {
                     Id = (int)RunClassification.RUNNING,
                     Status = RunClassification.RUNNING.ToString(),
                 });
            Context.CalculatorRunClassifications
                .Add(new CalculatorRunClassification
                {
                    Id = (int)RunClassification.ERROR,
                    Status = RunClassification.ERROR.ToString(),
                });
        }

        [TestMethod]
        public async Task UpdateRpdStatus_With_RunId_When_Successful()
        {
            // Arrange
            var runId = Fixture.Create<int>();
            var run = Fixture.Create<CalculatorRun>();
            run.Id = runId;
            run.RelativeYear = Fixture.Create<RelativeYear>();
            Context.CalculatorRuns.Add(run);
            await Context.SaveChangesAsync();

            CalculatorRunOrgData.Setup(s => s.LoadOrgDataForCalcRun(It.IsAny<int>(), It.IsAny<RelativeYear>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            CalculatorRunPomData.Setup(s => s.LoadPomDataForCalcRun(It.IsAny<int>(), It.IsAny<RelativeYear>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await TestClass.UpdateRpdStatus(
                runId,
                Fixture.Create<string>(),
                Fixture.Create<string>(),
                CancellationToken.None);

            // Assert
            var calcRun = await Context.CalculatorRuns.SingleAsync(x => x.Id == runId);
            Assert.IsNotNull(calcRun);
            Assert.AreEqual((int)RunClassification.RUNNING, calcRun.CalculatorRunClassificationId);
            CalculatorRunOrgData.Verify(s => s.LoadOrgDataForCalcRun(runId, run.RelativeYear, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            CalculatorRunPomData.Verify(s => s.LoadPomDataForCalcRun(runId, run.RelativeYear, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task UpdateRpdStatus_IsNotValidRun()
        {
            // Arrange
            var runId = Fixture.Create<int>();
            var run = Fixture.Create<CalculatorRun>();
            run.Id = runId;
            run.RelativeYear = Fixture.Create<RelativeYear>();
            Context.CalculatorRuns.Add(run);
            await Context.SaveChangesAsync();

            Validator.Setup(v => v.IsValidRun(
                It.IsAny<CalculatorRun>(),
                It.IsAny<int>(),
                It.IsAny<IEnumerable<CalculatorRunClassification>>()))
                .Returns(new RpdStatusValidation { isValid = false });

            // Act
            Exception? result = null;
            try
            {
                await TestClass.UpdateRpdStatus(
                    runId,
                    Fixture.Create<string>(),
                    Fixture.Create<string>(),
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                result = ex;
            }

            // Assert
            Assert.IsInstanceOfType<ValidationException>(result);
        }

        [TestMethod]
        public async Task UpdateRpdStatus_IsNotValidSuccessfulRun()
        {
            // Arrange
            var runId = Fixture.Create<int>();
            var run = Fixture.Create<CalculatorRun>();
            run.Id = runId;
            run.RelativeYear = Fixture.Create<RelativeYear>();
            Context.CalculatorRuns.Add(run);
            await Context.SaveChangesAsync();

            Validator.Setup(v => v.IsValidSuccessfulRun(It.IsAny<int>()))
                .Returns(new RpdStatusValidation { isValid = false });

            // Act
            Exception? result = null;
            try
            {
                await TestClass.UpdateRpdStatus(
                    runId,
                    Fixture.Create<string>(),
                    Fixture.Create<string>(),
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                result = ex;
            }

            // Assert
            Assert.IsInstanceOfType<ValidationException>(result);
        }

        [TestMethod]
        public async Task UpdateRpdStatus_DataThrowsException()
        {
            // Arrange
            var runId = Fixture.Create<int>();
            var run = Fixture.Create<CalculatorRun>();
            run.Id = runId;
            run.RelativeYear = Fixture.Create<RelativeYear>();
            Context.CalculatorRuns.Add(run);
            await Context.SaveChangesAsync();

            CalculatorRunOrgData.Setup(s => s.LoadOrgDataForCalcRun(It.IsAny<int>(), It.IsAny<RelativeYear>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();
            CalculatorRunPomData.Setup(s => s.LoadPomDataForCalcRun(It.IsAny<int>(), It.IsAny<RelativeYear>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            Exception? result = null;
            try
            {
                await TestClass.UpdateRpdStatus(
                    runId,
                    Fixture.Create<string>(),
                    Fixture.Create<string>(),
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