using EPR.Calculator.Service.Function.Builder.CommsCost;
using EPR.Calculator.Service.Function.Constants;
using System.Linq;
using System.Text.Json.Serialization;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcResultCommsCostJson
    {
        [JsonPropertyName(CommonConstants.OnePlusFourCommsCostApportionmentPercentages)]
        public required OnePlusFourCommsCostApportionmentPercentages OnePlusFourCommsCostApportionmentPercentages { get; set; }

        public static CalcResultCommsCostJson From(CalcResultCommsCost calcResultCommsCost)
        {
            var onePlusFourApportionment = calcResultCommsCost.CalcResultCommsCostOnePlusFourApportionment.SingleOrDefault(x => x.Name == CalcResultCommsCostBuilder.OnePlusFourApportionment);

            if (onePlusFourApportionment == null)
            {
                return new CalcResultCommsCostJson {  
                    OnePlusFourCommsCostApportionmentPercentages = new OnePlusFourCommsCostApportionmentPercentages() 
                };
            }

            return new CalcResultCommsCostJson
            {
                OnePlusFourCommsCostApportionmentPercentages = OnePlusFourCommsCostApportionmentPercentages.From(onePlusFourApportionment)
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

        public static OnePlusFourCommsCostApportionmentPercentages From(CalcResultCommsCostOnePlusFourApportionment dataRow)
        {
            string AppendPercent(string input)
            {
                if (string.IsNullOrWhiteSpace(input))
                {
                    return "0.00%";
                }

                string trimmedInput = input.Trim();
                return trimmedInput.EndsWith('%') ? trimmedInput : trimmedInput + "%";
            }


            return new OnePlusFourCommsCostApportionmentPercentages
            {
                England = AppendPercent(dataRow.England),
                Wales = AppendPercent(dataRow.Wales),
                Scotland = AppendPercent(dataRow.Scotland),
                NorthernIreland = AppendPercent(dataRow.NorthernIreland),
                Total = AppendPercent(dataRow.Total)
            };
        }
    }

            
}
