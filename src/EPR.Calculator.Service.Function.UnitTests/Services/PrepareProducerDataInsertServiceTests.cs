using AutoFixture;
using EPR.Calculator.Service.Common.Logging;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using Moq;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    [TestClass]
    public class PrepareProducerDataInsertServiceTests
    {
     

        
        public PrepareProducerDataInsertServiceTests()
        {
            _billingInstructionService = new Mock<IBillingInstructionService>();
            _producerInvoiceNetTonnageService = new Mock<IProducerInvoiceNetTonnageService>();
            _telemetryLogger = new Mock<ICalculatorTelemetryLogger>();
            testClass = new PrepareProducerDataInsertService(_billingInstructionService.Object, _producerInvoiceNetTonnageService.Object, _telemetryLogger.Object);
        }

        private PrepareProducerDataInsertService testClass { get; init; }
        private Mock<IBillingInstructionService> _billingInstructionService { get; init; }
        private Mock<IProducerInvoiceNetTonnageService> _producerInvoiceNetTonnageService { get; init; }
        private Mock<ICalculatorTelemetryLogger> _telemetryLogger { get; init; }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new PrepareProducerDataInsertService(_billingInstructionService.Object, _producerInvoiceNetTonnageService.Object, _telemetryLogger.Object);

            // Assert
            Assert.IsNotNull(instance);
        }


        [TestMethod]
        public async Task CanCallInsertProducerDataToDatabase()
        {
            // Arrange
            var fixture = new Fixture().Customize(new ImmutableCollectionsCustomization());;
            var calcResult = fixture.Create<CalcResult>();

            _telemetryLogger.Setup(mock => mock.LogInformation(It.IsAny<TrackMessage>())).Verifiable();

            // Act
            var result = await testClass.InsertProducerDataToDatabase(calcResult);

            // Assert
            _telemetryLogger.Verify(mock => mock.LogInformation(It.IsAny<TrackMessage>()));

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task CanCallInsertProducerDataToDatabaseWith()
        {
            // Arrange
            var fixture = new Fixture().Customize(new ImmutableCollectionsCustomization());;
            var calcResult = fixture.Create<CalcResult>();

            _telemetryLogger.Setup(mock => mock.LogInformation(It.IsAny<TrackMessage>())).Verifiable();
            _billingInstructionService.Setup(m => m.CreateBillingInstructions(It.IsAny<CalcResult>())).ReturnsAsync(true);
            _producerInvoiceNetTonnageService.Setup(m => m.CreateProducerInvoiceNetTonnage(It.IsAny<CalcResult>())).ReturnsAsync(true);

            // Act
            var result = await testClass.InsertProducerDataToDatabase(calcResult);

            // Assert
            _telemetryLogger.Verify(mock => mock.LogInformation(It.IsAny<TrackMessage>()));

            Assert.IsTrue(result);
        }

       
        [TestMethod]
        public async Task CannotCallInsertProducerDataToDatabaseWithNullCalcResult()

        {
            // Arrange
            var fixture = new Fixture().Customize(new ImmutableCollectionsCustomization());;
            var calcResult = fixture.Create<CalcResult>();

            _telemetryLogger.Setup(mock => mock.LogInformation(It.IsAny<TrackMessage>())).Verifiable();
            _billingInstructionService.Setup(m => m.CreateBillingInstructions(It.IsAny<CalcResult>())).ThrowsAsync(new Exception());
            _producerInvoiceNetTonnageService.Setup(m => m.CreateProducerInvoiceNetTonnage(It.IsAny<CalcResult>())).ReturnsAsync(true);

            // Act
            var result = await testClass.InsertProducerDataToDatabase(calcResult);

            // Assert
            _telemetryLogger.Verify(mock => mock.LogError(It.IsAny<ErrorMessage>()));

            Assert.IsFalse(result);

        }
    }
}