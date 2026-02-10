using System;
using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Models;

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

            return new CalcResultCommsCostOnePlusFourApportionmentUKWide
            {
                Name = record.Name,
                EnglandCommsCostUKWide = record.England,
                WalesCommsCostUKWide = record.Wales,
                ScotlandCommsCostUKWide = record.Scotland,
                NorthernIrelandCommsCostUKWide = record.NorthernIreland,
                TotalCommsCostUKWide = record.Total,
             };
         }
    }
}