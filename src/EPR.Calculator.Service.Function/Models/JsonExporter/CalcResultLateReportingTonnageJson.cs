using EPR.Calculator.Service.Function.Converter;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcResultLateReportingTonnageJson
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("calcResultLateReportingTonnageDetails")]
        public List<CalcResultLateReportingTonnageDetailsJson> calcResultLateReportingTonnageDetails { get; set; }

        [JsonPropertyName("calcResultLateReportingTonnageTotal")]
        [JsonConverter(typeof(DecimalPrecision3Converter))]
        public decimal CalcResultLateReportingTonnageTotal { get; set; }
    }

    public class CalcResultLateReportingTonnageDetailsJson
    {
        [JsonPropertyName("materialName")]
        public string MaterialName { get; set; }


        [JsonPropertyName("totalLateReportingTonnage")]
        [JsonConverter(typeof(DecimalPrecision3Converter))]
        public decimal TotalLateReportingTonnage { get; set; }
    }
}
