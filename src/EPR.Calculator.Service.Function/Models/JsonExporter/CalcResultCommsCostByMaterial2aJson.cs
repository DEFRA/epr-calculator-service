using System.Collections.Generic;
using EPR.Calculator.Service.Function.Converter;
using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public record CalcResultCommsCostByMaterial2aJson
    {
        [JsonProperty(PropertyName = "materialBreakdown")]
        public IEnumerable<CalcResultCommsCostByMaterial2aMaterialBreakdown> MaterialBreakdown { get; set; }
    }

    public record CalcResultCommsCostByMaterial2aMaterialBreakdown
    {
        [JsonProperty(PropertyName = "materialName")]
        public required string MaterialName { get; set; }

        [JsonProperty(PropertyName = "householdPackagingWasteTonnage")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal HouseholdPackagingWasteTonnage { get; set; }

        [JsonProperty(PropertyName = "publicBinTonnage")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal ReportedPublicBinTonnage { get; set; }

        [JsonProperty(PropertyName = "totalTonnage")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal TotalReportedTonnage { get; set; }

        [JsonProperty(PropertyName = "householdDrinksContainers")]
        public decimal HouseholdDrinksContainers { get; set; }

        [JsonProperty(PropertyName = "pricePerTonne")]
        public decimal PriceperTonne { get; set; }

        [JsonProperty(PropertyName = "producerTotalCostWithoutBadDebtProvision")]
        public decimal ProducerTotalCostWithoutBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "badDebtProvision")]
        public decimal BadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "producerTotalCostWithBadDebtProvision")]
        public decimal ProducerTotalCostwithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "englandWithBadDebtProvision")]
        public decimal EnglandWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "walesWithBadDebtProvision")]
        public decimal WalesWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "scotlandWithBadDebtProvision")]
        public decimal ScotlandWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "northernIrelandWithBadDebtProvision")]
        public decimal NorthernIrelandWithBadDebtProvision { get; set; }
    }
}
