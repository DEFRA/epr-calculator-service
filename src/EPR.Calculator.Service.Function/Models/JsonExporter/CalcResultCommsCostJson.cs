using EPR.Calculator.Service.Function.Constants;
using System.Text.Json.Serialization;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcResultCommsCostJson
    {
        [JsonPropertyName(CommonConstants.OnePlusFourCommsCostApportionmentPercentages)]
        public required OnePlusFourCommsCostApportionmentPercentages OnePlusFourCommsCostApportionmentPercentages { get; set; }
    }    

    public class OnePlusFourCommsCostApportionmentPercentages
    {
        [JsonPropertyName(CommonConstants.England)]
        public string? England { get; set; }

        [JsonPropertyName(CommonConstants.Wales)]
        public string? Wales { get; set; }

        [JsonPropertyName(CommonConstants.Scotland)]
        public string? Scotland { get; set; }

        [JsonPropertyName(CommonConstants.NorthernIreland)]
        public string? NorthernIreland { get; set; }

        [JsonPropertyName(CommonConstants.Total)]
        public string? Total { get; set; }
    }
}
