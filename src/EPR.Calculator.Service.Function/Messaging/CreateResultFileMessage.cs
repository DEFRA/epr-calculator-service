namespace EPR.Calculator.Service.Function.Messaging;

public record CreateResultFileMessage : MessageBase
{
    /// <summary>
    ///     The user who initiated the calculator run.
    /// </summary>
    public required string CreatedBy { get; init; }
}