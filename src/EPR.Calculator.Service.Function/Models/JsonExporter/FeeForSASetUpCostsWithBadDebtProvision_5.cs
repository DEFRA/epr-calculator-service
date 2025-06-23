using EPR.Calculator.Service.Function.Converter;
using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class FeeForSASetUpCostsWithBadDebtProvision_5
    {
        [JsonProperty(PropertyName = "totalProducerOneOffFeeForSASetUpCostsWithoutBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 2)]
        public decimal TotalProducerOneOffFeeForSASetUpCostsWithoutBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "badDebtProvisionFor5")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 2)]
        public decimal BadDebtProvisionFor5 { get; set; }

        [JsonProperty(PropertyName = "totalProducerOneOffFeeForSASetUpCostsWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 2)]
        public decimal TotalProducerOneOffFeeForSASetUpCostsWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "englandTotalForSASetUpCostsWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 2)]
        public decimal EnglandTotalForSASetUpCostsWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "walesTotalForSASetUpCostsWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 2)]
        public decimal WalesTotalForSASetUpCostsWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "scotlandTotalForSASetUpCostsWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 2)]
        public decimal ScotlandTotalForSASetUpCostsWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "northernIrelandTotalForSASetUpCostsWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 2)]
        public decimal NorthernIrelandTotalForSASetUpCostsWithBadDebtProvision { get; set; }
    }
}
