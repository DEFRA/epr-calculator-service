namespace EPR.Calculator.Service.Common.UnitTests.Logging
{
    using System;
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

        /// <summary>
        /// Initializes the test setup.
        /// </summary>
        [TestInitialize]
        public void Setup()
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
                Exception = new Exception("Test exception")
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
    }
}