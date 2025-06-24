using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class JsonBillingFileExporter
    {
        [JsonProperty(PropertyName = "calcResultDetail")]
        public required string CalcResultDetail { get; set; }

        [JsonProperty(PropertyName = "calcResultLapcapData")]
        public required string CalcResultLapcapData { get; set; }

        [JsonProperty(PropertyName = "calcResultLateReportingTonnageData")]
        public required string CalcResultLateReportingTonnageData { get; set; }

        [JsonProperty(PropertyName = "onePlusFourApportionment")]
        public required string OnePlusFourApportionment { get; set; }

        [JsonProperty(PropertyName = "parametersCommsCost")]
        public required string ParametersCommsCost { get; set; }

        [JsonProperty(PropertyName = "calcResult2aCommsDataByMaterial")]
        public required string CalcResult2aCommsDataByMaterial { get; set; }

        [JsonProperty(PropertyName = "calcResult2bCommsDataByUkWide")]
        public string? CalcResult2bCommsDataByUkWide { get; set; }

        [JsonProperty(PropertyName = "calcResult2cCommsDataByCountry")]
        public string? CalcResult2cCommsDataByCountry { get; set; }

        [JsonProperty(PropertyName = "calcResultLaDisposalCostData")]
        public string? CalcResultLaDisposalCostData { get; set; }

        [JsonProperty(PropertyName = "cancelledProducers")]
        public required string CancelledProducers { get; set; }

        [JsonProperty(PropertyName = "scaleUpProducers")]
        public required string ScaleUpProducers { get; set; }

        [JsonProperty(PropertyName = "calculationResults")]
        public required string CalculationResults { get; set; }
    }
}
