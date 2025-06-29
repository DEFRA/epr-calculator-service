using EPR.Calculator.Service.Function.Converter;
using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2a
    {
        [JsonProperty(PropertyName = "totalProducerFeeForCommsCostsWithoutBadDebtProvision")]
        public string? TotalProducerFeeForCommsCostsWithoutBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "badDebProvisionFor2a")]
        public string? BadDebProvisionFor2a { get; set; }

        [JsonProperty(PropertyName = "totalProducerFeeForCommsCostsWithBadDebtProvision")]
        public string? TotalProducerFeeForCommsCostsWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "englandTotalWithBadDebtProvision")]
        public string? EnglandTotalWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "walesTotalWithBadDebtProvision")]
        public string? WalesTotalWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "scotlandTotalWithBadDebtProvision")]
        public string? ScotlandTotalWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "northernIrelandTotalWithBadDebtProvision")]
        public string? NorthernIrelandTotalWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "percentageOfProducerTonnageVsAllProducers")]
        public string? PercentageOfProducerTonnageVsAllProducers { get; set; }
    }
}
