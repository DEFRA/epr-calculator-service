using System.Text.Encodings.Web;
using System.Text.Json;
using EPR.Calculator.Service.Function.Builder.CommsCost;
using EPR.Calculator.Service.Function.Converters;
using EPR.Calculator.Service.Function.Features.Billing.Contexts;
using EPR.Calculator.Service.Function.Features.Billing.FileExports.Json.Parts;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;

namespace EPR.Calculator.Service.Function.Features.Billing.FileExports.Json;

public interface IBillingFileJsonWriter
{
    Task<string> WriteToString(BillingRunContext runContext, CalcResult calcResult);
}

public class BillingFileJsonWriter(IMaterialService materialService)
    : IBillingFileJsonWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new DecimalPrecisionConverter(3) }
    };

    public async Task<string> WriteToString(BillingRunContext runContext, CalcResult calcResult)
    {
        var materials = await materialService.GetMaterials();
        var billingFileObject = GetBillingFileObject(runContext, calcResult, materials);

        return JsonSerializer.Serialize(billingFileObject, JsonOptions);
    }

    private static BillingFileJson GetBillingFileObject(BillingRunContext runContext, CalcResult results, ImmutableList<MaterialDetail> materials)
    {
        return new BillingFileJson
        {
            CalcResultDetail = CalcResultDetailJson.From(results.CalcResultDetail),
            CalcResultLapcapData = CalcResultLapcapDataJson.From(results.CalcResultLapcapData),
            CalcResultLateReportingTonnageData = CalcResultLateReportingTonnageJson.From(results.CalcResultLateReportingTonnageData),
            ParametersOther = CalcResultParametersOtherJson.From(results.CalcResultParameterOtherCost),
            OnePlusFourApportionment = CalcResultOnePlusFourApportionmentJson.From(results.CalcResultOnePlusFourApportionment),
            ParametersCommsCost = CalcResultCommsCostJson.From(results.CalcResultCommsCostReportDetail),
            CalcResult2aCommsDataByMaterial = CalcResult2ACommsDataByMaterial.From(results.CalcResultCommsCostReportDetail.CalcResultCommsCostCommsCostByMaterial),
            CalcResult2bCommsDataByUkWide = CalcResultCommsCostOnePlusFourApportionmentUKWide.From(results.CalcResultCommsCostReportDetail.CommsCostByCountry.SingleOrDefault(r => r.Name == CalcResultCommsCostBuilder.TwoBCommsCostUkWide)),
            CalcResult2cCommsDataByCountry = CalcResultCommsCostOnePlusFourApportionmentCountryWide.From(results.CalcResultCommsCostReportDetail.CommsCostByCountry.SingleOrDefault(r => r.Name == CalcResultCommsCostBuilder.TwoCCommsCostByCountry)),
            CalcResultLaDisposalCostData = CalcResultLaDisposalCostDataJson.From(results.CalcResultLaDisposalCostData.CalcResultLaDisposalCostDetails),
            CancelledProducers = CancelledProducers.From(results.CalcResultCancelledProducers),
            ScaleUpProducers = CalcResultScaledupProducersJson.From(runContext, results.CalcResultScaledupProducers, materials),
            CalculationResults = CalculationResultsJson.From(runContext, results.CalcResultSummary, materials)
        };
    }
}
