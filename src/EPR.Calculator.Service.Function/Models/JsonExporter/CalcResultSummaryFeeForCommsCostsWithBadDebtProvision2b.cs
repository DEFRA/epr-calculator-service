using EPR.Calculator.Service.Function.Converter;
using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2b
    {
        [JsonProperty(PropertyName = "totalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal TotalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "badDebProvisionFor2b")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal BadDebtProvisionFor2bComms { get; set; }

        [JsonProperty(PropertyName = "totalProducerFeeForCommsCostsUKWideWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal TotalProducerFeeForCommsCostsUKWideWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "englandTotalWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal EnglandTotalWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "walesTotalWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal WalesTotalWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "scotlandTotalWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal ScotlandTotalWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "northernIrelandTotalWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal NorthernIrelandTotalWithBadDebtProvision { get; set; }
    }
}