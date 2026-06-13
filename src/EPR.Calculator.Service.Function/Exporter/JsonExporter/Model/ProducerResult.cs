using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

public record ProducerResult
{
    [JsonPropertyName("producerName")]
    public required string ProducerName { get; init; }

    [JsonPropertyName("producerID")]
    public required string ProducerID { get; init; }

    [JsonPropertyName("subsidiaryID")]
    public required string SubsidiaryID { get; init; }

    [JsonPropertyName("level")]
    public required int Level { get; init; }

    [JsonPropertyName("totalBill")]
    public required FeeWithCountries TotalBill { get; init; }

    [JsonPropertyName("invoice")]
    public required ProducerInvoice Invoice { get; init; }

    [JsonPropertyName("disposalFeesByMaterial")]
    public required IEnumerable<ProducerDisposalFeesWithBadDebtProvision1MaterialBreakdown> DisposalFeesByMaterial { get; init; }

    [JsonPropertyName("disposalCosts")]
    public required FeeWithCountries DisposalCosts { get; init; }

    [JsonPropertyName("commsCostsByMaterial")]
    public required FeeWithCountries CommsCostsByMaterial { get; init; }

    [JsonPropertyName("commsCostsUKWide")]
    public required FeeWithCountries CommsCostsUKWide { get; init; }

    [JsonPropertyName("commsCostsByCountry")]
    public required FeeWithCountries CommsCostsByCountry { get; init; }

    [JsonPropertyName("saOperatingCosts")]
    public required FeeWithCountries SaOperatingCosts { get; init; }

    [JsonPropertyName("laDataPrepCosts")]
    public required FeeWithCountries LaDataPrepCosts { get; init; }

    [JsonPropertyName("saSetUpCosts")]
    public required FeeWithCountries SaSetUpCosts { get; init; }

    public static ProducerResult From(
        CalcResultSummaryProducerDisposalFees producer,
        IImmutableList<MaterialDetail> materials,
        bool applyModulation) => new()
    {
        ProducerName          = producer.ProducerName,
        ProducerID            = producer.ProducerId.ToString(),
        SubsidiaryID          = producer.SubsidiaryId,
        Level                 = string.IsNullOrWhiteSpace(producer.Level) ? 1 : int.Parse(producer.Level),
        TotalBill             = FeeWithCountries.From(producer.TotalProducerBillBreakdownCosts),
        Invoice               = ProducerInvoice.From(producer.BillingInstructionSection),
        DisposalFeesByMaterial = ProducerDisposalFeesWithBadDebtProvision1
            .From(producer.ProducerDisposalFeesByMaterial, materials, producer.Level ?? "1", applyModulation)
            .MaterialBreakdown,
        DisposalCosts        = FeeWithCountries.From(producer.LADisposalCostsSection1),
        CommsCostsByMaterial  = FeeWithCountries.From(producer.CommsCostsSection2a),
        CommsCostsUKWide     = FeeWithCountries.From(producer.CommsCostsSection2b),
        CommsCostsByCountry  = FeeWithCountries.From(producer.CommsCostsSection2c),
        SaOperatingCosts     = FeeWithCountries.From(producer.SaOperatingCostsSection3),
        LaDataPrepCosts      = FeeWithCountries.From(producer.LaDataPrepSection4),
        SaSetUpCosts         = FeeWithCountries.From(producer.SaSetupCostsSection5),
    };
}
