using System.Globalization;
using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

public record FeeWithCountries
{
    [JsonPropertyName("base")]
    public required string Base { get; init; }

    [JsonPropertyName("badDebtProvision")]
    public required string BadDebtProvision { get; init; }

    [JsonPropertyName("total")]
    public required string Total { get; init; }

    [JsonPropertyName("england")]
    public required string England { get; init; }

    [JsonPropertyName("wales")]
    public required string Wales { get; init; }

    [JsonPropertyName("scotland")]
    public required string Scotland { get; init; }

    [JsonPropertyName("northernIreland")]
    public required string NorthernIreland { get; init; }

    public static FeeWithCountries From(CalcResultSummaryBadDebtProvision s) => new()
    {
        Base             = s.FeeWithoutBadDebtProvision.ToString("F2", CultureInfo.InvariantCulture),
        BadDebtProvision = s.BadDebtProvision.ToString("F2", CultureInfo.InvariantCulture),
        Total            = s.FeeWithBadDebtProvision.Total.ToString("F2", CultureInfo.InvariantCulture),
        England          = s.FeeWithBadDebtProvision.England.ToString("F2", CultureInfo.InvariantCulture),
        Wales            = s.FeeWithBadDebtProvision.Wales.ToString("F2", CultureInfo.InvariantCulture),
        Scotland         = s.FeeWithBadDebtProvision.Scotland.ToString("F2", CultureInfo.InvariantCulture),
        NorthernIreland  = s.FeeWithBadDebtProvision.NorthernIreland.ToString("F2", CultureInfo.InvariantCulture),
    };
}
