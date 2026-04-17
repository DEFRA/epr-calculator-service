namespace EPR.Calculator.Service.Function.Models;

public record InvoicedProducerRecord
{
    public required int ProducerId { get; init; }
    public required string ProducerName { get; init; }
    public required string? TradingName { get; init; }
    public required int CalculatorRunId { get; init; }
    public required string CalculatorName { get; init; }
    public required int MaterialId { get; init; }
    public required decimal? InvoicedNetTonnage { get; init; }
    public required string? BillingInstructionId { get; init; }
    public decimal? CurrentYearInvoicedTotalAfterThisRun { get; set; }
}