using System.Text.Json.Serialization;
using System.Globalization;

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

        public static CalcResultCommsCostOnePlusFourApportionmentCountryWide? From(CalcResultCommsCostOnePlusFourApportionment? record)
        {
            if (record == null)
            {
                return null;
            }

            var culture = CultureInfo.CreateSpecificCulture("en-GB");
            culture.NumberFormat.CurrencySymbol = "£";

            return new CalcResultCommsCostOnePlusFourApportionmentCountryWide
            {
                Name = record.Name,
                EnglandCommsCostByCountry = $"{record.England.ToString("C", culture)}",
                WalesCommsCostByCountry = $"{record.Wales.ToString("C", culture)}",
                ScotlandCommsCostByCountry = $"{record.Scotland.ToString("C", culture)}",
                NorthernIrelandCommsCostByCountry = $"{record.NorthernIreland.ToString("C", culture)}",
                TotalCommsCostByCountry = $"{record.Total.ToString("C", culture)}"
            };
        }
    }
}
