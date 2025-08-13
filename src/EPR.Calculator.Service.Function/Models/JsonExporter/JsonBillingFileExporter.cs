using System.Text.Json.Serialization;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class JsonBillingFileExporter
    {
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
        public object? CalcResult2bCommsDataByUkWide { get; set; }

        [JsonPropertyName("calcResult2cCommsDataByCountry")]
        public object? CalcResult2cCommsDataByCountry { get; set; }

        [JsonPropertyName("calcResultLaDisposalCostData")]
        public CalcResultLaDisposalCostDataJson? CalcResultLaDisposalCostData { get; set; }

        [JsonPropertyName("cancelledProducers")]
        public CancelledProducers? CancelledProducers { get; set; }

        [JsonPropertyName("scaleUpProducers")]
        public CalcResultScaledupProducersJson? ScaleUpProducers { get; set; }

        [JsonPropertyName("calculationResults")]
        public object? CalculationResults { get; set; }
    }
}
