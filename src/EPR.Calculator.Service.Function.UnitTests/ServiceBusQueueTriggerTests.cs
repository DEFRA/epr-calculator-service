using System.Text.Json;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Exceptions;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.Telemetry;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using Moq;

namespace EPR.Calculator.Service.Function.UnitTests
{
    [TestClass]
    public class ServiceBusQueueTriggerTests
    {
        private readonly ServiceBusQueueTrigger function;
        private readonly Mock<ICalculatorRunService> calculatorRunService;
        private readonly Mock<IRunNameService> runNameService;
        private readonly Mock<IClassificationService> classificationService;
        private readonly Mock<ILogger<ServiceBusQueueTrigger>> logger;
        private readonly Mock<IMessageTypeService> messageTypeService;
        private readonly Mock<IPrepareBillingFileService> prepareBillingFileService;
        private readonly Mock<ITelemetryClient> telemetryClient;

        private const string ResultMessage = @"{""CalculatorRunId"": 1, ""RelativeYear"": 2024, ""CreatedBy"": ""Test user"", ""MessageType"": ""Result""}";
        private const string BillingMessage = @"{""CalculatorRunId"": 1, ""ApprovedBy"": ""Test User"", ""MessageType"": ""Billing""}";
        private const string RunName = "Test Run Name";

        public ServiceBusQueueTriggerTests()
        {
            calculatorRunService = new Mock<ICalculatorRunService>();
            runNameService = new Mock<IRunNameService>();
            messageTypeService = new Mock<IMessageTypeService>();
            classificationService = new Mock<IClassificationService>();
            logger = new Mock<ILogger<ServiceBusQueueTrigger>>();
            prepareBillingFileService = new Mock<IPrepareBillingFileService>();
            telemetryClient = new Mock<ITelemetryClient>();

            function = new ServiceBusQueueTrigger(
                calculatorRunService.Object,
                runNameService.Object,
                logger.Object,
                messageTypeService.Object,
                prepareBillingFileService.Object,
                classificationService.Object,
                telemetryClient.Object);
        }

        /// <summary>
        /// Tests that a valid result file message causes the calculator run service to be invoked.
        /// </summary>
        [TestMethod]
        public async Task Run_WithValidResultMessage_CallsCalculatorRunService()
        {
            // Arrange
            runNameService.Setup(s => s.GetRunNameAsync(It.IsAny<int>())).ReturnsAsync(RunName);
            calculatorRunService.Setup(s => s.PrepareResultsFileAsync(It.IsAny<CalculatorRunParams>())).ReturnsAsync(true);
            SetupResultFileMessage(ResultMessage);

            // Act
            await function.Run(ResultMessage);

            // Assert
            calculatorRunService.Verify(s => s.PrepareResultsFileAsync(It.IsAny<CalculatorRunParams>()), Times.Once);
            AssertEvent("PayCalRunInit");
            AssertEvent("PayCalRunStarted");
            AssertEvent("PayCalRunCompleted");
        }

        /// <summary>
        /// Tests that a valid billing file message causes the billing file service to be invoked.
        /// </summary>
        [TestMethod]
        public async Task Run_WithValidBillingMessage_CallsPrepareBillingFileService()
        {
            // Arrange
            runNameService.Setup(s => s.GetRunNameAsync(It.IsAny<int>())).ReturnsAsync(RunName);
            prepareBillingFileService.Setup(s => s.PrepareBillingFileAsync(It.IsAny<BillingRunParams>())).ReturnsAsync(true);
            SetupBillingFileMessage(BillingMessage);

            // Act
            await function.Run(BillingMessage);

            // Assert
            prepareBillingFileService.Verify(s => s.PrepareBillingFileAsync(It.IsAny<BillingRunParams>()), Times.Once);
            AssertEvent("PayCalRunInit");
            AssertEvent("PayCalRunStarted");
            AssertEvent("PayCalRunCompleted");
        }

        /// <summary>
        /// Tests that a failure to deserialize the message throws a RunInitialiseException.
        /// </summary>
        [TestMethod]
        public async Task Run_WhenDeserializeFails_ThrowsRunInitialiseException()
        {
            // Arrange
            messageTypeService.Setup(s => s.DeserializeMessage(It.IsAny<string>())).Throws<JsonException>();

            // Act & Assert
            await Assert.ThrowsExceptionAsync<RunInitialiseException>(() => function.Run(ResultMessage));
            AssertEvent("PayCalRunInitFailed");
        }

