using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Converter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcResultCommsCostJson
    {
        [JsonProperty(CommonConstants.ParametersCommsCost)]
        public ParametersCommsCost ParametersCommsCost { get; set; }
    }

    public class ParametersCommsCost
    {
        [JsonProperty(CommonConstants.OnePlusFourCommsCostApportionmentPercentages)]
        public OnePlusFourCommsCostApportionmentPercentages Percentages { get; set; }
    }

    public class OnePlusFourCommsCostApportionmentPercentages
    {
        [JsonProperty(CommonConstants.England)]
        public string England { get; set; }

        [JsonProperty(CommonConstants.Wales)]
        public string Wales { get; set; }

        [JsonProperty(CommonConstants.Scotland)]
        public string Scotland { get; set; }

        [JsonProperty(CommonConstants.NorthernIreland)]
        public string NorthernIreland { get; set; }

        [JsonProperty(CommonConstants.Total)]
        public string Total { get; set; }
    }
}
