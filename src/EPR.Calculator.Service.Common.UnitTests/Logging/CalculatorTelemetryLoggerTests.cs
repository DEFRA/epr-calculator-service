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
            var runId = "123";
            var runName = "TestRun";
            var message = "This is an informational message";

            // Act
            this.calculatorTelemetryLogger.LogInformation(runId, runName, message);

            // Assert
            this.mockTelemetryClient.Verify(
                tc => tc.TrackTrace(It.Is<TraceTelemetry>(t =>
                t.Message.Contains(runId) &&
                t.Message.Contains(runName) &&
                t.Message.Contains(message) &&
                t.SeverityLevel == SeverityLevel.Information)), Times.Once);
        }

        /// <summary>
        /// Tests that the LogError method tracks an exception with error severity.
        /// </summary>
        [TestMethod]
        public void LogError_ShouldTrackExceptionWithErrorSeverity()
        {
            // Arrange
            var runId = "123";
            var runName = "TestRun";
            var message = "This is an error message";
            var exception = new Exception("Test exception");

            // Act
            this.calculatorTelemetryLogger.LogError(runId, runName, message, exception);

            // Assert
            this.mockTelemetryClient.Verify(
                tc => tc.TrackException(It.Is<ExceptionTelemetry>(et =>
                et.Exception == exception &&
                et.Message.Contains(runId) &&
                et.Message.Contains(runName) &&
                et.Message.Contains(message) &&
                et.SeverityLevel == SeverityLevel.Error)), Times.Once);
        }
    }
}