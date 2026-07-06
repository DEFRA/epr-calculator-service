using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

/// <summary>
/// One entry per producer group (identified by producerID), 2026 schema onwards. Financial fields
/// are the aggregate across all <see cref="Members"/> - for a single-organisation producer, Members
/// contains exactly one entry and the aggregate equals that entry.
/// </summary>
public sealed record ProducerGroupResult : ProducerFinancials
{
    [JsonPropertyName("producerID")]
    public required string ProducerID { get; init; }

    [JsonPropertyName("invoice")]
    public required ProducerInvoice Invoice { get; init; }

    [JsonPropertyName("members")]
    public required IEnumerable<ProducerMemberResult> Members { get; init; }

    public static ProducerGroupResult From(
        CalcResultSummaryProducerDisposalFees aggregateRow,
        IReadOnlyList<CalcResultSummaryProducerDisposalFees> memberRows,
        IImmutableList<MaterialDetail> materials,
        bool applyModulation)
    {
        var f = MapFrom(aggregateRow, materials, applyModulation);

        return new ProducerGroupResult
        {
            ProducerID = aggregateRow.ProducerId.ToString(),
            Invoice    = ProducerInvoice.From(aggregateRow.BillingInstructionSection),
            Members    = memberRows.Select(row => ProducerMemberResult.From(row, materials, applyModulation)).ToList(),

            TotalBill              = f.TotalBill,
            DisposalFeesByMaterial = f.DisposalFeesByMaterial,
            DisposalCosts          = f.DisposalCosts,
            CommsCostsByMaterial   = f.CommsCostsByMaterial,
            CommsCostsUKWide       = f.CommsCostsUKWide,
            CommsCostsByCountry    = f.CommsCostsByCountry,
            SaOperatingCosts       = f.SaOperatingCosts,
            LaDataPrepCosts        = f.LaDataPrepCosts,
            SaSetUpCosts           = f.SaSetUpCosts,
        };
    }
}
