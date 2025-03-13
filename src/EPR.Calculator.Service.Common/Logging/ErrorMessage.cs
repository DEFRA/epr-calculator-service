namespace EPR.Calculator.Service.Common.Logging
{
    /// <summary>
    /// Represents an error message to be logged.
    /// </summary>
    public class ErrorMessage : TrackMessage
    {
        /// <summary>
        /// Gets or sets the exception to log.
        /// </summary>
        required public Exception Exception { get; set; }
    }
}