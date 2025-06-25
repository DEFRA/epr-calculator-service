using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class JsonBillingFileExporter
    {
        [JsonProperty(PropertyName = "calcResultDetail")]
        public required CalcResultDetailJson CalcResultDetail { get; set; }

        [JsonProperty(PropertyName = "calcResultLapcapData")]
        public required object CalcResultLapcapData { get; set; }

        [JsonProperty(PropertyName = "calcResultLateReportingTonnageData")]
        public required CalcResultLateReportingTonnageJson CalcResultLateReportingTonnageData { get; set; }

        [JsonProperty(PropertyName = "onePlusFourApportionment")]
        public required CalcResultOnePlusFourApportionmentJson OnePlusFourApportionment { get; set; }

        [JsonProperty(PropertyName = "parametersCommsCost")]
        public required CalcResultCommsCostJson ParametersCommsCost { get; set; }

        [JsonProperty(PropertyName = "calcResult2aCommsDataByMaterial")]
        public required CalcResult2ACommsDataByMaterial CalcResult2aCommsDataByMaterial { get; set; }

        [JsonProperty(PropertyName = "calcResult2bCommsDataByUkWide")]
        public string? CalcResult2bCommsDataByUkWide { get; set; }

        [JsonProperty(PropertyName = "calcResult2cCommsDataByCountry")]
        public string? CalcResult2cCommsDataByCountry { get; set; }

        [JsonProperty(PropertyName = "calcResultLaDisposalCostData")]
        public CalcResultLaDisposalCostDataJson? CalcResultLaDisposalCostData { get; set; }

        [JsonProperty(PropertyName = "cancelledProducers")]
        public required CancelledProducers CancelledProducers { get; set; }

        [JsonProperty(PropertyName = "scaleUpProducers")]
        public required CalcResultScaledupProducersJson ScaleUpProducers { get; set; }

        [JsonProperty(PropertyName = "calculationResults")]
        public required object CalculationResults { get; set; }
    }
}
