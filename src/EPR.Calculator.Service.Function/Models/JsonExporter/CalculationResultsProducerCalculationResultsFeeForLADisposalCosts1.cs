using EPR.Calculator.Service.Function.Converter;
using System.Text.Json.Serialization;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalculationResultsProducerCalculationResultsFeeForLADisposalCosts1
    {
        [JsonPropertyName("totalProducerFeeForLADisposalCostsWithoutBadDebtProvision")]
        public required string TotalProducerFeeForLADisposalCostsWithoutBadDebtProvision { get; set; }

        [JsonPropertyName("badDebtProvisionForLADisposalCosts")]
        public required string BadDebtProvisionForLADisposalCosts { get; set; }

        [JsonPropertyName("totalProducerFeeForLADisposalCostsWithBadDebtProvision")]
        public required string TotalProducerFeeForLADisposalCostsWithBadDebtProvision { get; set; }

        [JsonPropertyName("englandTotalForLADisposalCostsWithBadDebtProvision")]
        public required string EnglandTotalForLADisposalCostsWithBadDebtProvision { get; set; }

        [JsonPropertyName("walesTotalForLADisposalCostsWithBadDebtProvision")]
        public required string WalesTotalForLADisposalCostsWithBadDebtProvision { get; set; }

        [JsonPropertyName("scotlandTotalForLADisposalCostsWithBadDebtProvision")]
        public required string ScotlandTotalForLADisposalCostsWithBadDebtProvision { get; set; }

        [JsonPropertyName("northernIrelandTotalForLADisposalCostsWithBadDebtProvision")]
        public required string NorthernIrelandTotalForLADisposalCostsWithBadDebtProvision { get; set; }
    }
}
