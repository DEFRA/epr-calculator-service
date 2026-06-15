using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Converter;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

public record ProducerDisposalFeesWithBadDebtProvision1
{
    [JsonPropertyName("materialBreakdown")]
    public required IEnumerable<ProducerDisposalFeesWithBadDebtProvision1MaterialBreakdown> MaterialBreakdown { get; set; }

    public static ProducerDisposalFeesWithBadDebtProvision1 From(
        Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial> producerDisposalFeesByMaterial,
        IImmutableList<MaterialDetail> materials,
        string level,
        bool applyModulation
    ) => new ()
        {
            MaterialBreakdown = producerDisposalFeesByMaterial
                .Select(x => ProducerDisposalFeesWithBadDebtProvision1MaterialBreakdown.From(x.Value, materials.Single(m => m.Code == x.Key), level, applyModulation))
                .ToList()
        };
}

public abstract record ValueOrModulated<TValue, TModulated>
{
    public TValue? Value { get; init; }

    public TModulated? Modulated { get; init; }

    protected static TWrapper FromValue<TWrapper>(TValue value)
        where TWrapper : ValueOrModulated<TValue, TModulated>, new()
        => new() { Value = value };

    protected static TWrapper FromModulated<TWrapper>(TModulated value)
        where TWrapper : ValueOrModulated<TValue, TModulated>, new()
        => new() { Modulated = value };
}

[SuppressMessage(
    "Major Code Smell",
    "S2436:Types and methods should not have too many generic parameters",
    Justification = "Strongly typed JsonConverter requires wrapper, value, and modulated types for reusable serialization.")]
public abstract class ValueOrModulatedConverter<TWrapper, TValue, TModulated>: JsonConverter<TWrapper>
    where TWrapper : ValueOrModulated<TValue, TModulated>
{
    public override TWrapper Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotImplementedException();

    public override void Write(Utf8JsonWriter writer, TWrapper value, JsonSerializerOptions options)
    {
        if (value.Modulated is not null)
        {
            JsonSerializer.Serialize(writer, value.Modulated, options);
            return;
        }

        JsonSerializer.Serialize(writer, value.Value, options);
    }
}

public class DecimalOrModulatedTonnageBreakdownConverter
    : ValueOrModulatedConverter<DecimalOrModulatedTonnageBreakdown, decimal, ModulatedTonnageBreakdown>{}

[JsonConverter(typeof(DecimalOrModulatedTonnageBreakdownConverter))]
public record DecimalOrModulatedTonnageBreakdown
    : ValueOrModulated<decimal, ModulatedTonnageBreakdown>
{
    public static implicit operator DecimalOrModulatedTonnageBreakdown(decimal value)
        => FromValue<DecimalOrModulatedTonnageBreakdown>(value);

    public static implicit operator DecimalOrModulatedTonnageBreakdown(ModulatedTonnageBreakdown value)
        => FromModulated<DecimalOrModulatedTonnageBreakdown>(value);
}

public record ModulatedTonnageBreakdown
{
    [JsonPropertyName("total")]
    [JsonConverter(typeof(DecimalPrecision3Converter))]
    public decimal Total { get; init; }

    [JsonPropertyName("red")]
    [JsonConverter(typeof(DecimalPrecision3Converter))]
    public decimal Red { get; init; }

    [JsonPropertyName("amber")]
    [JsonConverter(typeof(DecimalPrecision3Converter))]
    public decimal Amber { get; init; }

    [JsonPropertyName("green")]
    [JsonConverter(typeof(DecimalPrecision3Converter))]
    public decimal Green { get; init; }

    [JsonPropertyName("redMedical")]
    [JsonConverter(typeof(DecimalPrecision3Converter))]
    public decimal RedMedical { get; init; }

    [JsonPropertyName("amberMedical")]
    [JsonConverter(typeof(DecimalPrecision3Converter))]
    public decimal AmberMedical { get; init; }

    [JsonPropertyName("greenMedical")]
    [JsonConverter(typeof(DecimalPrecision3Converter))]
    public decimal GreenMedical { get; init; }

