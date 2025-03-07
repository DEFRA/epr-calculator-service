// <copyright file="ServiceBusQueueTriggerTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace EPR.Calculator.Service.Function.UnitTests
{
    using EPR.Calculator.Service.Common;
    using EPR.Calculator.Service.Function.Interface;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Moq.Protected;
    using Newtonsoft.Json;

    /// <summary>
    /// Unit Test class for service bus queue trigger.
    /// </summary>
    [TestClass]
    public class ServiceBusQueueTriggerTests
    {
        private readonly ServiceBusQueueTrigger function;
        private readonly Mock<ICalculatorRunService> calculatorRunService;
        private readonly Mock<ICalculatorRunParameterMapper> parameterMapper;
        private readonly Mock<IRunNameService> runNameService;
        private readonly Mock<ILogger> mockLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusQueueTriggerTests"/> class.
        /// </summary>
        public ServiceBusQueueTriggerTests()
        {
            this.calculatorRunService = new Mock<ICalculatorRunService>();
            this.parameterMapper = new Mock<ICalculatorRunParameterMapper>();
            this.runNameService = new Mock<IRunNameService>();
            this.function = new ServiceBusQueueTrigger(
                this.calculatorRunService.Object,
                this.parameterMapper.Object,
                this.runNameService.Object);
            this.mockLogger = new Mock<ILogger>();
        }

        /// <summary>
        /// Tests the Run method with a valid message to ensure it processes the queue item correctly and returns true.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task ServiceBusTrigger_Valid_Message_Run()
        {
            // Arrange
            var myQueueItem = @"{ CalculatorRunId: 678767, FinancialYear: '2024-25', CreatedBy: 'Test user'}";
            var processedParameterData = new CalculatorRunParameter() { FinancialYear = "2024-25", User = "Test user", Id = 678767 };
            var runName = "Test Run Name";

            this.runNameService.Setup(t => t.GetRunNameAsync(It.IsAny<int>())).ReturnsAsync(runName);
            this.parameterMapper.Setup(t => t.Map(It.IsAny<CalculatorParameter>())).Returns(processedParameterData);
            this.calculatorRunService.Setup(t => t.StartProcess(It.IsAny<CalculatorRunParameter>(), It.IsAny<string>())).ReturnsAsync(true);

            var mockHttpHandler = new Mock<HttpMessageHandler>();

            mockHttpHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
                {
                    HttpResponseMessage response = new HttpResponseMessage()
                    { StatusCode = System.Net.HttpStatusCode.OK };

                    return response;
                });

            var httpClient = new HttpClient(mockHttpHandler.Object)
            {
                BaseAddress = new Uri("http://google.com"),
            };

            // Act
            await this.function.Run(myQueueItem, this.mockLogger.Object);

            // Assert
            this.calculatorRunService.Verify(
                p => p.StartProcess(
                    It.Is<CalculatorRunParameter>(msg => msg == processedParameterData),
                    It.Is<string>(name => name == runName)),
                Times.Once);

            // Optionally, verify logging if needed
            this.mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Executing the function app started")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        /// <summary>
        /// Tests the Run method to ensure it logs an error and returns false when the message is null or empty.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task ServiceBusTrigger_Empty_Message_Run()
        {
            // Arrange
            var myQueueItem = string.Empty;

            // Act
            await this.function.Run(myQueueItem, this.mockLogger.Object);

            // Assert
            this.mockLogger.Verify(
                log => log.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Message is null or empty")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests the Run method to ensure it logs an error and returns false when mapper throw json exception .
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task ServiceBusTrigger_Message_Json_Exception()
        {
            // Arrange
            var myQueueItem = @"{ CalculatorRunId: 678767, FinancialYear: '2024-25', CreatedBy: 'Test user'}";
            this.parameterMapper.Setup(t => t.Map(It.IsAny<CalculatorParameter>())).Throws<JsonException>();

            // Act
            await this.function.Run(myQueueItem, this.mockLogger.Object);

            // Assert
            this.mockLogger.Verify(
                log => log.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Incorrect format")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests the Run method to ensure it logs an error and returns false when mapper throw unhandled exception.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task ServiceBusTrigger_Message_Mapper_Unhandled_Exception()
        {
            // Arrange
            var myQueueItem = @"{ CalculatorRunId: 678767, FinancialYear: '2024-25', CreatedBy: 'Test user'}";
            this.parameterMapper.Setup(t => t.Map(It.IsAny<CalculatorParameter>())).Throws<Exception>();

            // Act
            await this.function.Run(myQueueItem, this.mockLogger.Object);

            // Assert
            this.mockLogger.Verify(
                log => log.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests the Run method to ensure it logs an error and returns false when startprocess throw unhandled exception.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task ServiceBusTrigger_Invalid_Message_Run_Unhandled_Exception()
        {
            // Arrange
            var myQueueItem = @"{ CalculatorRunId: 678767, FinancialYear: '2024-25', CreatedBy: 'Test user'}";
            var processedParameterData = new CalculatorRunParameter() { FinancialYear = "2024-25", User = "Test user", Id = 678767 };
            var runName = "Test Run Name";

            this.parameterMapper.Setup(t => t.Map(It.IsAny<CalculatorParameter>())).Returns(processedParameterData);
            this.runNameService.Setup(t => t.GetRunNameAsync(It.IsAny<int>())).ReturnsAsync(runName);

            // Setup StartProcess to throw an exception
            this.calculatorRunService.Setup(t => t.StartProcess(It.IsAny<CalculatorRunParameter>(), It.IsAny<string>())).ThrowsAsync(new Exception("Unhandled exception"));

            // Act
            await this.function.Run(myQueueItem, this.mockLogger.Object);

            // Assert
            this.mockLogger.Verify(
                log => log.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unhandled exception")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
