using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

public class BillingFileJson2026
{
    [JsonPropertyName("$schema")]
    public required string Schema { get; init; }

    [JsonPropertyName("runId")]
    public required int RunId { get; init; }

    [JsonPropertyName("financialYear")]
    public required string FinancialYear { get; init; }

    [JsonPropertyName("badDebtProvisionPercentage")]
    public required string BadDebtProvisionPercentage { get; init; }

    [JsonPropertyName("modulationResults")]
    public required CalcResultModulationResults ModulationResults { get; init; }

    [JsonPropertyName("materials")]
    public required IEnumerable<MaterialPrices> Materials { get; init; }

    [JsonPropertyName("producers")]
    public required IEnumerable<ProducerResult> Producers { get; init; }
}
