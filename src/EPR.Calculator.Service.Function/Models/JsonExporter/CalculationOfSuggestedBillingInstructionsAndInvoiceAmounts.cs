using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public record CalculationOfSuggestedBillingInstructionsAndInvoiceAmounts
    {
        [JsonProperty(PropertyName = "currentYearInvoicedTotalToDate")]
        public required string CurrentYearInvoicedTotalToDate { get; init; }

        [JsonProperty(PropertyName = "tonnageChangeSinceLastInvoice")]
        public required string TonnageChangeSinceLastInvoice { get; init; }

        [JsonProperty(PropertyName = "liabilityDifferenceCalcVsPrev")]
        public required string LiabilityDifferenceCalcVsPrev { get; init; }

        [JsonProperty(PropertyName = "materialThresholdBreached")]
        public required string MaterialThresholdBreached { get; init; }

        [JsonProperty(PropertyName = "tonnageThresholdBreached")]
        public required string TonnageThresholdBreached { get; init; }

        [JsonProperty(PropertyName = "percentageLiabilityDifferenceCalcVsPrev")]
        public required string PercentageLiabilityDifferenceCalcVsPrev { get; init; }

        [JsonProperty(PropertyName = "materialPercentageThresholdBreached")]
        public required string MaterialPercentageThresholdBreached { get; init; }

        [JsonProperty(PropertyName = "tonnagePercentageThresholdBreached")]
        public required string TonnagePercentageThresholdBreached { get; init; }

        [JsonProperty(PropertyName = "suggestedBillingInstruction")]
        public required string SuggestedBillingInstruction { get; init; }

        [JsonProperty(PropertyName = "suggestedInvoiceAmount")]
        public required string SuggestedInvoiceAmount { get; init; }
    }
}
