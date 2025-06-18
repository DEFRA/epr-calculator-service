using EPR.Calculator.Service.Function.Converter;
using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class TotalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2c
    {
        [JsonProperty(PropertyName = "totalFeeWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal TotalFeeWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "producerPercentageOfOverallProducerCost")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 8)]
        public decimal ProducerPercentageOfOverallProducerCost { get; set; }
    }
}
