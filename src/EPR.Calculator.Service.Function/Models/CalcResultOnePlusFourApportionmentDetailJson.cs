using EPR.Calculator.Service.Function.Converter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Models
{
    public record CalcResultOnePlusFourApportionmentDetailJson
    {
        [JsonProperty(PropertyName = "england")]        
        public string England { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "wales")]
        public string Wales { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "scotland")]
        public string Scotland { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "northernIreland")]
        public string NorthernIreland { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "total")]
        public string Total { get; set; } = string.Empty;
    }
}