using EPR.Calculator.Service.Function.Converter;
using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalculationResultsProducerCalculationResultsFeeForLADisposalCosts1
    {
        [JsonProperty(PropertyName = "totalProducerFeeForLADisposalCostsWithoutBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal TotalProducerFeeForLADisposalCostsWithoutBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "badDebtProvisionForLADisposalCosts")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal BadDebtProvisionForLADisposalCosts { get; set; }

        [JsonProperty(PropertyName = "totalProducerFeeForLADisposalCostsWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal TotalProducerFeeForLADisposalCostsWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "englandTotalForLADisposalCostsWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal EnglandTotalForLADisposalCostsWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "walesTotalForLADisposalCostsWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal WalesTotalForLADisposalCostsWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "scotlandTotalForLADisposalCostsWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal ScotlandTotalForLADisposalCostsWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "northernIrelandTotalForLADisposalCostsWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal NorthernIrelandTotalForLADisposalCostsWithBadDebtProvision { get; set; }
    }
}
