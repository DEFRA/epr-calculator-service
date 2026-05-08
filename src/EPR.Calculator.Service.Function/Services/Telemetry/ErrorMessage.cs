namespace EPR.Calculator.Service.Function.Services.Telemetry;

/// <summary>
///     Represents an error message to be logged.
/// </summary>
public class ErrorMessage : TrackMessage
{
    /// <summary>
    ///     Gets or sets the exception to log.
    /// </summary>
    public required Exception Exception { get; set; }
}
