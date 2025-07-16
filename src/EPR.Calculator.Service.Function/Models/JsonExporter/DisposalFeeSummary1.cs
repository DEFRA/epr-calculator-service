using EPR.Calculator.Service.Function.Converter;
using System.Text.Json.Serialization;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class DisposalFeeSummary1
    {
        [JsonPropertyName("totalProducerDisposalFeeWithoutBadDebtProvision")]
        public required string TotalProducerDisposalFeeWithoutBadDebtProvision { get; set; }

        [JsonPropertyName("badDebtProvision")]
        public required string BadDebtProvision { get; set; }

        [JsonPropertyName("totalProducerDisposalFeeWithBadDebtProvision")]
        public required string TotalProducerDisposalFeeWithBadDebtProvision { get; set; }

        [JsonPropertyName("englandTotal")]
        public required string EnglandTotal { get; set; }

        [JsonPropertyName("walesTotal")]
        public required string WalesTotal { get; set; }

        [JsonPropertyName("scotlandTotal")]
        public required string ScotlandTotal { get; set; }

        [JsonPropertyName("northernIrelandTotal")]
        public required string NorthernIrelandTotal { get; set; }

        [JsonPropertyName("tonnageChangeCount")]
        public required string TonnageChangeCount { get; set; }

        [JsonPropertyName("tonnageChangeAdvice")]
        public required string TonnageChangeAdvice { get; set; }
    }
}
