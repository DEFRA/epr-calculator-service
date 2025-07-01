using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public record ProducerDisposalFeesWithBadDebtProvision1
    {
        [JsonPropertyName("materialBreakdown")]
        public required IEnumerable<ProducerDisposalFeesWithBadDebtProvision1MaterialBreakdown> MaterialBreakdown { get; set; }
    }

    public record ProducerDisposalFeesWithBadDebtProvision1MaterialBreakdown
    {
        [JsonPropertyName("materialName")]
        public required string MaterialName { get; init; }

        [JsonPropertyName("previousInvoicedTonnage")]
        public required string PreviousInvoicedTonnage { get; init; }

        [JsonPropertyName("householdPackagingWasteTonnage")]
        public required decimal HouseholdPackagingWasteTonnage { get; init; }

        [JsonPropertyName("publicBinTonnage")]
        public required decimal PublicBinTonnage { get; init; }

        [JsonPropertyName("householdDrinksContainersTonnageGlass")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? HouseholdDrinksContainersTonnageGlass { get; set; }

        [JsonPropertyName("totalTonnage")]
        public required decimal TotalTonnage { get; init; }

        [JsonPropertyName("selfManagedConsumerWasteTonnage")]
        public required decimal SelfManagedConsumerWasteTonnage { get; init; }

        [JsonPropertyName("netTonnage")]
        public required decimal NetTonnage { get; init; }

        [JsonPropertyName("tonnageChange")]
        public required string TonnageChange { get; init; }

        [JsonPropertyName("pricePerTonne")]
        public required string PricePerTonne { get; init; }

        [JsonPropertyName("producerDisposalFeeWithoutBadDebtProvision")]
        public required string ProducerDisposalFeeWithoutBadDebtProvision { get; init; }

        [JsonPropertyName("badDebtProvision")]
        public required string BadDebtProvision { get; init; }

        [JsonPropertyName("producerDisposalFeeWithBadDebtProvision")]
        public required string ProducerDisposalFeeWithBadDebtProvision { get; init; }

        [JsonPropertyName("englandWithBadDebtProvision")]
        public required string EnglandWithBadDebtProvision { get; init; }

        [JsonPropertyName("walesWithBadDebtProvision")]
        public required string WalesWithBadDebtProvision { get; init; }

        [JsonPropertyName("scotlandWithBadDebtProvision")]
        public required string ScotlandWithBadDebtProvision { get; init; }

        [JsonPropertyName("northernIrelandWithBadDebtProvision")]
        public required string NorthernIrelandWithBadDebtProvision { get; init; }
    }

}
