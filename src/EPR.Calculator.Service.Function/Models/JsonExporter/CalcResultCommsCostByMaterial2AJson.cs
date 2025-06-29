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

        [JsonProperty(PropertyName = "householdDrinksContainers", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal? HouseholdDrinksContainers { get; set; }

        [JsonProperty(PropertyName = "pricePerTonne")]
        public required string PriceperTonne { get; init; }

        [JsonProperty(PropertyName = "producerTotalCostWithoutBadDebtProvision")]
        public required string ProducerTotalCostWithoutBadDebtProvision { get; init; }

        [JsonProperty(PropertyName = "badDebtProvision")]
        public required string BadDebtProvision { get; init; }

        [JsonProperty(PropertyName = "producerTotalCostWithBadDebtProvision")]
        public required string ProducerTotalCostwithBadDebtProvision { get; init; }

        [JsonProperty(PropertyName = "englandWithBadDebtProvision")]
        public required string EnglandWithBadDebtProvision { get; init; }

        [JsonProperty(PropertyName = "walesWithBadDebtProvision")]
        public required string WalesWithBadDebtProvision { get; init; }

        [JsonProperty(PropertyName = "scotlandWithBadDebtProvision")]
        public required string ScotlandWithBadDebtProvision { get; init; }

        [JsonProperty(PropertyName = "northernIrelandWithBadDebtProvision")]
        public required string NorthernIrelandWithBadDebtProvision { get; init; }
    }
}
