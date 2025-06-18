using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2a
    {
        [JsonProperty(PropertyName = "totalProducerFeeForCommsCostsWithoutBadDebtProvision")]
        public decimal TotalProducerFeeForCommsCostsWithoutBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "badDebProvisionFor2a")]
        public decimal BadDebProvisionFor2a { get; set; }

        [JsonProperty(PropertyName = "totalProducerFeeForCommsCostsWithBadDebtProvision")]
        public decimal TotalProducerFeeForCommsCostsWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "englandTotalWithBadDebtProvision")]
        public decimal EnglandTotalWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "walesTotalWithBadDebtProvision")]
        public decimal WalesTotalWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "scotlandTotalWithBadDebtProvision")]
        public decimal ScotlandTotalWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "northernIrelandTotalWithBadDebtProvision")]
        public decimal NorthernIrelandTotalWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "percentageOfProducerTonnageVsAllProducers")]
        public decimal PercentageOfProducerTonnageVsAllProducers { get; set; }
    }
}
