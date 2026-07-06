using System.Text.Encodings.Web;
using System.Text.Json;
using EPR.Calculator.Service.Function.Converter;
using EPR.Calculator.Service.Function.Features.BillingRun.Contexts;
using EPR.Calculator.Service.Function.JsonExporter.Model;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter;

public interface IBillingFileJsonWriter
{
    Task<string> WriteToString(BillingRunContext runContext, CalcResult calcResult);
}

public class BillingFileJsonWriter(IMaterialService materialService)
    : IBillingFileJsonWriter
{
    private const int DecimalPrecision = 3;

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new DecimalPrecisionConverter(DecimalPrecision) }
    };

    public async Task<string> WriteToString(BillingRunContext runContext, CalcResult calcResult)
    {
        var materials = await materialService.GetMaterials();

        if (runContext.RequiresModulation)
        {
            var content = new BillingFileJson2026
            {
                Schema                     = "https://epr.gov.uk/schemas/billing/2026/v1",
                RunId                      = calcResult.CalcResultDetail.RunId,
                FinancialYear              = calcResult.CalcResultDetail.RelativeYear.ToFinancialYear(),
                BadDebtProvisionPercentage = $"{calcResult.CalcResultParameterOtherCost.BadDebtValue:0.00}",
                ModulationResults          = CalcResultModulationResults.From(calcResult.CalcResultModulation!),
                Materials                  = MaterialPrices.FromAll(materials, calcResult).ToList(),
                Producers                  = BuildProducerGroups(calcResult.CalcResultSummary.ProducerDisposalFees, runContext.AcceptedProducerIds, materials, applyModulation: true),
            };
            return JsonSerializer.Serialize(content, JsonSerializerOptions);
        }

        var content2025 = new BillingFileJson2025
        {
            Schema                     = "https://epr.gov.uk/schemas/billing/2025/v1",
            RunId                      = calcResult.CalcResultDetail.RunId,
            FinancialYear              = calcResult.CalcResultDetail.RelativeYear.ToFinancialYear(),
            BadDebtProvisionPercentage = $"{calcResult.CalcResultParameterOtherCost.BadDebtValue:0.00}",
            Materials                  = MaterialPrices.FromAll(materials, calcResult).ToList(),
            Producers                  = calcResult.CalcResultSummary.ProducerDisposalFees
                                             .Where(p => runContext.AcceptedProducerIds.Contains(p.ProducerId))
                                             .Select(p => ProducerResult.From(p, materials, applyModulation: false))
                                             .ToList(),
        };
        return JsonSerializer.Serialize(content2025, JsonSerializerOptions);
    }

    /// <summary>
    /// Groups the flat per-row producer summary (one Level-1 row per group, plus one Level-2 row
    /// per member when the group is composite) into one <see cref="ProducerGroupResult"/> per
    /// producerID. A single-organisation group has no Level-2 rows, so its Level-1 row also serves
    /// as its sole member.
    /// </summary>
    private static List<ProducerGroupResult> BuildProducerGroups(
        IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees,
        ImmutableHashSet<int> acceptedProducerIds,
        IImmutableList<MaterialDetail> materials,
        bool applyModulation) =>
        producerDisposalFees
            .Where(p => acceptedProducerIds.Contains(p.ProducerId))
            .GroupBy(p => p.ProducerId)
            .Select(group =>
            {
                var rows          = group.ToList();
                var aggregateRow  = rows.Single(r => r.Level == "1");
                var memberRows    = rows.Where(r => r.Level == "2").ToList();

                return ProducerGroupResult.From(aggregateRow, memberRows.Count > 0 ? memberRows : [aggregateRow], materials, applyModulation);
            })
            .ToList();
}
