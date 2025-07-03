using System.Collections.Generic;
using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Converter;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public record CalcResultCommsCostByMaterial2AJson
    {
        [JsonPropertyName("materialBreakdown")]
        public required IEnumerable<CalcResultCommsCostByMaterial2AMaterialBreakdown> MaterialBreakdown { get; init; }
    }

    public record CalcResultCommsCostByMaterial2AMaterialBreakdown
    {
        [JsonPropertyName("materialName")]
        public required string MaterialName { get; init; }

        [JsonPropertyName("householdPackagingWasteTonnage")]
        public decimal HouseholdPackagingWasteTonnage { get; init; }

        [JsonPropertyName("publicBinTonnage")]
        public decimal PublicBinTonnage { get; init; }

        [JsonPropertyName("totalTonnage")]
        public decimal TotalTonnage { get; init; }

        [JsonPropertyName("householdDrinksContainersTonnageGlass")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? HouseholdDrinksContainersTonnageGlass { get; set; }

        [JsonPropertyName("pricePerTonne")]
        public required string PricePerTonne { get; init; }

        [JsonPropertyName("producerTotalCostWithoutBadDebtProvision")]
        public required string ProducerTotalCostWithoutBadDebtProvision { get; init; }

        [JsonPropertyName("badDebtProvision")]
        public required string BadDebtProvision { get; init; }

        [JsonPropertyName("producerTotalCostWithBadDebtProvision")]
        public required string ProducerTotalCostwithBadDebtProvision { get; init; }

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
