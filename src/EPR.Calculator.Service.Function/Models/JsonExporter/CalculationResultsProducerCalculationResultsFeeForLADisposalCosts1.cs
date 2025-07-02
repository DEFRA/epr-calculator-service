using EPR.Calculator.Service.Function.Converter;
using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalculationResultsProducerCalculationResultsFeeForLADisposalCosts1
    {
        [JsonProperty(PropertyName = "totalProducerFeeForLADisposalCostsWithoutBadDebtProvision")]
        public required string TotalProducerFeeForLADisposalCostsWithoutBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "badDebtProvisionForLADisposalCosts")]
        public required string BadDebtProvisionForLADisposalCosts { get; set; }

        [JsonProperty(PropertyName = "totalProducerFeeForLADisposalCostsWithBadDebtProvision")]
        public required string TotalProducerFeeForLADisposalCostsWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "englandTotalForLADisposalCostsWithBadDebtProvision")]
        public required string EnglandTotalForLADisposalCostsWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "walesTotalForLADisposalCostsWithBadDebtProvision")]
        public required string WalesTotalForLADisposalCostsWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "scotlandTotalForLADisposalCostsWithBadDebtProvision")]
        public required string ScotlandTotalForLADisposalCostsWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "northernIrelandTotalForLADisposalCostsWithBadDebtProvision")]
        public required string NorthernIrelandTotalForLADisposalCostsWithBadDebtProvision { get; set; }
    }
}
