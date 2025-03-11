// <copyright file="ServiceBusQueueTriggerTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace EPR.Calculator.Service.Function.UnitTests
{
    using EPR.Calculator.Service.Common;
    using EPR.Calculator.Service.Common.Logging;
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
        private readonly Mock<ICalculatorTelemetryLogger> telemetryLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusQueueTriggerTests"/> class.
        /// </summary>
        public ServiceBusQueueTriggerTests()
        {
            this.calculatorRunService = new Mock<ICalculatorRunService>();
            this.parameterMapper = new Mock<ICalculatorRunParameterMapper>();
            this.runNameService = new Mock<IRunNameService>();
            this.telemetryLogger = new Mock<ICalculatorTelemetryLogger>();
            this.function = new ServiceBusQueueTrigger(
                this.calculatorRunService.Object,
                this.parameterMapper.Object,
                this.runNameService.Object,
                this.telemetryLogger.Object);
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
            await this.function.Run(myQueueItem);

            // Assert
            this.calculatorRunService.Verify(
                p => p.StartProcess(
                    It.Is<CalculatorRunParameter>(msg => msg == processedParameterData),
                    It.Is<string>(name => name == runName)),
                Times.Once);

            // Optionally, verify logging if needed
            this.telemetryLogger.Verify(
                x => x.LogInformation(It.Is<TrackMessage>(log =>
                    log.Message.Contains("Executing the function app started"))),
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
            await this.function.Run(myQueueItem);

            // Assert
            this.telemetryLogger.Verify(
                log => log.LogError(It.Is<ErrorMessage>(msg =>
                    msg.Message.Contains("Message is null or empty") &&
                    msg.Exception is ArgumentNullException)),
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
            await this.function.Run(myQueueItem);

            // Assert
            this.telemetryLogger.Verify(
                log => log.LogError(It.Is<ErrorMessage>(msg =>
                    msg.Message.Contains("Incorrect format") &&
                    msg.Exception is JsonException)),
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
            await this.function.Run(myQueueItem);

            // Assert
            this.telemetryLogger.Verify(
                log => log.LogError(It.Is<ErrorMessage>(msg =>
                    msg.Message.Contains("Error") &&
                    msg.Exception is Exception)),
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
            await this.function.Run(myQueueItem);

            // Assert
            this.telemetryLogger.Verify(
                log => log.LogError(It.Is<ErrorMessage>(msg =>
                    msg.Message.Contains("Unhandled exception") &&
                    msg.Exception is Exception)),
                Times.Once);
        }
    }
}