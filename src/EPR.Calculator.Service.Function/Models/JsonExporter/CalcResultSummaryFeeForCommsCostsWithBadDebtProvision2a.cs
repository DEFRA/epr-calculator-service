using EPR.Calculator.Service.Function.Converter;
using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2a
    {
        [JsonProperty(PropertyName = "totalProducerFeeForCommsCostsWithoutBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal TotalProducerFeeForCommsCostsWithoutBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "badDebProvisionFor2a")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal BadDebProvisionFor2a { get; set; }

        [JsonProperty(PropertyName = "totalProducerFeeForCommsCostsWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal TotalProducerFeeForCommsCostsWithBadDebtProvision { get; set; }

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

        [JsonProperty(PropertyName = "percentageOfProducerTonnageVsAllProducers")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal PercentageOfProducerTonnageVsAllProducers { get; set; }
    }
}
