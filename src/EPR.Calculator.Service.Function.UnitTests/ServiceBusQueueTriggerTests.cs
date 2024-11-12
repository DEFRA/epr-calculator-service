// <copyright file="ServiceBusQueueTriggerTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace EPR.Calculator.Service.Function.UnitTests
{
    using EPR.Calculator.Service.Common;
    using EPR.Calculator.Service.Common.AzureSynapse;
    using EPR.Calculator.Service.Function.Interface;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Newtonsoft.Json;

    [TestClass]
    public class ServiceBusQueueTriggerTests
    {
        private readonly ServiceBusQueueTrigger function;
        private readonly Mock<ICalculatorRunService> calculatorRunService;
        private readonly Mock<ICalculatorRunParameterMapper> parameterMapper;
        private readonly Mock<ILogger> mockLogger;
        private readonly Mock<IAzureSynapseRunner> azureSynapseRunner;

        public ServiceBusQueueTriggerTests()
        {
            this.calculatorRunService = new Mock<ICalculatorRunService>();
            this.parameterMapper = new Mock<ICalculatorRunParameterMapper>();
            this.azureSynapseRunner = new Mock<IAzureSynapseRunner>();
            this.function = new ServiceBusQueueTrigger(
                this.calculatorRunService.Object,
                this.parameterMapper.Object,
                this.azureSynapseRunner.Object);
            this.mockLogger = new Mock<ILogger>();
        }

        [TestMethod]
        public void CanCallRun()
        {
            // Arrange
            var myQueueItem = @"{ CalculatorRunId: 678767, FinancialYear: '2024-25', CreatedBy: 'Test user'}";
            var processedParameterData = new CalculatorRunParameter() { FinancialYear = "2024-25", User = "Test user", Id = 678767 };

            this.parameterMapper.Setup(t => t.Map(JsonConvert.DeserializeObject<CalculatorParameter>(myQueueItem))).Returns(processedParameterData);
            var log = new Mock<ILogger>().Object;

            this.function.Run(myQueueItem, log);

            this.calculatorRunService.Verify(
                p => p.StartProcess(
                    It.Is<CalculatorRunParameter>(msg => msg == processedParameterData)),
                Times.Never);
        }

        [TestMethod]
        public void ServiceBusTrigger_Message_Empty()
        {
            // Arrange
            var myQueueItem = string.Empty;
            this.function.Run(myQueueItem, this.mockLogger.Object);

            this.mockLogger.Verify(
                log => log.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Message is Null or empty")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [TestMethod]
        public void ServiceBusTrigger_Message_Notvalid_Json_Exception()
        {
            // Arrange
            var myQueueItem = @"{ CalculatorRunId: 678767, FinancialYear: '2024-25', CreatedBy: 'Test user'}";

            this.parameterMapper.Setup(t => t.Map(JsonConvert.DeserializeObject<CalculatorParameter>(myQueueItem))).Throws<JsonException>();
            this.function.Run(myQueueItem, this.mockLogger.Object);

            this.mockLogger.Verify(
                log => log.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Incorrect format")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Never);
        }

        [TestMethod]
        public void ServiceBusTrigger_Message_Notvalid_Exception()
        {
            // Arrange
            var myQueueItem = @"{ CalculatorRunId: 678767, FinancialYear: '2024-25', CreatedBy: 'Test user'}";

            this.parameterMapper.Setup(t => t.Map(JsonConvert.DeserializeObject<CalculatorParameter>(myQueueItem))).Throws<Exception>();
            this.function.Run(myQueueItem, this.mockLogger.Object);

            this.mockLogger.Verify(
                log => log.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Never);
        }
    }
}
