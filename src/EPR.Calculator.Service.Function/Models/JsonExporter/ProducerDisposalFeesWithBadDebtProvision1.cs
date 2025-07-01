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
        public required string PricePerTonne { get; init; }

        [JsonProperty(PropertyName = "producerDisposalFeeWithoutBadDebtProvision")]
        public required string ProducerDisposalFeeWithoutBadDebtProvision { get; init; }

        [JsonProperty(PropertyName = "badDebtProvision")]
        public required string BadDebtProvision { get; init; }

        [JsonProperty(PropertyName = "producerDisposalFeeWithBadDebtProvision")]
        public required string ProducerDisposalFeeWithBadDebtProvision { get; init; }

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
