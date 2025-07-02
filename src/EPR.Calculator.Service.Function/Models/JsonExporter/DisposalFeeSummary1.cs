using EPR.Calculator.Service.Function.Converter;
using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class DisposalFeeSummary1
    {
        [JsonProperty(PropertyName = "totalProducerDisposalFeeWithoutBadDebtProvision")]
        public required string TotalProducerDisposalFeeWithoutBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "badDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 2)]
        public required string BadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "totalProducerDisposalFeeWithBadDebtProvision")]
        public required string TotalProducerDisposalFeeWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "englandTotal")]
        public required string EnglandTotal { get; set; }

        [JsonProperty(PropertyName = "walesTotal")]
        public required string WalesTotal { get; set; }

        [JsonProperty(PropertyName = "scotlandTotal")]
        public required string ScotlandTotal { get; set; }

        [JsonProperty(PropertyName = "northernIrelandTotal")]
        public required string NorthernIrelandTotal { get; set; }

        [JsonProperty(PropertyName = "tonnageChangeCount")]
        public required string TonnageChangeCount { get; set; }

        [JsonProperty(PropertyName = "tonnageChangeAdvice")]
        public required string TonnageChangeAdvice { get; set; }
    }
}
