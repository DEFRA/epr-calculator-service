using System;
using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Models;

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

            return new CalcResultCommsCostOnePlusFourApportionmentCountryWide
            {
                Name = record.Name,
                EnglandCommsCostByCountry = record.England,
                WalesCommsCostByCountry = record.Wales,
                ScotlandCommsCostByCountry = record.Scotland,
                NorthernIrelandCommsCostByCountry = record.NorthernIreland,
                TotalCommsCostByCountry = record.Total,
            };
        }
    }
}