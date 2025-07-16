using System.Text.Json.Serialization;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class TotalProducerFeeWithBadDebtProvisionFor2con_1_2a_2b_2c
    {
        [JsonPropertyName("totalFeeWithBadDebtProvision")]
        public required string TotalFeeWithBadDebtProvision { get; set; }

        [JsonPropertyName("producerPercentageOfOverallProducerCost")]
        public required string ProducerPercentageOfOverallProducerCost { get; set; }
    }
}
