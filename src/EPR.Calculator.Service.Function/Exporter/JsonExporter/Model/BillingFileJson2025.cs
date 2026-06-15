using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

public class BillingFileJson2025
{
    [JsonPropertyName("calcResultDetail")]
    public required CalcResultDetailJson CalcResultDetail { get; init; }

    [JsonPropertyName("parametersOther")]
    public required ParametersOtherJson ParametersOther { get; init; }

    [JsonPropertyName("materials")]
    public required IEnumerable<MaterialPrices> Materials { get; init; }

    [JsonPropertyName("producers")]
    public required IEnumerable<ProducerResult> Producers { get; init; }
}
