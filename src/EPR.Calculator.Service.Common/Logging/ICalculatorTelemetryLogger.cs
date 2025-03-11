namespace EPR.Calculator.Service.Common.Logging
{
    using System;

    /// <summary>
    /// Interface for logging telemetry data for calculator operations.
    /// </summary>
    public interface ICalculatorTelemetryLogger
    {
        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="logMessage">The log message to log.</param>
        void LogInformation(TrackMessage logMessage);

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="errorMessage">The log message to log.</param>
        void LogError(ErrorMessage errorMessage);
    }
}