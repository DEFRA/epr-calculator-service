using EPR.Calculator.Service.Function.Converter;
using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2b
    {
        [JsonProperty(PropertyName = "totalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision")]
        public string? TotalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "badDebProvisionFor2b")]
        public string? BadDebtProvisionFor2bComms { get; set; }

        [JsonProperty(PropertyName = "totalProducerFeeForCommsCostsUKWideWithBadDebtProvision")]
        public string? TotalProducerFeeForCommsCostsUKWideWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "englandTotalWithBadDebtProvision")]
        public string? EnglandTotalWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "walesTotalWithBadDebtProvision")]
        public string? WalesTotalWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "scotlandTotalWithBadDebtProvision")]
        public string? ScotlandTotalWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "northernIrelandTotalWithBadDebtProvision")]
        public string? NorthernIrelandTotalWithBadDebtProvision { get; set; }
    }
}