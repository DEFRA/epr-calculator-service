using System.Text.Json.Serialization;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class TotalProducerFeeWithBadDebtProvisionFor2Con12A2B2CMapper
    {
        [JsonPropertyName("totalFeeWithBadDebtProvision")]
        public required string TotalFeeWithBadDebtProvision { get; set; }

        [JsonPropertyName("producerPercentageOfOverallProducerCost")]
        public required string ProducerPercentageOfOverallProducerCost { get; set; }
    }
}
