namespace EPR.Calculator.Service.Common.Logging
{
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DataContracts;

    /// <summary>
    /// Logger for telemetry data in the calculator service.
    /// </summary>
    public class CalculatorTelemetryLogger : ICalculatorTelemetryLogger
    {
        private readonly ITelemetryClientWrapper _telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculatorTelemetryLogger"/> class.
        /// </summary>
        /// <param name="telemetryClient">The telemetry client to use for logging.</param>
        public CalculatorTelemetryLogger(ITelemetryClientWrapper telemetryClient)
        {
            this._telemetryClient = telemetryClient;
        }

        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="logMessage">The log message to log.</param>
        public void LogInformation(TrackMessage logMessage)
        {
            this.TrackTrace(logMessage, SeverityLevel.Information);
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="errorMessage">The error message to log.</param>
        public void LogError(ErrorMessage errorMessage)
        {
            var formattedMessage = CreateLogMessage(errorMessage.RunId, errorMessage.RunName, errorMessage.MessageType, errorMessage.Message);
            var exceptionTelemetry = new ExceptionTelemetry(errorMessage.Exception)
            {
                SeverityLevel = SeverityLevel.Error,
                Message = formattedMessage,
            };
            AddProperties(exceptionTelemetry.Properties, errorMessage.RunId, errorMessage.RunName);
            this._telemetryClient.TrackException(exceptionTelemetry);
        }

        /// <summary>
        /// Creates a log message string with the provided run information and message.
        /// </summary>
        /// <param name="runId">The ID of the run.</param>
        /// <param name="runName">The name of the run.</param>
        /// <param name="message">The message to log.</param>
        /// <returns>A formatted log message string.</returns>
        internal static string CreateLogMessage(int? runId, string? runName, string? messageType, string message)
        {
            return $"[{DateTime.Now}] RunId: {runId}, RunName: {runName}, MessageType: {messageType}, Message: {message}";
        }

        /// <summary>
        /// Adds properties to the telemetry data.
        /// </summary>
        /// <param name="properties">The properties dictionary to add to.</param>
        /// <param name="runId">The ID of the run.</param>
        /// <param name="runName">The name of the run.</param>
        internal static void AddProperties(IDictionary<string, string> properties, int? runId, string? runName)
        {
            if (runId.HasValue)
            {
                properties["RunId"] = runId.Value.ToString();
            }

            if (!string.IsNullOrEmpty(runName))
            {
                properties["RunName"] = runName;
            }
        }

        private void TrackTrace(TrackMessage logMessage, SeverityLevel severityLevel)
        {
            var formattedMessage = CreateLogMessage(logMessage.RunId, logMessage.RunName, logMessage.MessageType, logMessage.Message);
            var traceTelemetry = new TraceTelemetry
            {
                Message = formattedMessage,
                SeverityLevel = severityLevel,
            };
            AddProperties(traceTelemetry.Properties, logMessage.RunId, logMessage.RunName);
            this._telemetryClient.TrackTrace(traceTelemetry);
        }
    }
}