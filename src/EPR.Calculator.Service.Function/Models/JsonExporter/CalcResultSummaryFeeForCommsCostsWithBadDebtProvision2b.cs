using EPR.Calculator.Service.Function.Converter;
using System.Text.Json.Serialization;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2b
    {
        [JsonPropertyName("totalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision")]
        public string? TotalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision { get; set; }

        [JsonPropertyName("badDebtProvisionFor2b")]
        public string? BadDebtProvisionFor2b { get; set; }

        [JsonPropertyName("totalProducerFeeForCommsCostsUKWideWithBadDebtProvision")]
        public string? TotalProducerFeeForCommsCostsUKWideWithBadDebtProvision { get; set; }

        [JsonPropertyName("englandTotalWithBadDebtProvision")]
        public string? EnglandTotalWithBadDebtProvision { get; set; }

        [JsonPropertyName("walesTotalWithBadDebtProvision")]
        public string? WalesTotalWithBadDebtProvision { get; set; }

        [JsonPropertyName("scotlandTotalWithBadDebtProvision")]
        public string? ScotlandTotalWithBadDebtProvision { get; set; }

        [JsonPropertyName("northernIrelandTotalWithBadDebtProvision")]
        public string? NorthernIrelandTotalWithBadDebtProvision { get; set; }
    }
}
