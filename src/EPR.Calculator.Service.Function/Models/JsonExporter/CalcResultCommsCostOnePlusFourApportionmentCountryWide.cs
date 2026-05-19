using System.Text.Json.Serialization;
using System.Globalization;
using EPR.Calculator.Service.Function.Utils;
using EPR.Calculator.Service.Function.Builder.CommsCost;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcResultCommsCostOnePlusFourApportionmentCountryWide
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("englandCommsCostByCountry")]
        public required string EnglandCommsCostByCountry { get; set; }

        [JsonPropertyName("walesCommsCostByCountry")]
        public required string WalesCommsCostByCountry { get; set; }

        [JsonPropertyName("scotlandCommsCostByCountry")]
        public required string ScotlandCommsCostByCountry { get; set; }

        [JsonPropertyName("northernIrelandCommsCostByCountry")]
        public required string NorthernIrelandCommsCostByCountry { get; set; }

        [JsonPropertyName("totalCommsCostByCountry")]
        public required string TotalCommsCostByCountry { get; set; }

        public static CalcResultCommsCostOnePlusFourApportionmentCountryWide? From(ByCountryCost? record)
        {
            if (record == null)
            {
                return null;
            }

            return new CalcResultCommsCostOnePlusFourApportionmentCountryWide
            {
                Name                              = CalcResultCommsCostBuilder.TwoCCommsCostByCountry,
                EnglandCommsCostByCountry         = CurrencyConverterUtils.ConvertToCurrency(record.England),
                WalesCommsCostByCountry           = CurrencyConverterUtils.ConvertToCurrency(record.Wales),
                ScotlandCommsCostByCountry        = CurrencyConverterUtils.ConvertToCurrency(record.Scotland),
                NorthernIrelandCommsCostByCountry = CurrencyConverterUtils.ConvertToCurrency(record.NorthernIreland),
                TotalCommsCostByCountry           = CurrencyConverterUtils.ConvertToCurrency(record.Total)
            };
        }
    }
}
