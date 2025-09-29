using EPR.Calculator.Service.Function.Converter;
using System.Text.Json.Serialization;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcResultsCommsCostsWithBadDebtProvision2C
    {
        [JsonPropertyName("totalProducerFeeForCommsCostsByCountryWithoutBadDebtProvision")]
        public string? TotalProducerFeeForCommsCostsByCountryWithoutBadDebtProvision { get; set; }

        [JsonPropertyName("badDebProvisionFor2c")]        
        public string? BadDebtProvisionFor2c { get; set; }

        [JsonPropertyName("totalProducerFeeForCommsCostsByCountryWithBadDebtProvision")]
        public string? TotalProducerFeeForCommsCostsByCountryWithBadDebtProvision { get; set; }


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
