using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

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
        
    }

}
