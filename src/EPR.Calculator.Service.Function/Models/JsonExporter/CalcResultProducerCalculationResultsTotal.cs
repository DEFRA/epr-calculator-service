using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public record CalcResultProducerCalculationResultsTotal
    {
        [JsonProperty("producerCalculationResultsTotal")]
        public string? ProducerCalculationResultsTotal { get; set; }
    }
}
