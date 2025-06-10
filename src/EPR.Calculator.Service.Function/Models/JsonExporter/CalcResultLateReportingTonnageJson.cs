using EPR.Calculator.Service.Function.Converter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcResultLateReportingTonnageJson
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "calcResultLateReportingTonnageDetails")]
        public List<CalcResultLateReportingTonnageDetailsJson> calcResultLateReportingTonnageDetails { get; set; }

        [JsonProperty(PropertyName = "calcResultLateReportingTonnageTotal")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal CalcResultLateReportingTonnageTotal { get; set; }
    }

    public class CalcResultLateReportingTonnageDetailsJson
    {
        [JsonProperty(PropertyName = "materialName")]
        public string MaterialName { get; set; }


        [JsonProperty(PropertyName = "totalLateReportingTonnage")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal TotalLateReportingTonnage { get; set; }
    }
}
