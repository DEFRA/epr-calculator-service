using EPR.Calculator.Service.Function.Converter;
using System.Text.Json.Serialization;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcResultsCommsCostsWithBadDebtProvision2c
    {
        [JsonConverter(typeof(DecimalPrecision3Converter))]
        [JsonPropertyName("totalProducerFeeForCommsCostsByCountryWithoutBadDebtProvision")]
        public string? TotalProducerFeeForCommsCostsByCountryWithoutBadDebtProvision { get; set; }

        [JsonConverter(typeof(DecimalPrecision3Converter))]
        [JsonPropertyName("badDebProvisionFor2c")]        
        public string? BadDebProvisionFor2c { get; set; }

        [JsonConverter(typeof(DecimalPrecision3Converter))]
        [JsonPropertyName("totalProducerFeeForCommsCostsByCountryWithBadDebtProvision")]
        public string? TotalProducerFeeForCommsCostsByCountryWithBadDebtProvision { get; set; }


        [JsonConverter(typeof(DecimalPrecision3Converter))]
        [JsonPropertyName("englandTotalWithBadDebtProvision")]
        public string? EnglandTotalWithBadDebtProvision { get; set; }

        [JsonConverter(typeof(DecimalPrecision3Converter))]
        [JsonPropertyName("walesTotalWithBadDebtProvision")]
        public string? WalesTotalWithBadDebtProvision { get; set; }

        [JsonConverter(typeof(DecimalPrecision3Converter))]
        [JsonPropertyName("scotlandTotalWithBadDebtProvision")]
        public string? ScotlandTotalWithBadDebtProvision { get; set; }

        [JsonConverter(typeof(DecimalPrecision3Converter))]
        [JsonPropertyName("northernIrelandTotalWithBadDebtProvision")]
        public string? NorthernIrelandTotalWithBadDebtProvision { get; set; }

    }
}
