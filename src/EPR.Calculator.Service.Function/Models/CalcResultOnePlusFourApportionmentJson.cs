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
                    England         = CurrencyConverterUtils.ConvertToCurrency(calcResultOnePlusFourApportionment.LaDisposalCost.EnglandCost),
                    Wales           = CurrencyConverterUtils.ConvertToCurrency(calcResultOnePlusFourApportionment.LaDisposalCost.WalesCost),
                    Scotland        = CurrencyConverterUtils.ConvertToCurrency(calcResultOnePlusFourApportionment.LaDisposalCost.ScotlandCost),
                    NorthernIreland = CurrencyConverterUtils.ConvertToCurrency(calcResultOnePlusFourApportionment.LaDisposalCost.NorthernIrelandCost),
                    Total           = CurrencyConverterUtils.ConvertToCurrency(calcResultOnePlusFourApportionment.LaDisposalCost.TotalCost)
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
                    England         = CurrencyConverterUtils.ConvertToCurrency(calcResultOnePlusFourApportionment.TotalOnePlusFour.EnglandTotal),
                    Wales           = CurrencyConverterUtils.ConvertToCurrency(calcResultOnePlusFourApportionment.TotalOnePlusFour.WalesTotal),
                    Scotland        = CurrencyConverterUtils.ConvertToCurrency(calcResultOnePlusFourApportionment.TotalOnePlusFour.ScotlandTotal),
                    NorthernIreland = CurrencyConverterUtils.ConvertToCurrency(calcResultOnePlusFourApportionment.TotalOnePlusFour.NorthernIrelandTotal),
                    Total           = CurrencyConverterUtils.ConvertToCurrency(calcResultOnePlusFourApportionment.TotalOnePlusFour.Total)
                },

                OnePlusFourApportionmentPercentages = new CalcResultOnePlusFourApportionmentDetailJson
                {
                    England         = $"{Math.Round(calcResultOnePlusFourApportionment.OnePlusFourApportionment.EnglandTotal        , (int)DecimalPlaces.Eight)}%",
                    Wales           = $"{Math.Round(calcResultOnePlusFourApportionment.OnePlusFourApportionment.WalesTotal          , (int)DecimalPlaces.Eight)}%",
                    Scotland        = $"{Math.Round(calcResultOnePlusFourApportionment.OnePlusFourApportionment.ScotlandTotal       , (int)DecimalPlaces.Eight)}%",
                    NorthernIreland = $"{Math.Round(calcResultOnePlusFourApportionment.OnePlusFourApportionment.NorthernIrelandTotal, (int)DecimalPlaces.Eight)}%",
                    Total           = $"{Math.Round(calcResultOnePlusFourApportionment.OnePlusFourApportionment.Total               , (int)DecimalPlaces.Eight)}%",
                }
            };
        }
    }
}
