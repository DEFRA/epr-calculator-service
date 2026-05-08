namespace EPR.Calculator.Service.Function.Models;

public record CalcResultSummaryBillingInstruction
{
    public decimal? CurrentYearInvoiceTotalToDate { get; init; }
    public string? TonnageChangeSinceLastInvoice { get; init; }
    public decimal? LiabilityDifference { get; init; }
    public string? MaterialThresholdBreached { get; init; }
    public string? TonnageThresholdBreached { get; init; }
    public decimal? PercentageLiabilityDifference { get; init; }
    public string? MaterialPercentageThresholdBreached { get; init; }
    public string? TonnagePercentageThresholdBreached { get; init; }
    public required string SuggestedBillingInstruction { get; init; }
    public decimal? SuggestedInvoiceAmount { get; init; }
}