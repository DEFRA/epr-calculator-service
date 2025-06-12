// <copyright file="ServiceBusQueueTriggerTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace EPR.Calculator.Service.Function.UnitTests
{
    using EPR.Calculator.Service.Common;
    using EPR.Calculator.Service.Common.Logging;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Interface;
    using EPR.Calculator.Service.Function.Services;
    using Moq;
    using Newtonsoft.Json;
    using NuGet.Frameworks;

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
        private readonly Mock<IMessageTypeService> messageTypeService;
        private readonly Mock<IPrepareBillingFileService> prepareBillingFileService;


        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusQueueTriggerTests"/> class.
        /// </summary>
        public ServiceBusQueueTriggerTests()
        {
            this.calculatorRunService = new Mock<ICalculatorRunService>();
            this.parameterMapper = new Mock<ICalculatorRunParameterMapper>();
            this.runNameService = new Mock<IRunNameService>();
            this.messageTypeService = new Mock<IMessageTypeService>();
            this.telemetryLogger = new Mock<ICalculatorTelemetryLogger>();
            this.prepareBillingFileService = new Mock<IPrepareBillingFileService>();
            this.function = new ServiceBusQueueTrigger(
                this.calculatorRunService.Object,
                this.parameterMapper.Object,
                this.runNameService.Object,
                this.telemetryLogger.Object,
                this.messageTypeService.Object,
                this.prepareBillingFileService.Object);
        }

        /// <summary>
        /// Tests the Run method with a valid message to ensure it processes the queue item correctly and returns true.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task ServiceBusTrigger_ValidMessageRun()
        {
            // Arrange
            var myQueueItem = @"{ CalculatorRunId: 678767, FinancialYear: '2024-25', CreatedBy: 'Test user'}";
            var processedParameterData = new CalculatorRunParameter() { FinancialYear = "2024-25", User = "Test user", Id = 678767, MessageType = MessageTypes.Result };
            var runName = "Test Run Name";

            this.runNameService.Setup(t => t.GetRunNameAsync(It.IsAny<int>())).ReturnsAsync(runName);
            this.parameterMapper.Setup(t => t.Map(It.IsAny<CreateResultFileMessage>())).Returns(processedParameterData);
            this.calculatorRunService.Setup(t => t.StartProcess(It.IsAny<CalculatorRunParameter>(), It.IsAny<string>())).ReturnsAsync(true);
            MockResultMessage(myQueueItem);

            // Act
            await this.function.Run(myQueueItem);

            // Assert
            this.calculatorRunService.Verify(
                p => p.StartProcess(
                    It.Is<CalculatorRunParameter>(msg => msg == processedParameterData),
                    It.Is<string>(name => name == runName)),
                Times.Once);

            this.telemetryLogger.Verify(
                x => x.LogInformation(It.Is<TrackMessage>(log =>
                    log.Message.Contains("Executing the function app started"))),
                Times.Once);
        }

        /// <summary>
        /// Tests the Run method with a valid message to ensure it processes the queue item correctly and returns true for billing file.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task ServiceBusTrigger_ValidMessageRun_BiilingTest()
        {
            // Arrange
            var myQueueItem = @"{ Id: 678767, ApprovedBy: 'Test User', MessageType: 'Billing'}";
            var resultFileMessage = new CreateBillingFileMessage
            {
                CalculatorRunId = 123,
                ApprovedBy = "Test User",
                MessageType = "Billing"
            };

            var processedParameterData = new BillingFileMessage() { ApprovedBy = "2024-25", MessageType = "Billing", Id = 678767 };
            var runName = "Test Run Name";

            this.parameterMapper.Setup(t => t.Map(It.IsAny<CreateBillingFileMessage>())).Returns(processedParameterData);
            messageTypeService.Setup(s => s.DeserializeMessage(myQueueItem)).Returns(resultFileMessage);
            this.prepareBillingFileService.Setup(t => t.PrepareBillingFileAsync(processedParameterData.Id, runName)).ReturnsAsync(true);

            // Act
            await this.function.Run(myQueueItem);

            // Assert
            this.telemetryLogger.Verify(
                x => x.LogInformation(It.Is<TrackMessage>(log =>
                    log.Message.Contains("Azure function app execution finished"))),
                Times.Once);
        }

        /// <summary>
        /// Tests the Run method to ensure it logs an error and returns false when the message is null or empty.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task ServiceBusTrigger_EmptyMessageRun()
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

        [TestMethod]
        public async Task ServiceBusTrigger_NullMessageRun()
        {
            // Arrange
            string? myQueueItem = null;

            // Act
            await this.function.Run(myQueueItem!);

            // Assert
            this.telemetryLogger.Verify(
                log => log.LogError(It.Is<ErrorMessage>(msg =>
                    msg.Message.Contains("Message is null or empty") &&
                    msg.Exception is ArgumentNullException)),
                Times.Once);
        }

        [TestMethod]
        public async Task ServiceBusTrigger_MessageJsonException()
        {
            // Arrange
            var myQueueItem = @"{ CalculatorRunId: 678767, FinancialYear: '2024-25', CreatedBy: 'Test user'}";
            this.parameterMapper.Setup(t => t.Map(It.IsAny<CreateResultFileMessage>())).Throws<JsonException>();

            MockResultMessage(myQueueItem);

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
            this.parameterMapper.Setup(t => t.Map(It.IsAny<CreateResultFileMessage>())).Throws<Exception>();
            MockResultMessage(myQueueItem);
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
        public async Task ServiceBusTrigger_InvalidMessageRunUnhandledException()
        {
            // Arrange
            var myQueueItem = @"{ CalculatorRunId: 678767, FinancialYear: '2024-25', CreatedBy: 'Test user'}";
            var processedParameterData = new CalculatorRunParameter() { FinancialYear = "2024-25", User = "Test user", Id = 678767 , MessageType = MessageTypes.Result };
            var runName = "Test Run Name";

            this.parameterMapper.Setup(t => t.Map(It.IsAny<CreateResultFileMessage>())).Returns(processedParameterData);
            this.runNameService.Setup(t => t.GetRunNameAsync(It.IsAny<int>())).ReturnsAsync(runName);

            // Setup StartProcess to throw an exception
            this.calculatorRunService.Setup(t => t.StartProcess(It.IsAny<CalculatorRunParameter>(), It.IsAny<string>())).ThrowsAsync(new Exception("Unhandled exception"));
            MockResultMessage(myQueueItem);

            // Act
            await this.function.Run(myQueueItem);

            // Assert
            this.telemetryLogger.Verify(
                log => log.LogError(It.Is<ErrorMessage>(msg =>
                    msg.Message.Contains("Unhandled exception") &&
                    msg.Exception is Exception)),
                Times.Once);
        }

        [TestMethod]
        public async Task ServiceBusTrigger_NullParamRun()
        {
            // Arrange
            var myQueueItem = @"{ CalculatorRunId: 678767, FinancialYear: '2024-25', CreatedBy: 'Test user'}";
            this.parameterMapper.Setup(t => t.Map(It.IsAny<CreateResultFileMessage>())).Returns(default(CalculatorRunParameter?));

            // Act
            await this.function.Run(myQueueItem);

            // Assert
            this.telemetryLogger.Verify(
                log => log.LogError(It.Is<ErrorMessage>(msg =>
                    msg.Message.Contains("Deserialized object is null") &&
                    msg.Exception is JsonException)),
                Times.Never);
        }

        [TestMethod]
        public async Task ServiceBusTrigger_SuccessfulProcess_Run()
        {
            // Arrange
            this.parameterMapper.Setup(t => t.Map(It.IsAny<CreateResultFileMessage>())).Returns((CalculatorRunParameter?)null);
            var myQueueItem = @"{ CalculatorRunId: 678767, FinancialYear: '2024-25', CreatedBy: 'Test user'}";
            var processedParameterData = new CalculatorRunParameter() { FinancialYear = "2024-25", User = "Test user", Id = 678767 , MessageType = MessageTypes.Result };
            var runName = "Test Run Name";

            this.parameterMapper.Setup(t => t.Map(It.IsAny<CreateResultFileMessage>())).Returns(processedParameterData);
            this.runNameService.Setup(t => t.GetRunNameAsync(It.IsAny<int>())).ReturnsAsync(runName);
            this.calculatorRunService.Setup(t => t.StartProcess(It.IsAny<CalculatorRunParameter>(), It.IsAny<string>())).ReturnsAsync(true);
            MockResultMessage(myQueueItem);

            // Act
            await this.function.Run(myQueueItem);

            // Assert
            this.telemetryLogger.Verify(
                log => log.LogInformation(It.Is<TrackMessage>(msg =>
                    msg.Message.Contains("Process status: True"))),
                Times.Once);
        }

        [TestMethod]
        public async Task ServiceBusTrigger_StartProcess_Unhandled_Exception()
        {
            // Arrange
            var myQueueItem = @"{ CalculatorRunId: 678767, FinancialYear: '2024-25', CreatedBy: 'Test user'}";
            var processedParameterData = new CalculatorRunParameter() { FinancialYear = "2024-25", User = "Test user", Id = 678767 , MessageType = MessageTypes.Result };
            var runName = "Test Run Name";

            this.runNameService.Setup(t => t.GetRunNameAsync(It.IsAny<int>())).ReturnsAsync(runName);
            this.parameterMapper.Setup(t => t.Map(It.IsAny<CreateResultFileMessage>())).Returns(processedParameterData);
            this.calculatorRunService.Setup(t => t.StartProcess(It.IsAny<CalculatorRunParameter>(), It.IsAny<string>())).Throws<Exception>();
            MockResultMessage(myQueueItem);

            // Act
            await this.function.Run(myQueueItem);

            // Assert
            this.telemetryLogger.Verify(
                log => log.LogError(It.Is<ErrorMessage>(msg =>
                    msg.Message.Contains("Error") &&
                    msg.Exception is Exception)),
                Times.Once);
        }

        [TestMethod]
        public async Task ServiceBusTrigger_GetRunNameAsync_Unhandled_Exception()
        {
            // Arrange
            var myQueueItem = @"{ CalculatorRunId: 678767, FinancialYear: '2024-25', CreatedBy: 'Test user'}";
            var processedParameterData = new CalculatorRunParameter() { FinancialYear = "2024-25", User = "Test user", Id = 678767 , MessageType = MessageTypes.Result };

            this.parameterMapper.Setup(t => t.Map(It.IsAny<CreateResultFileMessage>())).Returns(processedParameterData);
            this.runNameService.Setup(t => t.GetRunNameAsync(It.IsAny<int>())).Throws<Exception>();
            MockResultMessage(myQueueItem);

            // Act
            await this.function.Run(myQueueItem);

            // Assert
            this.telemetryLogger.Verify(
                log => log.LogError(It.Is<ErrorMessage>(msg =>
                    msg.Message.Contains("Run name not found") &&
                    msg.Exception is Exception)),
                Times.Once);
        }

        private void MockResultMessage(string myQueueItem)
        {
            var resultFileMessage = new CreateResultFileMessage
            {
                CalculatorRunId = 1,
                FinancialYear = "2024-25",
                CreatedBy = "TestUser",
                MessageType = "Result"
            };

            messageTypeService.Setup(s => s.DeserializeMessage(myQueueItem)).Returns(resultFileMessage);
        }

    }
}