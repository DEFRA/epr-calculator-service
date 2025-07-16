using System.Text.Json.Serialization;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcResultSAOperatingCostsWithBadDebtProvision
    {

        [JsonPropertyName("totalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithoutBadDebtProvision")]
        public required string  TotalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithoutBadDebtProvision { get; set; }

        [JsonPropertyName("badDebtProvisionFor3")]
        public required string BadDebtProvisionFor3 { get; set; }

        [JsonPropertyName("totalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithBadDebtProvision")]
        public required string TotalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithBadDebtProvision { get; set; }

        [JsonPropertyName("englandTotalForSAOperatingCostsWithBadDebtProvision")]
        public required string EnglandTotalForSAOperatingCostsWithBadDebtProvision { get; set; }

        [JsonPropertyName("walesTotalForSAOperatingCostsWithBadDebtProvision")]
        public required string WalesTotalForSAOperatingCostsWithBadDebtProvision { get; set; }

        [JsonPropertyName("scotlandTotalForSAOperatingCostsWithBadDebtProvision")]
        public required string ScotlandTotalForSAOperatingCostsWithBadDebtProvision { get; set; }

        [JsonPropertyName("northernIrelandTotalForSAOperatingCostsWithBadDebtProvision")]
        public required string NorthernIrelandTotalForSAOperatingCostsWithBadDebtProvision { get; set; }
    }
}
