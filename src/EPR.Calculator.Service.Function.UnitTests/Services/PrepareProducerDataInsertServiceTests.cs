namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using EPR.Calculator.Service.Common.Logging;
    using EPR.Calculator.Service.Function.Interface;
    using EPR.Calculator.Service.Function.Models;
    using EPR.Calculator.Service.Function.Services;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class PrepareProducerDataInsertServiceTests
    {
        public PrepareProducerDataInsertServiceTests()
        {
            _billingInstructionService = new Mock<IBillingInstructionService>();
            _producerInvoiceNetTonnageService = new Mock<IProducerInvoiceNetTonnageService>();
            _telemetryLogger = new Mock<ICalculatorTelemetryLogger>();
            _configurationService = new Mock<IConfigurationService>();
            _configurationService.Setup(c => c.PrepareCalcResultsTimeout).Returns(TimeSpan.FromMinutes(5));
            this.testClass = new PrepareProducerDataInsertService(_billingInstructionService.Object, _producerInvoiceNetTonnageService.Object, _telemetryLogger.Object, _configurationService.Object);
        }

        private PrepareProducerDataInsertService testClass { get; init; }
        private Mock<IBillingInstructionService> _billingInstructionService { get; init; }
        private Mock<IProducerInvoiceNetTonnageService> _producerInvoiceNetTonnageService { get; init; }
        private Mock<ICalculatorTelemetryLogger> _telemetryLogger { get; init; }
        private Mock<IConfigurationService> _configurationService { get; init; }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new PrepareProducerDataInsertService(_billingInstructionService.Object, _producerInvoiceNetTonnageService.Object, _telemetryLogger.Object, _configurationService.Object);

            // Assert
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public async Task CanCallInsertProducerDataToDatabase()
        {
            var fixture = new Fixture();
            var calcResult = fixture.Create<CalcResult>();
            _telemetryLogger.Setup(mock => mock.LogInformation(It.IsAny<TrackMessage>())).Verifiable();

            // Act
            var result = await testClass.InsertProducerDataToDatabase(calcResult);
            _telemetryLogger.Verify(mock => mock.LogInformation(It.IsAny<TrackMessage>()));
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task CanCallInsertProducerDataToDatabaseWith()
        {
            var fixture = new Fixture();
            var calcResult = fixture.Create<CalcResult>();
            _telemetryLogger.Setup(mock => mock.LogInformation(It.IsAny<TrackMessage>())).Verifiable();
            _billingInstructionService.Setup(m => m.CreateBillingInstructions(It.IsAny<CalcResult>())).ReturnsAsync(true);
            _producerInvoiceNetTonnageService.Setup(m => m.CreateProducerInvoiceNetTonnage(It.IsAny<CalcResult>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            // Act
            var result = await testClass.InsertProducerDataToDatabase(calcResult);

            // Assert
            _telemetryLogger.Verify(mock => mock.LogInformation(It.IsAny<TrackMessage>()));

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task CannotCallInsertProducerDataToDatabaseWithNullCalcResult()
        {
            var fixture = new Fixture();
            var calcResult = fixture.Create<CalcResult>();

            _telemetryLogger.Setup(mock => mock.LogInformation(It.IsAny<TrackMessage>())).Verifiable();
            _billingInstructionService.Setup(m => m.CreateBillingInstructions(It.IsAny<CalcResult>())).ThrowsAsync(new Exception());
            _producerInvoiceNetTonnageService.Setup(m => m.CreateProducerInvoiceNetTonnage(It.IsAny<CalcResult>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            // Act
            var result = await testClass.InsertProducerDataToDatabase(calcResult);

            // Assert
            _telemetryLogger.Verify(mock => mock.LogError(It.IsAny<ErrorMessage>()));
            Assert.IsFalse(result);
        }
    }
}