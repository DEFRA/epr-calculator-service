using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

public class BillingFileJson2026
{
    [JsonPropertyName("calcResultDetail")]
    public required CalcResultDetailJson CalcResultDetail { get; init; }

    [JsonPropertyName("parametersOther")]
    public required ParametersOtherJson ParametersOther { get; init; }

    [JsonPropertyName("modulationResults")]
    public required CalcResultModulationResults ModulationResults { get; init; }

    [JsonPropertyName("materials")]
    public required IEnumerable<MaterialPrices> Materials { get; init; }

    [JsonPropertyName("producers")]
    public required IEnumerable<ProducerResult> Producers { get; init; }
}

public class CalcResultDetailJson
{
    [JsonPropertyName("runId")]
    public required int RunId { get; init; }

    [JsonPropertyName("financialYear")]
    public required string FinancialYear { get; init; }
}

public class ParametersOtherJson
{
    [JsonPropertyName("sixBadDebtProvision")]
    public required BadDebtProvisionJson SixBadDebtProvision { get; init; }
}

public class BadDebtProvisionJson
{
    [JsonPropertyName("percentage")]
    public required string Percentage { get; init; }
}
