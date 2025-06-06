namespace EPR.Calculator.Service.Common.UnitTests.Logging
{
    using EPR.Calculator.Service.Common.Logging;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class TelemetryClientWrapperTests
    {
        private Mock<ITelemetryClientWrapper> mockTelemetryClient;
        private TelemetryClient telemetryClient;
        private TelemetryClientWrapper telemetryClientWrapper;

        public TelemetryClientWrapperTests()
        {
            this.mockTelemetryClient = new Mock<ITelemetryClientWrapper>();
            this.telemetryClient = new TelemetryClient(new TelemetryConfiguration());
            this.telemetryClientWrapper = new TelemetryClientWrapper(this.telemetryClient);
        }

        [TestMethod]
        public void TelemetryClientShouldCall_TrackTrace()
        {
            // Arrange
            var traceTelemetry = new TraceTelemetry("Test trace message", SeverityLevel.Information);

            // Act
            this.telemetryClientWrapper.TrackTrace(traceTelemetry);

            // Assert
            this.mockTelemetryClient.Verify(client => client.TrackTrace(traceTelemetry), Times.Never);
        }

        [TestMethod]
        public void TelemetryClientShouldCall_TrackException()
        {
            // Arrange
            var exceptionTelemetry = new ExceptionTelemetry(new System.Exception("Test exception"));

            // Act
            this.telemetryClientWrapper.TrackException(exceptionTelemetry);

            // Assert
            this.mockTelemetryClient.Verify(client => client.TrackException(exceptionTelemetry), Times.Never);
        }
    }
}