    public static ModulatedTonnageBreakdown From(decimal total, Dictionary<RagRating, decimal> values)
    {
        return new ModulatedTonnageBreakdown
        {
            Total        = total,
            Red          = values.GetValueOrDefault(RagRating.Red),
            Amber        = values.GetValueOrDefault(RagRating.Amber),
            Green        = values.GetValueOrDefault(RagRating.Green),
            RedMedical   = values.GetValueOrDefault(RagRating.RedMedical),
            AmberMedical = values.GetValueOrDefault(RagRating.AmberMedical),
            GreenMedical = values.GetValueOrDefault(RagRating.GreenMedical)
        };
    }
}

public class DecimalOrCombinedModulatedTonnageBreakdownConverter
    : ValueOrModulatedConverter<DecimalOrCombinedModulatedTonnageBreakdown, decimal, CombinedModulatedTonnageBreakdown> {}

[JsonConverter(typeof(DecimalOrCombinedModulatedTonnageBreakdownConverter))]
public record DecimalOrCombinedModulatedTonnageBreakdown
    : ValueOrModulated<decimal, CombinedModulatedTonnageBreakdown>
{
    public static implicit operator DecimalOrCombinedModulatedTonnageBreakdown(decimal value)
        => FromValue<DecimalOrCombinedModulatedTonnageBreakdown>(value);

    public static implicit operator DecimalOrCombinedModulatedTonnageBreakdown(CombinedModulatedTonnageBreakdown value)
        => FromModulated<DecimalOrCombinedModulatedTonnageBreakdown>(value);
}

public record CombinedModulatedTonnageBreakdown
{
    [JsonPropertyName("total")]
    [JsonConverter(typeof(DecimalPrecision3Converter))]
    public decimal Total { get; init; }

    [JsonPropertyName("redAndRedMedical")]
    [JsonConverter(typeof(DecimalPrecision3Converter))]
    public decimal RedAndRedMedical { get; init; }

    [JsonPropertyName("amberAndAmberMedical")]
    [JsonConverter(typeof(DecimalPrecision3Converter))]
    public decimal AmberAndAmberMedical { get; init; }

    [JsonPropertyName("greenAndGreenMedical")]
    [JsonConverter(typeof(DecimalPrecision3Converter))]
    public decimal GreenAndGreenMedical { get; init; }
}

public class StringOrCombinedModulatedCostBreakdownConverter
    : ValueOrModulatedConverter<StringOrCombinedModulatedCostBreakdown, string , CombinedModulatedCostBreakdown> {}

[JsonConverter(typeof(StringOrCombinedModulatedCostBreakdownConverter))]
public record StringOrCombinedModulatedCostBreakdown
    : ValueOrModulated<string, CombinedModulatedCostBreakdown>
{
    public static implicit operator StringOrCombinedModulatedCostBreakdown(string value)
        => FromValue<StringOrCombinedModulatedCostBreakdown>(value);

    public static implicit operator StringOrCombinedModulatedCostBreakdown(CombinedModulatedCostBreakdown value)
        => FromModulated<StringOrCombinedModulatedCostBreakdown>(value);
}

public record CombinedModulatedCostBreakdown
{
    [JsonPropertyName("total")]
    public required string Total { get; init; }

    [JsonPropertyName("redAndRedMedical")]
    public string RedAndRedMedical { get; init; } = default!;

    [JsonPropertyName("amberAndAmberMedical")]
    public string AmberAndAmberMedical { get; init; } = default!;

    [JsonPropertyName("greenAndGreenMedical")]
    public string GreenAndGreenMedical { get; init; } = default!;
}

public record ProducerDisposalFeesWithBadDebtProvision1MaterialBreakdown
{
    [JsonPropertyName("material")]
    public required string MaterialName { get; init; }

    [JsonPropertyName("previousInvoicedTonnage")]
    public required string PreviousInvoicedTonnage { get; init; }

    [JsonPropertyName("householdPackagingWasteTonnage")]
    public required DecimalOrModulatedTonnageBreakdown HouseholdPackagingWasteTonnage { get; init; }

