using EPR.Calculator.Service.Function.Converter;
using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class FeeForSASetUpCostsWithBadDebtProvision_5
    {
        [JsonProperty(PropertyName = "totalProducerOneOffFeeForSASetUpCostsWithoutBadDebtProvision")]
        public string? TotalProducerOneOffFeeForSASetUpCostsWithoutBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "badDebtProvisionFor5")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 2)]
        public string? BadDebtProvisionFor5 { get; set; }

        [JsonProperty(PropertyName = "totalProducerOneOffFeeForSASetUpCostsWithBadDebtProvision")]
        public string? TotalProducerOneOffFeeForSASetUpCostsWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "englandTotalForSASetUpCostsWithBadDebtProvision")]
        public string? EnglandTotalForSASetUpCostsWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "walesTotalForSASetUpCostsWithBadDebtProvision")]
        public string? WalesTotalForSASetUpCostsWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "scotlandTotalForSASetUpCostsWithBadDebtProvision")]
        public string? ScotlandTotalForSASetUpCostsWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "northernIrelandTotalForSASetUpCostsWithBadDebtProvision")]
        public string? NorthernIrelandTotalForSASetUpCostsWithBadDebtProvision { get; set; }
    }
}
