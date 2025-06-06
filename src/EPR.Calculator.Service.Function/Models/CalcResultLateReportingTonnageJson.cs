using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultLateReportingTonnageJson
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("calcResultLateReportingTonnageDetails")]
        public List<CalcResultLateReportingTonnageDetailsJson> calcResultLateReportingTonnageDetails { get; set; }

        [JsonPropertyName("calcResultLateReportingTonnageTotal")]
        public decimal CalcResultLateReportingTonnageTotal { get; set; }
    }

    public class CalcResultLateReportingTonnageDetailsJson
    {
        [JsonPropertyName("materialName")]
        public string MaterialName { get; set; }

        [JsonPropertyName("totalLateReportingTonnage")]
        public decimal TotalLateReportingTonnage { get; set; }
    }
}