    [JsonPropertyName("publicBinTonnage")]
    public required DecimalOrModulatedTonnageBreakdown PublicBinTonnage { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("householdDrinksContainersTonnageGlass")]
    public DecimalOrModulatedTonnageBreakdown? HouseholdDrinksContainersTonnageGlass { get; set; }

    [JsonPropertyName("totalTonnage")]
    public required DecimalOrModulatedTonnageBreakdown TotalTonnage { get; init; }

    [JsonPropertyName("selfManagedConsumerWasteTonnage")]
    [JsonConverter(typeof(DecimalPrecision3Converter))]
    public required decimal SelfManagedConsumerWasteTonnage { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("actionedSelfManagedConsumerWasteTonnage")]
    public CombinedModulatedTonnageBreakdown? ActionedSelfManagedConsumerWasteTonnage { get; init; }

    [JsonPropertyName("netTonnage")]
    public required DecimalOrCombinedModulatedTonnageBreakdown NetTonnage { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("residualSelfManagedConsumerWasteTonnage")]
    [JsonConverter(typeof(DecimalPrecision3Converter))]
    public decimal? ResidualSelfManagedConsumerWasteTonnage { get; init; }

    [JsonPropertyName("feeWithoutBadDebt")]
    public required StringOrCombinedModulatedCostBreakdown FeeWithoutBadDebt { get; init; }

    [JsonPropertyName("tonnageChange")]
    public required string TonnageChange { get; init; }

    [JsonPropertyName("badDebtProvision")]
    public required string BadDebtProvision { get; init; }

    [JsonPropertyName("fee")]
    public required string Fee { get; init; }

    [JsonPropertyName("england")]
    public required string England { get; init; }

    [JsonPropertyName("wales")]
    public required string Wales { get; init; }

    [JsonPropertyName("scotland")]
    public required string Scotland { get; init; }

    [JsonPropertyName("northernIreland")]
    public required string NorthernIreland { get; init; }

    private static decimal RoundMoney(decimal value, int precision = 2)
        => Math.Round(value, precision, MidpointRounding.ToEven);

    private static string F2(decimal value) => value.ToString("F2", CultureInfo.InvariantCulture);

