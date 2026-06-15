using System.Globalization;
using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

public record Fee
{
    [JsonPropertyName("base")]
    public required string Base { get; init; }

    [JsonPropertyName("badDebtProvision")]
    public required string BadDebtProvision { get; init; }

    public static Fee From(CalcResultSummaryBadDebtProvision s) => new()
    {
        Base             = s.FeeWithoutBadDebtProvision.ToString("F2", CultureInfo.InvariantCulture),
        BadDebtProvision = s.BadDebtProvision.ToString("F2", CultureInfo.InvariantCulture),
    };
}
