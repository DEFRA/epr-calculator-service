using System.Text.Json.Serialization;
using System.Globalization;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcResultCommsCostOnePlusFourApportionmentUKWide
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("englandCommsCostUKWide")]
        public required string EnglandCommsCostUKWide { get; set; }

        [JsonPropertyName("walesCommsCostUKWide")]
        public required string WalesCommsCostUKWide { get; set; }

        [JsonPropertyName("scotlandCommsCostUKWide")]
        public required string ScotlandCommsCostUKWide { get; set; }

        [JsonPropertyName("northernIrelandCommsCostUKWide")]
        public required string NorthernIrelandCommsCostUKWide { get; set; }

        [JsonPropertyName("totalCommsCostUKWide")]
        public required string TotalCommsCostUKWide { get; set; }

        public static CalcResultCommsCostOnePlusFourApportionmentUKWide? From(CalcResultCommsCostOnePlusFourApportionment? record)
        {
            if (record == null)
            {
                return null;
            }

            var culture = CultureInfo.CreateSpecificCulture("en-GB");
            culture.NumberFormat.CurrencySymbol = "£";
            culture.NumberFormat.CurrencyPositivePattern = 0;

            return new CalcResultCommsCostOnePlusFourApportionmentUKWide
            {
                Name = record.Name,
                EnglandCommsCostUKWide = $"{record.England.ToString("C", culture)}",
                WalesCommsCostUKWide = $"{record.Wales.ToString("C", culture)}",
                ScotlandCommsCostUKWide = $"{record.Scotland.ToString("C", culture)}",
                NorthernIrelandCommsCostUKWide = $"{record.NorthernIreland.ToString("C", culture)}",
                TotalCommsCostUKWide = $"{record.Total.ToString("C", culture)}"
             };
         }
    }
}
