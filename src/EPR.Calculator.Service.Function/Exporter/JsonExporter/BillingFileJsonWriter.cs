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
                RunId                      = calcResult.CalcResultDetail.RunId,
                FinancialYear              = calcResult.CalcResultDetail.RelativeYear.ToFinancialYear(),
                BadDebtProvisionPercentage = $"{calcResult.CalcResultParameterOtherCost.BadDebtValue:0.00}",
                ModulationResults          = CalcResultModulationResults.From(calcResult.CalcResultModulation!),
                Materials                  = MaterialPrices.FromAll(materials, calcResult).ToList(),
                Producers                  = calcResult.CalcResultSummary.ProducerDisposalFees
                                                 .Where(p => runContext.AcceptedProducerIds.Contains(p.ProducerId))
                                                 .Select(p => ProducerResult.From(p, materials, applyModulation: true))
                                                 .ToList(),
            };
            return JsonSerializer.Serialize(content, JsonSerializerOptions);
        }

        var content2025 = new BillingFileJson2025
        {
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
}
