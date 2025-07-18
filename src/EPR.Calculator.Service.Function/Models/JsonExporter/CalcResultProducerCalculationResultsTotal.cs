using System.Text.Json.Serialization;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public record CalcResultProducerCalculationResultsTotal
    {
        [JsonPropertyName("producerCalculationResultsTotal")]
        public string? ProducerCalculationResultsTotal { get; set; }
    }
}
