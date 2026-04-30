using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.Models;

public class CalcResultOnePlusFourApportionmentJson
{
    [JsonPropertyName("oneFeeForLADisposalCosts")]
    public CalcResultOnePlusFourApportionmentDetailJson OneFeeForLADisposalCosts { get; set; } = new();

    [JsonPropertyName("fourLADataPrepCharge")]
    public CalcResultOnePlusFourApportionmentDetailJson FourLADataPrepCharge { get; set; } = new();

    [JsonPropertyName("totalOfonePlusFour")]
    public CalcResultOnePlusFourApportionmentDetailJson TotalOfonePlusFour { get; set; } = new();

    [JsonPropertyName("onePlusFourApportionmentPercentages")]
    public CalcResultOnePlusFourApportionmentDetailJson OnePlusFourApportionmentPercentages { get; set; } = new();

    public static CalcResultOnePlusFourApportionmentJson From(CalcResultOnePlusFourApportionment calcResultOnePlusFourApportionment)
    {
        if (calcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails is null)
        {
            return new CalcResultOnePlusFourApportionmentJson();
        }

        var i = 1;

        return new CalcResultOnePlusFourApportionmentJson
        {
            OneFeeForLADisposalCosts = calcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Where(t => t.OrderId == i && !string.IsNullOrWhiteSpace(t.Name)).Select(y => new CalcResultOnePlusFourApportionmentDetailJson
            {
                England = CurrencyConverterUtils.ConvertToCurrency(y.EnglandTotal),
                Scotland = CurrencyConverterUtils.ConvertToCurrency(y.ScotlandTotal),
                Wales = CurrencyConverterUtils.ConvertToCurrency(y.WalesTotal),
                NorthernIreland = CurrencyConverterUtils.ConvertToCurrency(y.NorthernIrelandTotal),
                Total = y.Total
            }).SingleOrDefault() ?? new CalcResultOnePlusFourApportionmentDetailJson(),

            FourLADataPrepCharge = calcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Where(t => t.OrderId == i + 1).Select(y => new CalcResultOnePlusFourApportionmentDetailJson
            {
                England = CurrencyConverterUtils.ConvertToCurrency(y.EnglandTotal),
                Scotland = CurrencyConverterUtils.ConvertToCurrency(y.ScotlandTotal),
                Wales = CurrencyConverterUtils.ConvertToCurrency(y.WalesTotal),
                NorthernIreland = CurrencyConverterUtils.ConvertToCurrency(y.NorthernIrelandTotal),
                Total = y.Total
            }).SingleOrDefault() ?? new CalcResultOnePlusFourApportionmentDetailJson(),

            TotalOfonePlusFour = calcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Where(t => t.OrderId == i + 2).Select(y => new CalcResultOnePlusFourApportionmentDetailJson
            {
                England = CurrencyConverterUtils.ConvertToCurrency(y.EnglandTotal),
                Scotland = CurrencyConverterUtils.ConvertToCurrency(y.ScotlandTotal),
                Wales = CurrencyConverterUtils.ConvertToCurrency(y.WalesTotal),
                NorthernIreland = CurrencyConverterUtils.ConvertToCurrency(y.NorthernIrelandTotal),
                Total = y.Total
            }).SingleOrDefault() ?? new CalcResultOnePlusFourApportionmentDetailJson(),

            OnePlusFourApportionmentPercentages = calcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Where(t => t.OrderId == i + 3).Select(y => new CalcResultOnePlusFourApportionmentDetailJson
            {
                England = $"{Math.Round(y.EnglandTotal, (int)DecimalPlaces.Eight).ToString()}%",
                Scotland = $"{Math.Round(y.ScotlandTotal, (int)DecimalPlaces.Eight).ToString()}%",
                Wales = $"{Math.Round(y.WalesTotal, (int)DecimalPlaces.Eight).ToString()}%",
                NorthernIreland = $"{Math.Round(y.NorthernIrelandTotal, (int)DecimalPlaces.Eight).ToString()}%",
                Total = y.Total
            }).SingleOrDefault() ?? new CalcResultOnePlusFourApportionmentDetailJson()
        };
    }
}