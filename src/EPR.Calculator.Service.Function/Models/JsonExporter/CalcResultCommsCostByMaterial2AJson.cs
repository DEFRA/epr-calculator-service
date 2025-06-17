using System.Collections.Generic;
using EPR.Calculator.Service.Function.Converter;
using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public record CalcResultCommsCostByMaterial2AJson
    {
        [JsonProperty(PropertyName = "materialBreakdown")]
        public required IEnumerable<CalcResultCommsCostByMaterial2AMaterialBreakdown> MaterialBreakdown { get; init; }
    }

    public record CalcResultCommsCostByMaterial2AMaterialBreakdown
    {
        [JsonProperty(PropertyName = "materialName")]
        public required string MaterialName { get; init; }

        [JsonProperty(PropertyName = "householdPackagingWasteTonnage")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal HouseholdPackagingWasteTonnage { get; init; }

        [JsonProperty(PropertyName = "publicBinTonnage")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal ReportedPublicBinTonnage { get; init; }

        [JsonProperty(PropertyName = "totalTonnage")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal TotalReportedTonnage { get; init; }

        [JsonProperty(PropertyName = "householdDrinksContainers")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal HouseholdDrinksContainers { get; init; }

        [JsonProperty(PropertyName = "pricePerTonne")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal PriceperTonne { get; init; }

        [JsonProperty(PropertyName = "producerTotalCostWithoutBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal ProducerTotalCostWithoutBadDebtProvision { get; init; }

        [JsonProperty(PropertyName = "badDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal BadDebtProvision { get; init; }

        [JsonProperty(PropertyName = "producerTotalCostWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal ProducerTotalCostwithBadDebtProvision { get; init; }

        [JsonProperty(PropertyName = "englandWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal EnglandWithBadDebtProvision { get; init; }

        [JsonProperty(PropertyName = "walesWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal WalesWithBadDebtProvision { get; init; }

        [JsonProperty(PropertyName = "scotlandWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal ScotlandWithBadDebtProvision { get; init; }

        [JsonProperty(PropertyName = "northernIrelandWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal NorthernIrelandWithBadDebtProvision { get; init; }
    }
}