    public static ProducerDisposalFeesWithBadDebtProvision1MaterialBreakdown From(
        CalcResultSummaryProducerDisposalFeesByMaterial producerTonnage,
        MaterialDetail material,
        string level,
        bool applyModulation)
    {
        if (!applyModulation)
        {
            return new ProducerDisposalFeesWithBadDebtProvision1MaterialBreakdown
            {
                MaterialName              = material.Name,
                PreviousInvoicedTonnage   = level == "1" ? producerTonnage.PreviousInvoicedTonnage?.ToString() ?? CommonConstants.Hyphen : CommonConstants.Hyphen,
                TonnageChange             = level == "1" ? producerTonnage.TonnageChange?.ToString()           ?? CommonConstants.Hyphen : CommonConstants.Hyphen,
                BadDebtProvision          = F2(producerTonnage.BadDebtProvision),
                Fee                       = F2(producerTonnage.ProducerDisposalFeeWithBadDebtProvision.Total),
                England                   = F2(producerTonnage.ProducerDisposalFeeWithBadDebtProvision.England),
                Wales                     = F2(producerTonnage.ProducerDisposalFeeWithBadDebtProvision.Wales),
                Scotland                  = F2(producerTonnage.ProducerDisposalFeeWithBadDebtProvision.Scotland),
                NorthernIreland           = F2(producerTonnage.ProducerDisposalFeeWithBadDebtProvision.NorthernIreland),
                HouseholdPackagingWasteTonnage        = producerTonnage.HouseholdPackagingWasteTonnage,
                PublicBinTonnage                      = producerTonnage.PublicBinTonnage,
                HouseholdDrinksContainersTonnageGlass = material.Code == MaterialCodes.Glass ? producerTonnage.HouseholdDrinksContainersTonnage : null,
                TotalTonnage                          = producerTonnage.TotalReportedTonnage,
                SelfManagedConsumerWasteTonnage       = producerTonnage.SelfManagedConsumerWasteTonnage,
                NetTonnage                            = producerTonnage.NetReportedTonnage.total ?? 0,
                FeeWithoutBadDebt                     = F2(RoundMoney(producerTonnage.ProducerDisposalFee.total ?? 0)),
            };
        }

        return new ProducerDisposalFeesWithBadDebtProvision1MaterialBreakdown
        {
            MaterialName              = material.Name,
            PreviousInvoicedTonnage   = level == "1" ? producerTonnage.PreviousInvoicedTonnage?.ToString() ?? CommonConstants.Hyphen : CommonConstants.Hyphen,
            TonnageChange             = level == "1" ? producerTonnage.TonnageChange?.ToString()           ?? CommonConstants.Hyphen : CommonConstants.Hyphen,
            BadDebtProvision          = F2(producerTonnage.BadDebtProvision),
            Fee                       = F2(producerTonnage.ProducerDisposalFeeWithBadDebtProvision.Total),
            England                   = F2(producerTonnage.ProducerDisposalFeeWithBadDebtProvision.England),
            Wales                     = F2(producerTonnage.ProducerDisposalFeeWithBadDebtProvision.Wales),
            Scotland                  = F2(producerTonnage.ProducerDisposalFeeWithBadDebtProvision.Scotland),
            NorthernIreland           = F2(producerTonnage.ProducerDisposalFeeWithBadDebtProvision.NorthernIreland),
            HouseholdPackagingWasteTonnage        = ModulatedTonnageBreakdown.From(producerTonnage.HouseholdPackagingWasteTonnage, producerTonnage.HouseholdPackagingWasteTonnageRagRating),
            PublicBinTonnage                      = ModulatedTonnageBreakdown.From(producerTonnage.PublicBinTonnage, producerTonnage.PublicBinTonnageRagRating),
            HouseholdDrinksContainersTonnageGlass = material.Code == MaterialCodes.Glass
                                                        ? (DecimalOrModulatedTonnageBreakdown)ModulatedTonnageBreakdown.From(producerTonnage.HouseholdDrinksContainersTonnage, producerTonnage.HouseholdDrinksContainersTonnageRagRating)
                                                        : null,
            TotalTonnage                          = ModulatedTonnageBreakdown.From(producerTonnage.TotalReportedTonnage, producerTonnage.TotalReportedTonnageRagRating),
            SelfManagedConsumerWasteTonnage       = producerTonnage.SelfManagedConsumerWasteTonnage,
            ActionedSelfManagedConsumerWasteTonnage = new CombinedModulatedTonnageBreakdown
            {
                Total                = producerTonnage.ActionedSelfManagedConsumerWasteTonnage.total ?? 0,
                RedAndRedMedical     = producerTonnage.ActionedSelfManagedConsumerWasteTonnage.red   ?? 0,
                AmberAndAmberMedical = producerTonnage.ActionedSelfManagedConsumerWasteTonnage.amber ?? 0,
                GreenAndGreenMedical = producerTonnage.ActionedSelfManagedConsumerWasteTonnage.green ?? 0
            },
            NetTonnage = new CombinedModulatedTonnageBreakdown
            {
                Total                = producerTonnage.NetReportedTonnage.total ?? 0,
                RedAndRedMedical     = producerTonnage.NetReportedTonnage.red   ?? 0,
                AmberAndAmberMedical = producerTonnage.NetReportedTonnage.amber ?? 0,
                GreenAndGreenMedical = producerTonnage.NetReportedTonnage.green ?? 0
            },
            ResidualSelfManagedConsumerWasteTonnage = producerTonnage.ResidualSelfManagedConsumerWasteTonnage,
            FeeWithoutBadDebt = new CombinedModulatedCostBreakdown
            {
                Total                = F2(RoundMoney(producerTonnage.ProducerDisposalFee.total ?? 0)),
                RedAndRedMedical     = F2(RoundMoney(producerTonnage.ProducerDisposalFee.red   ?? 0)),
                AmberAndAmberMedical = F2(RoundMoney(producerTonnage.ProducerDisposalFee.amber ?? 0)),
                GreenAndGreenMedical = F2(RoundMoney(producerTonnage.ProducerDisposalFee.green ?? 0)),
            },
        };
    }
}
