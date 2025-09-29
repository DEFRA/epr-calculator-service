using System.Text.Json.Serialization;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public record CalculationOfSuggestedBillingInstructionsAndInvoiceAmounts
    {
        [JsonPropertyName("currentYearInvoicedTotalToDate")]
        public required string? CurrentYearInvoicedTotalToDate { get; init; }

        [JsonPropertyName("tonnageChangeSinceLastInvoice")]
        public required string TonnageChangeSinceLastInvoice { get; init; }

        [JsonPropertyName("liabilityDifferenceCalcVsPrev")]
        public required string? LiabilityDifferenceCalcVsPrev { get; init; }

        [JsonPropertyName("material£ThresholdBreached")]
        public required string MaterialThresholdBreached { get; init; }

        [JsonPropertyName("tonnage£ThresholdBreached")]
        public required string TonnageThresholdBreached { get; init; }

        [JsonPropertyName("percentageLiabilityDifferenceCalcVsPrev")]
        public required string PercentageLiabilityDifferenceCalcVsPrev { get; init; }

        [JsonPropertyName("materialPercentageThresholdBreached")]
        public required string MaterialPercentageThresholdBreached { get; init; }

        [JsonPropertyName("tonnagePercentageThresholdBreached")]
        public required string TonnagePercentageThresholdBreached { get; init; }

        [JsonPropertyName("suggestedBillingInstruction")]
        public required string SuggestedBillingInstruction { get; init; }

        [JsonPropertyName("suggestedInvoiceAmount")]
        public required string SuggestedInvoiceAmount { get; init; }
    }
}
