using EPR.Calculator.Service.Function.Converter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public record ProducerDisposalFeesWithBadDebtProvision1
    {
        [JsonProperty("materialBreakdown")]
        public required IEnumerable<ProducerDisposalFeesWithBadDebtProvision1MaterialBreakdown> MaterialBreakdown { get; set; }
    }

    public record ProducerDisposalFeesWithBadDebtProvision1MaterialBreakdown
    {
        [JsonProperty(PropertyName = "materialName")]
        public required string MaterialName { get; init; }

        [JsonProperty(PropertyName = "previousInvoicedTonnage")]
        public required string PreviousInvoicedTonnage { get; init; }

        [JsonProperty(PropertyName = "householdPackagingWasteTonnage")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public required decimal HouseholdPackagingWasteTonnage { get; init; }

        [JsonProperty(PropertyName = "publicBinTonnage")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public required decimal PublicBinTonnage { get; init; }

        [JsonProperty(PropertyName = "householdDrinksContainersTonnageGlass", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal? HouseholdDrinksContainersTonnageGlass { get; set; }

        [JsonProperty(PropertyName = "totalTonnage")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public required decimal TotalTonnage { get; init; }

        [JsonProperty(PropertyName = "selfManagedConsumerWasteTonnage")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public required decimal SelfManagedConsumerWasteTonnage { get; init; }

        [JsonProperty(PropertyName = "netTonnage")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public required decimal NetTonnage { get; init; }

        [JsonProperty(PropertyName = "tonnageChange")]
        public required string TonnageChange { get; init; }

        [JsonProperty(PropertyName = "pricePerTonne")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public required decimal PricePerTonne { get; init; }

        [JsonProperty(PropertyName = "producerDisposalFeeWithoutBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 2)]
        public required decimal ProducerDisposalFeeWithoutBadDebtProvision { get; init; }

        [JsonProperty(PropertyName = "badDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 2)]
        public required decimal BadDebtProvision { get; init; }

        [JsonProperty(PropertyName = "producerDisposalFeeWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 2)]
        public required decimal ProducerDisposalFeeWithBadDebtProvision { get; init; }

        [JsonProperty(PropertyName = "englandWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 2)]
        public required decimal EnglandWithBadDebtProvision { get; init; }

        [JsonProperty(PropertyName = "walesWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 2)]
        public required decimal WalesWithBadDebtProvision { get; init; }

        [JsonProperty(PropertyName = "scotlandWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 2)]
        public required decimal ScotlandWithBadDebtProvision { get; init; }

        [JsonProperty(PropertyName = "northernIrelandWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 2)]
        public required decimal NorthernIrelandWithBadDebtProvision { get; init; }
    }

}
