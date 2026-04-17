namespace EPR.Calculator.Service.Function.Messaging;

public record CreateBillingFileMessage : MessageBase
{
    /// <summary>
    ///     The user who approved the billing file.
    /// </summary>
    public required string ApprovedBy { get; init; }
}