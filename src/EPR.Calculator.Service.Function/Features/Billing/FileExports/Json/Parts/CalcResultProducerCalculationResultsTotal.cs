using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Features.Billing.FileExports.Json.Parts
{
    public record CalcResultProducerCalculationResultsTotal
    {
        [JsonPropertyName("producerCalculationResultsTotal")]
        public string? ProducerCalculationResultsTotal { get; set; }

        public static CalcResultProducerCalculationResultsTotal? From(CalcResultSummary calcResultSummary)
        {
            // specified in user story as remaining null
            return null;
        }
    }
}
