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
        public decimal England { get; set; }

        [JsonProperty(PropertyName = "wales")]
        public decimal Wales { get; set; }

        [JsonProperty(PropertyName = "scotland")]
        public decimal Scotland { get; set; }

        [JsonProperty(PropertyName = "northernIreland")]
        public decimal NorthernIreland { get; set; }

        [JsonProperty(PropertyName = "total")]
        public string Total { get; set; } = string.Empty;
    }
}
