using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Builder.CommsCost;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcResultCommsCostJson
    {
        [JsonPropertyName(CommonConstants.OnePlusFourCommsCostApportionmentPercentages)]
        public required OnePlusFourCommsCostApportionmentPercentages OnePlusFourCommsCostApportionmentPercentages { get; set; }

        public static CalcResultCommsCostJson From(CalcResultCommsCost calcResultCommsCost)
        {
            return new CalcResultCommsCostJson {
                OnePlusFourCommsCostApportionmentPercentages = OnePlusFourCommsCostApportionmentPercentages.From(calcResultCommsCost.OnePlusFourApportionment)
            };
        }
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

        public static OnePlusFourCommsCostApportionmentPercentages From(ByCountryApportionment dataRow)
        {
            string AppendPercent(decimal input)
            {
                return $"{input:0.00}%";
            }

            return new OnePlusFourCommsCostApportionmentPercentages
            {
                England         = AppendPercent(dataRow.England),
                Wales           = AppendPercent(dataRow.Wales),
                Scotland        = AppendPercent(dataRow.Scotland),
                NorthernIreland = AppendPercent(dataRow.NorthernIreland),
                Total           = AppendPercent(dataRow.Total)
            };
        }
    }
}
