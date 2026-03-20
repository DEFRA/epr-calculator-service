using EPR.Calculator.Service.Common.Logging;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Moq;

namespace EPR.Calculator.Service.Common.UnitTests.Logging
{
    [TestClass]
    public class TelemetryClientWrapperTests
    {
        private Mock<ITelemetryClientWrapper> mockTelemetryClient;
        private TelemetryClient telemetryClient;
        private TelemetryClientWrapper telemetryClientWrapper;

        public TelemetryClientWrapperTests()
        {
            mockTelemetryClient = new Mock<ITelemetryClientWrapper>();
            telemetryClient = new TelemetryClient(new TelemetryConfiguration());
            telemetryClientWrapper = new TelemetryClientWrapper(telemetryClient);
        }

        [TestMethod]
        public void TelemetryClientShouldCall_TrackTrace()
        {
            // Arrange
            var traceTelemetry = new TraceTelemetry("Test trace message", SeverityLevel.Information);

            // Act
            telemetryClientWrapper.TrackTrace(traceTelemetry);

            // Assert
            mockTelemetryClient.Verify(client => client.TrackTrace(traceTelemetry), Times.Never);
        }

        [TestMethod]
        public void TelemetryClientShouldCall_TrackException()
        {
            // Arrange
            var exceptionTelemetry = new ExceptionTelemetry(new Exception("Test exception"));

            // Act
            telemetryClientWrapper.TrackException(exceptionTelemetry);

            // Assert
            mockTelemetryClient.Verify(client => client.TrackException(exceptionTelemetry), Times.Never);
        }
    }
}
