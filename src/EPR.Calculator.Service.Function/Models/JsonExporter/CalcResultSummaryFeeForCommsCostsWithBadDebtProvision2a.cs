using EPR.Calculator.Service.Function.Converter;
using System.Text.Json.Serialization;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2a
    {
        [JsonPropertyName("totalProducerFeeForCommsCostsWithoutBadDebtProvision")]
        public string? TotalProducerFeeForCommsCostsWithoutBadDebtProvision { get; set; }

        [JsonPropertyName("badDebProvisionFor2a")]
        public string? BadDebtProvisionFor2a { get; set; }

        [JsonPropertyName("totalProducerFeeForCommsCostsWithBadDebtProvision")]
        public string? TotalProducerFeeForCommsCostsWithBadDebtProvision { get; set; }

        [JsonPropertyName("englandTotalWithBadDebtProvision")]
        public string? EnglandTotalWithBadDebtProvision { get; set; }

        [JsonPropertyName("walesTotalWithBadDebtProvision")]
        public string? WalesTotalWithBadDebtProvision { get; set; }

        [JsonPropertyName("scotlandTotalWithBadDebtProvision")]
        public string? ScotlandTotalWithBadDebtProvision { get; set; }

        [JsonPropertyName("northernIrelandTotalWithBadDebtProvision")]
        public string? NorthernIrelandTotalWithBadDebtProvision { get; set; }

        [JsonPropertyName("percentageOfProducerTonnageVsAllProducers")]
        public string? PercentageOfProducerTonnageVsAllProducers { get; set; }
    }
}
