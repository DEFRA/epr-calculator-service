using EPR.Calculator.Service.Function.Converter;
using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class TotalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2c
    {
        [JsonProperty(PropertyName = "totalFeeWithBadDebtProvision")]
        public required string TotalFeeWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "producerPercentageOfOverallProducerCost")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 8)]
        public required string ProducerPercentageOfOverallProducerCost { get; set; }
    }
}
