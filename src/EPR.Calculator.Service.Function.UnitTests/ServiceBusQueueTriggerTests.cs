using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.UnitTests
{
    [TestClass]
    public class ServiceBusQueueTriggerTests
    {
        private readonly ServiceBusQueueTrigger _function;
        private readonly ICalculatorRunService _calculatorRunService;
        private readonly Mock<ILogger> _mockLogger;

        public ServiceBusQueueTriggerTests()
        {

            _calculatorRunService = new CalculatorRunService();
            _function = new ServiceBusQueueTrigger(_calculatorRunService);
            _mockLogger = new Mock<ILogger>();
        }


        [TestMethod]
        public void Run_ReturnsOkResult_WhenMessageIsSent()
        {
            var message = @"{ CalculatorRunId: 678767, FinancialYear: '2024-25', CreatedBy: 'Test user'}";

            var result = _function.Run(message, _mockLogger.Object);

            Assert.IsInstanceOfType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.AreEqual(message, okResult?.Value);
        }

        [TestMethod]
        public void Run_ReturnsOkResult_WhenMessageIsEMpty()
        {
            var message = string.Empty;

            var result = _function.Run(message, _mockLogger.Object);
            Assert.IsInstanceOfType<BadRequestResult>(result);
        }
    }

}
