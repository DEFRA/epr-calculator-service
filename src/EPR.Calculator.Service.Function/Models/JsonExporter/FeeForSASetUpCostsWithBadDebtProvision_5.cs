using EPR.Calculator.Service.Function.Converter;
using System.Text.Json.Serialization;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class FeeForSaSetUpCostsWithBadDebtProvision5
    {
        [JsonPropertyName("totalProducerOneOffFeeForSASetUpCostsWithoutBadDebtProvision")]
        public string? TotalProducerOneOffFeeForSASetUpCostsWithoutBadDebtProvision { get; set; }

        [JsonPropertyName("badDebtProvisionFor5")]
        public string? BadDebtProvisionFor5 { get; set; }

        [JsonPropertyName("totalProducerOneOffFeeForSASetUpCostsWithBadDebtProvision")]
        public string? TotalProducerOneOffFeeForSASetUpCostsWithBadDebtProvision { get; set; }

        [JsonPropertyName("englandTotalForSASetUpCostsWithBadDebtProvision")]
        public string? EnglandTotalForSASetUpCostsWithBadDebtProvision { get; set; }

        [JsonPropertyName("walesTotalForSASetUpCostsWithBadDebtProvision")]
        public string? WalesTotalForSASetUpCostsWithBadDebtProvision { get; set; }

        [JsonPropertyName("scotlandTotalForSASetUpCostsWithBadDebtProvision")]
        public string? ScotlandTotalForSASetUpCostsWithBadDebtProvision { get; set; }

        [JsonPropertyName("northernIrelandTotalForSASetUpCostsWithBadDebtProvision")]
        public string? NorthernIrelandTotalForSASetUpCostsWithBadDebtProvision { get; set; }
    }
}
