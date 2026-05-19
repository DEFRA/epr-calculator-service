using System.Globalization;
using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultOnePlusFourApportionmentJson
    {
        [JsonPropertyName("oneFeeForLADisposalCosts")]
        public CalcResultOnePlusFourApportionmentDetailJson OneFeeForLADisposalCosts { get; set; } = new CalcResultOnePlusFourApportionmentDetailJson();

        [JsonPropertyName("fourLADataPrepCharge")]
        public CalcResultOnePlusFourApportionmentDetailJson FourLADataPrepCharge { get; set; } = new CalcResultOnePlusFourApportionmentDetailJson();

        [JsonPropertyName("totalOfonePlusFour")]
        public CalcResultOnePlusFourApportionmentDetailJson TotalOfonePlusFour { get; set; } = new CalcResultOnePlusFourApportionmentDetailJson();

        [JsonPropertyName("onePlusFourApportionmentPercentages")]
        public CalcResultOnePlusFourApportionmentDetailJson OnePlusFourApportionmentPercentages { get; set; } = new CalcResultOnePlusFourApportionmentDetailJson();

        public static CalcResultOnePlusFourApportionmentJson From(CalcResultOnePlusFourApportionment calcResultOnePlusFourApportionment)
        {
            return new CalcResultOnePlusFourApportionmentJson
            {
                OneFeeForLADisposalCosts = new CalcResultOnePlusFourApportionmentDetailJson
                {
                    England         = CurrencyConverterUtils.ConvertToCurrency(calcResultOnePlusFourApportionment.LaDisposalCost.England),
                    Wales           = CurrencyConverterUtils.ConvertToCurrency(calcResultOnePlusFourApportionment.LaDisposalCost.Wales),
                    Scotland        = CurrencyConverterUtils.ConvertToCurrency(calcResultOnePlusFourApportionment.LaDisposalCost.Scotland),
                    NorthernIreland = CurrencyConverterUtils.ConvertToCurrency(calcResultOnePlusFourApportionment.LaDisposalCost.NorthernIreland),
                    Total           = CurrencyConverterUtils.ConvertToCurrency(calcResultOnePlusFourApportionment.LaDisposalCost.Total)
                },

                FourLADataPrepCharge = new CalcResultOnePlusFourApportionmentDetailJson
                {
                    England         = CurrencyConverterUtils.ConvertToCurrency(calcResultOnePlusFourApportionment.LADataPrepCharge.England),
                    Wales           = CurrencyConverterUtils.ConvertToCurrency(calcResultOnePlusFourApportionment.LADataPrepCharge.Wales),
                    Scotland        = CurrencyConverterUtils.ConvertToCurrency(calcResultOnePlusFourApportionment.LADataPrepCharge.Scotland),
                    NorthernIreland = CurrencyConverterUtils.ConvertToCurrency(calcResultOnePlusFourApportionment.LADataPrepCharge.NorthernIreland),
                    Total           = CurrencyConverterUtils.ConvertToCurrency(calcResultOnePlusFourApportionment.LADataPrepCharge.Total)
                },

                TotalOfonePlusFour = new CalcResultOnePlusFourApportionmentDetailJson
                {
                    England         = CurrencyConverterUtils.ConvertToCurrency(calcResultOnePlusFourApportionment.TotalOnePlusFour.England),
                    Wales           = CurrencyConverterUtils.ConvertToCurrency(calcResultOnePlusFourApportionment.TotalOnePlusFour.Wales),
                    Scotland        = CurrencyConverterUtils.ConvertToCurrency(calcResultOnePlusFourApportionment.TotalOnePlusFour.Scotland),
                    NorthernIreland = CurrencyConverterUtils.ConvertToCurrency(calcResultOnePlusFourApportionment.TotalOnePlusFour.NorthernIreland),
                    Total           = CurrencyConverterUtils.ConvertToCurrency(calcResultOnePlusFourApportionment.TotalOnePlusFour.Total)
                },

                OnePlusFourApportionmentPercentages = new CalcResultOnePlusFourApportionmentDetailJson
                {
                    England         = $"{calcResultOnePlusFourApportionment.OnePlusFourApportionment.England        .ToString("N", new NumberFormatInfo { NumberDecimalDigits = 8 })}%",
                    Wales           = $"{calcResultOnePlusFourApportionment.OnePlusFourApportionment.Wales          .ToString("N", new NumberFormatInfo { NumberDecimalDigits = 8 })}%",
                    Scotland        = $"{calcResultOnePlusFourApportionment.OnePlusFourApportionment.Scotland       .ToString("N", new NumberFormatInfo { NumberDecimalDigits = 8 })}%",
                    NorthernIreland = $"{calcResultOnePlusFourApportionment.OnePlusFourApportionment.NorthernIreland.ToString("N", new NumberFormatInfo { NumberDecimalDigits = 8 })}%",
                    Total           = $"{calcResultOnePlusFourApportionment.OnePlusFourApportionment.Total          .ToString("N", new NumberFormatInfo { NumberDecimalDigits = 8 })}%",
                }
            };
        }
    }
}
