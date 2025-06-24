using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class JsonBillingFileExporter
    {
        [JsonProperty(PropertyName = "calcResultDetail")]
        public string CalcResultDetail { get; set; }

        [JsonProperty(PropertyName = "calcResultLapcapData")]
        public required string CalcResultLapcapData { get; set; }

        [JsonProperty(PropertyName = "calcResultLateReportingTonnageData")]
        public string CalcResultLateReportingTonnageData { get; set; }

        [JsonProperty(PropertyName = "onePlusFourApportionment")]
        public string OnePlusFourApportionment { get; set; }

        [JsonProperty(PropertyName = "parametersCommsCost")]
        public string ParametersCommsCost { get; set; }

        [JsonProperty(PropertyName = "calcResult2aCommsDataByMaterial")]
        public string CalcResult2aCommsDataByMaterial { get; set; }

        [JsonProperty(PropertyName = "calcResult2bCommsDataByUkWide")]
        public string CalcResult2bCommsDataByUkWide { get; set; }

        [JsonProperty(PropertyName = "calcResult2cCommsDataByCountry")]
        public string CalcResult2cCommsDataByCountry { get; set; }

        [JsonProperty(PropertyName = "calcResultLaDisposalCostData")]
        public string CalcResultLaDisposalCostData { get; set; }

        [JsonProperty(PropertyName = "cancelledProducers")]
        public string CancelledProducers { get; set; }

        [JsonProperty(PropertyName = "scaleUpProducers")]
        public string ScaleUpProducers { get; set; }

        [JsonProperty(PropertyName = "calculationResults")]
        public string CalculationResults { get; set; }
    }
}
