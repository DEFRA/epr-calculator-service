namespace EPR.Calculator.Service.Common.UnitTests.Logging
{
    using System;
    using System.Collections.Generic;
    using EPR.Calculator.Service.Common.Logging;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Unit tests for the <see cref="CalculatorTelemetryLogger"/> class.
    /// </summary>
    [TestClass]
    public class CalculatorTelemetryLoggerTests
    {
        private Mock<ITelemetryClientWrapper> mockTelemetryClient;
        private CalculatorTelemetryLogger calculatorTelemetryLogger;

        public CalculatorTelemetryLoggerTests()
        {
            this.mockTelemetryClient = new Mock<ITelemetryClientWrapper>();
            this.calculatorTelemetryLogger = new CalculatorTelemetryLogger(this.mockTelemetryClient.Object);
        }

        /// <summary>
        /// Tests that the LogInformation method tracks a trace with information severity.
        /// </summary>
        [TestMethod]
        public void LogInformation_ShouldTrackTraceWithInformationSeverity()
        {
            // Arrange
            var logMessage = new TrackMessage
            {
                RunId = 123,
                RunName = "TestRun",
                Message = "This is an informational message",
            };

            // Act
            this.calculatorTelemetryLogger.LogInformation(logMessage);

            // Assert
            this.mockTelemetryClient.Verify(
                tc => tc.TrackTrace(It.Is<TraceTelemetry>(t =>
                t.Message.Contains(logMessage.RunId.ToString() ?? string.Empty) &&
                t.Message.Contains(logMessage.RunName ?? string.Empty) &&
                t.Message.Contains(logMessage.Message ?? string.Empty) &&
                t.SeverityLevel == SeverityLevel.Information)), Times.Once);
        }

        /// <summary>
        /// Tests that the LogError method tracks an exception with error severity.
        /// </summary>
        [TestMethod]
        public void LogError_ShouldTrackExceptionWithErrorSeverity()
        {
            // Arrange
            var errorMessage = new ErrorMessage
            {
                RunId = 123,
                RunName = "TestRun",
                Message = "This is an error message",
                Exception = new Exception("Test exception"),
            };

            // Act
            this.calculatorTelemetryLogger.LogError(errorMessage);

            // Assert
            this.mockTelemetryClient.Verify(
                tc => tc.TrackException(It.Is<ExceptionTelemetry>(et =>
                et.Exception == errorMessage.Exception &&
                et.Message.Contains(errorMessage.RunId.ToString() ?? string.Empty) &&
                et.Message.Contains(errorMessage.RunName) &&
                et.Message.Contains(errorMessage.Message) &&
                et.SeverityLevel == SeverityLevel.Error)), Times.Once);
        }

        [TestMethod]
        public void CreateLogMessage_ShouldReturnFormattedMessage()
        {
            // Arrange
            int? runId = 1;
            string? runName = "TestRun";
            string message = "Test message";
            string? messageType = "Result";

            // Act
            var result = CalculatorTelemetryLogger.CreateLogMessage(runId, runName, messageType, message);

            // Assert
            Assert.IsTrue(result.Contains("RunId: 1"));
            Assert.IsTrue(result.Contains("RunName: TestRun"));
            Assert.IsTrue(result.Contains("Message: Test message"));
        }

        [TestMethod]
        public void CreateLogMessage_ShouldHandleNullRunIdAndRunName()
        {
            // Arrange
            int? runId = null;
            string? runName = null;
            string message = "Test message";
            string? messageType = "Result";

            // Act
            var result = CalculatorTelemetryLogger.CreateLogMessage(runId, runName, messageType, message);

            // Assert
            Assert.IsTrue(result.Contains("RunId: "));
            Assert.IsTrue(result.Contains("RunName: "));
            Assert.IsTrue(result.Contains("Message: Test message"));
        }

        [TestMethod]
        public void AddProperties_ShouldAddRunIdAndRunNameToProperties()
        {
            // Arrange
            var properties = new Dictionary<string, string>();
            int? runId = 1;
            string? runName = "TestRun";

            // Act
            CalculatorTelemetryLogger.AddProperties(properties, runId, runName);

            // Assert
            Assert.AreEqual("1", properties["RunId"]);
            Assert.AreEqual("TestRun", properties["RunName"]);
        }

        [TestMethod]
        public void AddProperties_ShouldHandleNullRunIdAndRunName()
        {
            // Arrange
            var properties = new Dictionary<string, string>();
            int? runId = null;
            string? runName = null;

            // Act
            CalculatorTelemetryLogger.AddProperties(properties, runId, runName);

            // Assert
            Assert.IsFalse(properties.ContainsKey("RunId"));
            Assert.IsFalse(properties.ContainsKey("RunName"));
        }

        [TestMethod]
        public void TrackTrace_ShouldTrackTraceWithDifferentSeverityLevels()
        {
            // Arrange
            var logMessage = new TrackMessage
            {
                RunId = 123,
                RunName = "TestRun",
                Message = "This is a trace message",
            };

            // Act
            this.calculatorTelemetryLogger.LogInformation(logMessage);
            this.calculatorTelemetryLogger.LogError(new ErrorMessage { RunId = 123, RunName = "TestRun", Message = "This is an error message", Exception = new Exception("Test exception") });

            // Assert
            this.mockTelemetryClient.Verify(
                tc => tc.TrackTrace(It.Is<TraceTelemetry>(t =>
                t.Message.Contains(logMessage.RunId.ToString() ?? string.Empty) &&
                t.Message.Contains(logMessage.RunName ?? string.Empty) &&
                t.Message.Contains(logMessage.Message ?? string.Empty) &&
                t.SeverityLevel == SeverityLevel.Information)), Times.Once);

            this.mockTelemetryClient.Verify(
                tc => tc.TrackException(It.Is<ExceptionTelemetry>(et =>
                et.Exception.Message.Contains("Test exception") &&
                et.Message.Contains("This is an error message") &&
                et.SeverityLevel == SeverityLevel.Error)), Times.Once);
        }

        [TestCleanup]
        public void Cleanup()
        {
            this.mockTelemetryClient = null!;
            this.calculatorTelemetryLogger = null!;
        }
    }
}