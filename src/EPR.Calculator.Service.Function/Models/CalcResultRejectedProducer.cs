namespace EPR.Calculator.Service.Function.Models;

public record CalcResultRejectedProducer
{
    public required int ProducerId { get; init; }
    public required string ProducerName { get; init; }
    public required string TradingName { get; init; }
    public required string SuggestedBillingInstruction { get; init; }
    public required decimal SuggestedInvoiceAmount { get; init; }
    public required DateTime? InstructionConfirmedDate { get; init; }
    public required string InstructionConfirmedBy { get; init; }
    public required string ReasonForRejection { get; init; }
}
