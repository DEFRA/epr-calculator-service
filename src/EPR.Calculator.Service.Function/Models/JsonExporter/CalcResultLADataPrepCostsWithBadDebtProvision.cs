using EPR.Calculator.Service.Function.Converter;
using Newtonsoft.Json;


namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class FeeForLADataPrepCostsWithBadDebtProvision_4
    {
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        [JsonProperty(PropertyName = "totalProducerFeeForLADataPrepCostsWithoutBadDebtProvision")]
        public decimal TotalProducerFeeForLADataPrepCostsWithoutBadDebtProvision {get; set;}

        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        [JsonProperty(PropertyName = "badDebtProvisionFor4")]
        public decimal BadDebtProvisionFor4 {get; set;}

        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        [JsonProperty(PropertyName = "totalProducerFeeForLADataPrepCostsWithBadDebtProvision")]
        public decimal TotalProducerFeeForLADataPrepCostsWithBadDebtProvision {get; set;}

        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        [JsonProperty(PropertyName = "englandTotalForLADataPrepCostsWithBadDebtProvision")]
        public decimal EnglandTotalForLADataPrepCostsWithBadDebtProvision {get; set;}

        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        [JsonProperty(PropertyName = "walesTotalForLADataPrepCostsWithBadDebtProvision")]
        public decimal WalesTotalForLADataPrepCostsWithBadDebtProvision {get; set;}

        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        [JsonProperty(PropertyName = "scotlandTotalForLADataPrepCostsWithBadDebtProvision")]
        public decimal ScotlandTotalForLADataPrepCostsWithBadDebtProvision {get; set;}

        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        [JsonProperty(PropertyName = "northernIrelandTotalForLADataPrepCostsWithBadDebtProvision")]
        public decimal NorthernIrelandTotalForLADataPrepCostsWithBadDebtProvision { get; set;}
    }
}