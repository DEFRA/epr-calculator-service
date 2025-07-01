using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class JsonBillingFileExporter
    {
        [JsonProperty(PropertyName = "calcResultDetail")]
        public CalcResultDetailJson? CalcResultDetail { get; set; }

        [JsonProperty(PropertyName = "calcResultLapcapData")]
        public object? CalcResultLapcapData { get; set; }

        [JsonProperty(PropertyName = "calcResultLateReportingTonnageData")]
        public CalcResultLateReportingTonnageJson? CalcResultLateReportingTonnageData { get; set; }

        [JsonProperty(PropertyName = "parametersOther")]
        public CalcResultParametersOtherJson? ParametersOther { get; set; }

        [JsonProperty(PropertyName = "onePlusFourApportionment")]
        public CalcResultOnePlusFourApportionmentJson? OnePlusFourApportionment { get; set; }

        [JsonProperty(PropertyName = "parametersCommsCost")]
        public CalcResultCommsCostJson? ParametersCommsCost { get; set; }

        [JsonProperty(PropertyName = "calcResult2aCommsDataByMaterial")]
        public CalcResult2ACommsDataByMaterial? CalcResult2aCommsDataByMaterial { get; set; }

        [JsonProperty(PropertyName = "calcResult2bCommsDataByUkWide")]
        public object? CalcResult2bCommsDataByUkWide { get; set; }

        [JsonProperty(PropertyName = "calcResult2cCommsDataByCountry")]
        public object? CalcResult2cCommsDataByCountry { get; set; }

        [JsonProperty(PropertyName = "calcResultLaDisposalCostData")]
        public CalcResultLaDisposalCostDataJson? CalcResultLaDisposalCostData { get; set; }

        [JsonProperty(PropertyName = "cancelledProducers")]
        public CancelledProducers? CancelledProducers { get; set; }

        [JsonProperty(PropertyName = "scaleUpProducers")]
        public CalcResultScaledupProducersJson? ScaleUpProducers { get; set; }

        [JsonProperty(PropertyName = "calculationResults")]
        public object? CalculationResults { get; set; }
    }
}
