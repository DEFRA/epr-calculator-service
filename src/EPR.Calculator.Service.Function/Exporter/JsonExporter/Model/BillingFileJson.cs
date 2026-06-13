using System.Globalization;
using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Features.BillingRun.Contexts;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

public class BillingFileJson
{
    [JsonPropertyName("runId")]
    public int RunId { get; set; }

    [JsonPropertyName("financialYear")]
    public string? FinancialYear { get; set; }

    [JsonPropertyName("badDebtProvisionPercentage")]
    public string? BadDebtProvisionPercentage { get; set; }

    [JsonPropertyName("calcResultDetail")]
    public CalcResultDetailJson? CalcResultDetail { get; set; }

    [JsonPropertyName("calcResultLapcapData")]
    public object? CalcResultLapcapData { get; set; }

    [JsonPropertyName("calcResultLateReportingTonnageData")]
    public CalcResultLateReportingTonnageJson? CalcResultLateReportingTonnageData { get; set; }

    [JsonPropertyName("parametersOther")]
    public CalcResultParametersOtherJson? ParametersOther { get; set; }

    [JsonPropertyName("onePlusFourApportionment")]
    public CalcResultOnePlusFourApportionmentJson? OnePlusFourApportionment { get; set; }

    [JsonPropertyName("parametersCommsCost")]
    public CalcResultCommsCostJson? ParametersCommsCost { get; set; }

    [JsonPropertyName("calcResult2aCommsDataByMaterial")]
    public CalcResult2ACommsDataByMaterial? CalcResult2aCommsDataByMaterial { get; set; }

    [JsonPropertyName("calcResult2bCommsDataByUkWide")]
    public CalcResultCommsCostOnePlusFourApportionmentUKWide? CalcResult2bCommsDataByUkWide { get; set; }

    [JsonPropertyName("calcResult2cCommsDataByCountry")]
    public CalcResultCommsCostOnePlusFourApportionmentCountryWide? CalcResult2cCommsDataByCountry { get; set; }

    [JsonPropertyName("calcResultLaDisposalCostData")]
    public CalcResultLaDisposalCostDataJson? CalcResultLaDisposalCostData { get; set; }

    [JsonPropertyName("cancelledProducers")]
    public CancelledProducers? CancelledProducers { get; set; }

    [JsonPropertyName("scaleUpProducers")]
    public CalcResultScaledupProducersJson? ScaleUpProducers { get; set; }

    [JsonPropertyName("modulationResults")]
    public CalcResultModulationResults? ModulationResults { get; set; }

    [JsonPropertyName("materials")]
    public IEnumerable<MaterialPrices>? Materials { get; set; }

    [JsonPropertyName("producers")]
    public IEnumerable<ProducerResult>? Producers { get; set; }

    public static BillingFileJson From(BillingRunContext runContext, CalcResult calcResult, IImmutableList<MaterialDetail> materials)
    {
        return new BillingFileJson {
            RunId                              = calcResult.CalcResultDetail.RunId,
            FinancialYear                      = calcResult.CalcResultDetail.RelativeYear.ToFinancialYear(),
            BadDebtProvisionPercentage         = calcResult.CalcResultParameterOtherCost.BadDebtValue.ToString("F2", CultureInfo.InvariantCulture),
            CalcResultDetail                   = CalcResultDetailJson.From(calcResult.CalcResultDetail),
            CalcResultLapcapData               = CalcResultLapcapDataJson.From(calcResult.CalcResultLapcapData, materials),
            CalcResultLateReportingTonnageData = CalcResultLateReportingTonnageJson.From(calcResult.CalcResultLateReportingTonnageData, materials),
            ParametersOther                    = CalcResultParametersOtherJson.From(calcResult.CalcResultParameterOtherCost),
            OnePlusFourApportionment           = CalcResultOnePlusFourApportionmentJson.From(calcResult.CalcResultOnePlusFourApportionment),
            ParametersCommsCost                = CalcResultCommsCostJson.From(calcResult.CalcResultCommsCostReportDetail),
            CalcResult2aCommsDataByMaterial    = CalcResult2ACommsDataByMaterial.From(materials, calcResult.CalcResultCommsCostReportDetail.ByMaterial, calcResult.CalcResultCommsCostReportDetail.Total),
            CalcResult2bCommsDataByUkWide      = CalcResultCommsCostOnePlusFourApportionmentUKWide.From(calcResult.CalcResultCommsCostReportDetail.CommsCostUkWide),
            CalcResult2cCommsDataByCountry     = CalcResultCommsCostOnePlusFourApportionmentCountryWide.From(calcResult.CalcResultCommsCostReportDetail.CommsCostByCountry),
            CalcResultLaDisposalCostData       = CalcResultLaDisposalCostDataJson.From(calcResult.CalcResultLaDisposalCostData.ByMaterial, calcResult.CalcResultLaDisposalCostData.Total, materials),
            CancelledProducers                 = CancelledProducers.From(calcResult.CalcResultCancelledProducers),
            ScaleUpProducers                   = CalcResultScaledupProducersJson.From(runContext, calcResult.CalcResultScaledupProducers, materials),
            ModulationResults                  = runContext.RequiresModulation && calcResult.CalcResultModulation is not null ? CalcResultModulationResults.From(calcResult.CalcResultModulation) : null,
            Materials                          = MaterialPrices.FromAll(materials, calcResult),
            Producers                          = calcResult.CalcResultSummary.ProducerDisposalFees
                                                    .Where(p => runContext.AcceptedProducerIds.Contains(p.ProducerId))
                                                    .Select(p => ProducerResult.From(p, materials, runContext.RequiresModulation))
                                                    .ToList(),
        };
    }
}
