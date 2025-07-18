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
        [JsonPropertyName("england")]
        public string? England { get; set; }

        [JsonPropertyName("wales")]
        public string? Wales { get; set; }

        [JsonPropertyName("scotland")]
        public string? Scotland { get; set; }

        [JsonPropertyName("northernIreland")]
        public string? NorthernIreland { get; set; }

        [JsonPropertyName("total")]
        public string? Total { get; set; }
    }
}
