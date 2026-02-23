using EPR.Calculator.Service.Function.Converter;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcResultLateReportingTonnageJson
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("calcResultLateReportingTonnageDetails")]
        public List<CalcResultLateReportingTonnageDetailsJson> calcResultLateReportingTonnageDetails { get; set; } = null!;

        [JsonPropertyName("calcResultLateReportingTonnageTotal")]
        [JsonConverter(typeof(DecimalPrecision3Converter))]
        public decimal CalcResultLateReportingTonnageTotal { get; set; }

        public static CalcResultLateReportingTonnageJson From(CalcResultLateReportingTonnage? calcResultLateReportingTonnage)
        {
            if (calcResultLateReportingTonnage is null) return new CalcResultLateReportingTonnageJson();
            string Total = "total";
            return new CalcResultLateReportingTonnageJson
            {
                Name = "Late Reporting Tonnage",
                calcResultLateReportingTonnageDetails = calcResultLateReportingTonnage.CalcResultLateReportingTonnageDetails
                .Where(n=>n.Name.Trim().ToLower() != Total)
                .Select(t => new CalcResultLateReportingTonnageDetailsJson { MaterialName = t.Name, TotalLateReportingTonnage = t.TotalLateReportingTonnage }).ToList(),
                
                CalcResultLateReportingTonnageTotal = calcResultLateReportingTonnage.CalcResultLateReportingTonnageDetails
                .Where(n => n.Name.Trim().ToLower() != Total)
                .Sum(t => t.TotalLateReportingTonnage)
            };
        }
    }

    public class CalcResultLateReportingTonnageDetailsJson
    {
        [JsonPropertyName("materialName")]
        public string MaterialName { get; set; } = null!;


        [JsonPropertyName("totalLateReportingTonnage")]
        [JsonConverter(typeof(DecimalPrecision3Converter))]
        public decimal TotalLateReportingTonnage { get; set; }
    }
}
