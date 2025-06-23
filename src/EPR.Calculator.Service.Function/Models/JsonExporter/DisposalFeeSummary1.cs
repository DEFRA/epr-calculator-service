using EPR.Calculator.Service.Function.Converter;
using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class DisposalFeeSummary1
    {
        [JsonProperty(PropertyName = "totalProducerDisposalFeeWithoutBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 2)]
        public decimal TotalProducerDisposalFeeWithoutBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "badDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 2)]
        public decimal BadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "totalProducerDisposalFeeWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 2)]
        public decimal TotalProducerDisposalFeeWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "englandTotal")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 2)]
        public decimal EnglandTotal { get; set; }

        [JsonProperty(PropertyName = "walesTotal")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 2)]
        public decimal WalesTotal { get; set; }

        [JsonProperty(PropertyName = "scotlandTotal")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 2)]
        public decimal ScotlandTotal { get; set; }

        [JsonProperty(PropertyName = "northernIrelandTotal")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 2)]
        public decimal NorthernIrelandTotal { get; set; }

        [JsonProperty(PropertyName = "tonnageChangeCount")]
        public required string TonnageChangeCount { get; set; }

        [JsonProperty(PropertyName = "tonnageChangeAdvice")]
        public required string TonnageChangeAdvice { get; set; }
    }
}
