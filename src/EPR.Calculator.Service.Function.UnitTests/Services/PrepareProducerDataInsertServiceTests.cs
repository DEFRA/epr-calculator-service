using AutoFixture;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using Microsoft.Extensions.Logging;
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
            _logger = new Mock<ILogger<PrepareProducerDataInsertService>>();
            testClass = new PrepareProducerDataInsertService(_billingInstructionService.Object, _producerInvoiceNetTonnageService.Object, _logger.Object);
        }

        private PrepareProducerDataInsertService testClass { get; init; }
        private Mock<IBillingInstructionService> _billingInstructionService { get; init; }
        private Mock<IProducerInvoiceNetTonnageService> _producerInvoiceNetTonnageService { get; init; }
        private Mock<ILogger<PrepareProducerDataInsertService>> _logger { get; init; }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new PrepareProducerDataInsertService(_billingInstructionService.Object, _producerInvoiceNetTonnageService.Object, _logger.Object);

            // Assert
            Assert.IsNotNull(instance);
        }


        [TestMethod]
        public async Task CanCallInsertProducerDataToDatabase()
        {
            // Arrange
            var fixture = new Fixture();
            var calcResult = fixture.Create<CalcResult>();

            // Act
            var result = await testClass.InsertProducerDataToDatabase(calcResult);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task CanCallInsertProducerDataToDatabaseWith()
        {
            // Arrange
            var fixture = new Fixture();
            var calcResult = fixture.Create<CalcResult>();
            _billingInstructionService.Setup(m => m.CreateBillingInstructions(It.IsAny<CalcResult>())).ReturnsAsync(true);
            _producerInvoiceNetTonnageService.Setup(m => m.CreateProducerInvoiceNetTonnage(It.IsAny<CalcResult>())).ReturnsAsync(true);

            // Act
            var result = await testClass.InsertProducerDataToDatabase(calcResult);

            // Assert
            Assert.IsTrue(result);
        }

       
        [TestMethod]
        public async Task CannotCallInsertProducerDataToDatabaseWithNullCalcResult()

        {
            // Arrange
            var fixture = new Fixture();
            var calcResult = fixture.Create<CalcResult>();
            _billingInstructionService.Setup(m => m.CreateBillingInstructions(It.IsAny<CalcResult>())).ThrowsAsync(new Exception());
            _producerInvoiceNetTonnageService.Setup(m => m.CreateProducerInvoiceNetTonnage(It.IsAny<CalcResult>())).ReturnsAsync(true);

            // Act
            var result = await testClass.InsertProducerDataToDatabase(calcResult);

            // Assert
            Assert.IsFalse(result);
        }
    }
}