        /// <summary>
        /// Tests that a failure to retrieve the run name throws a RunInitialiseException.
        /// </summary>
        [TestMethod]
        public async Task Run_WhenGetRunNameFails_ThrowsRunInitialiseException()
        {
            // Arrange
            SetupResultFileMessage(ResultMessage);
            runNameService.Setup(s => s.GetRunNameAsync(It.IsAny<int>())).ThrowsAsync(new Exception("Service unavailable"));

            // Act & Assert
            await Assert.ThrowsExceptionAsync<RunInitialiseException>(() => function.Run(ResultMessage));
            AssertEvent("PayCalRunInitFailed");
        }

        /// <summary>
        /// Tests that an unhandled exception from the calculator run service is logged as an error.
        /// </summary>
        [TestMethod]
        public async Task Run_WhenPrepareResultsFileThrows_LogsError()
        {
            // Arrange
            runNameService.Setup(s => s.GetRunNameAsync(It.IsAny<int>())).ReturnsAsync(RunName);
            calculatorRunService.Setup(s => s.PrepareResultsFileAsync(It.IsAny<CalculatorRunParams>())).ThrowsAsync(new Exception("Unhandled exception"));
            SetupResultFileMessage(ResultMessage);

            // Act
            await function.Run(ResultMessage);

            // Assert
            VerifyLogError("Run failed");
            AssertEvent("PayCalRunFailed");
            AssertEventProperty("PayCalRunFailed", "FailureReason", "UnhandledException");
        }

        /// <summary>
        /// Tests that a false result from the calculator run service is logged as an error.
        /// </summary>
        [TestMethod]
        public async Task Run_WhenPrepareResultsFileReturnsFalse_LogsError()
        {
            // Arrange
            runNameService.Setup(s => s.GetRunNameAsync(It.IsAny<int>())).ReturnsAsync(RunName);
            calculatorRunService.Setup(s => s.PrepareResultsFileAsync(It.IsAny<CalculatorRunParams>())).ReturnsAsync(false);
            SetupResultFileMessage(ResultMessage);

            // Act
            await function.Run(ResultMessage);

            // Assert
            VerifyLogError("Run failed");
            AssertEvent("PayCalRunFailed");
            AssertEventProperty("PayCalRunFailed", "FailureReason", "ProcessingFailed");
        }

        /// <summary>
        /// Tests that an unhandled exception from the billing file service is logged as an error.
        /// </summary>
        [TestMethod]
        public async Task Run_WhenPrepareBillingFileThrows_LogsError()
        {
            // Arrange
            runNameService.Setup(s => s.GetRunNameAsync(It.IsAny<int>())).ReturnsAsync(RunName);
            prepareBillingFileService.Setup(s => s.PrepareBillingFileAsync(It.IsAny<BillingRunParams>())).ThrowsAsync(new Exception("Billing error"));
            SetupBillingFileMessage(BillingMessage);

            // Act
            await function.Run(BillingMessage);

            // Assert
            VerifyLogError("Run failed");
            AssertEvent("PayCalRunFailed");
            AssertEventProperty("PayCalRunFailed", "FailureReason", "UnhandledException");
        }

        private void SetupResultFileMessage(string message)
        {
            messageTypeService.Setup(s => s.DeserializeMessage(message)).Returns(new CreateResultFileMessage
            {
                CalculatorRunId = 1,
                RelativeYear = new RelativeYear(2024),
                CreatedBy = "Test user",
                MessageType = "Result"
            });
        }

        private void SetupBillingFileMessage(string message)
        {
            messageTypeService.Setup(s => s.DeserializeMessage(message)).Returns(new CreateBillingFileMessage
            {
                CalculatorRunId = 1,
                ApprovedBy = "Test User",
                MessageType = "Billing"
            });
        }

        private void VerifyLogError(string messageContains)
        {
            logger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(messageContains)),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private void AssertEvent(string eventName)
        {
            telemetryClient.Verify(
                c => c.TrackEvent(It.Is<EventTelemetry>(e => e.Name == eventName)),
                Times.AtLeastOnce,
                $"Expected telemetry event '{eventName}' was not raised.");
        }

        private void AssertEventProperty(string eventName, string propertyKey, string expectedValue)
        {
            telemetryClient.Verify(
                c => c.TrackEvent(It.Is<EventTelemetry>(e =>
                    e.Name == eventName &&
                    e.Properties.ContainsKey(propertyKey) &&
                    e.Properties[propertyKey] == expectedValue)),
                Times.AtLeastOnce,
                $"Expected '{eventName}'.{propertyKey} = '{expectedValue}'.");
        }
    }
}