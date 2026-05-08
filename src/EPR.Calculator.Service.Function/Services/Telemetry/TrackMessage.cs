namespace EPR.Calculator.Service.Function.Services.Telemetry;

/// <summary>
///     Represents an error message for logging.
/// </summary>
public class TrackMessage
{
    /// <summary>
    ///     Gets or sets the unique identifier for the run.
    /// </summary>
    public int? RunId { get; set; }

    /// <summary>
    ///     Gets or sets the name of the run.
    /// </summary>
    public string? RunName { get; set; }

    /// <summary>
    ///     Gets or sets the message Type of the run.
    /// </summary>
    public string? MessageType { get; set; }

    /// <summary>
    ///     Gets or sets the message to log.
    /// </summary>
    public required string Message { get; set; }
}
