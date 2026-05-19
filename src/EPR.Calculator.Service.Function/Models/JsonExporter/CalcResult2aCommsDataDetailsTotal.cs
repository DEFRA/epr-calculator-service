using System.Text.Json.Serialization;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public record CalcResult2ACommsDataDetailsTotal
    {
        [JsonPropertyName("total")]

        public required string Total { get; init; }

        [JsonPropertyName("englandCommsCostTotal")]

        public required string EnglandCommsCostTotal { get; init; }

        [JsonPropertyName("walesCommsCostTotal")]

        public required string WalesCommsCostTotal { get; init; }

        [JsonPropertyName("scotlandCommsCostTotal")]

        public required string ScotlandCommsCostTotal { get; init; }

        [JsonPropertyName("northernIrelandCommsCostTotal")]

        public required string NorthernIrelandCommsCostTotal { get; init; }

        [JsonPropertyName("totalCommsCostTotal")]

        public required string TotalCommsCostTotal { get; init; }

        [JsonPropertyName("producerHouseholdPackagingWasteTonnageTotal")]

        public required decimal HouseholdPackagingWasteTonnageTotal { get; init; }

        [JsonPropertyName("publicBinTonnage")]

        public required decimal PublicBinTonnageTotal { get; init; }

        [JsonPropertyName("householdDrinksContainersTonnageTotal")]

        public required decimal HouseholdDrinksContainersTonnageTotal { get; init; }

        [JsonPropertyName("lateReportingTonnageTotal")]

        public required decimal LateReportingTonnageTotal { get; init; }

        [JsonPropertyName("totalTonnageTotal")]

        public required decimal TotalTonnageTotal { get; init; }

        public static CalcResult2ACommsDataDetailsTotal From(CalcResultCommsCostCommsCostByMaterial commsCostByMaterial)
        {
            return new CalcResult2ACommsDataDetailsTotal
            {
                Total                         = "Total",
                EnglandCommsCostTotal         = CurrencyConverterUtils.ConvertToCurrency(commsCostByMaterial.EnglandCost),
                WalesCommsCostTotal           = CurrencyConverterUtils.ConvertToCurrency(commsCostByMaterial.WalesCost),
                ScotlandCommsCostTotal        = CurrencyConverterUtils.ConvertToCurrency(commsCostByMaterial.ScotlandCost),
                NorthernIrelandCommsCostTotal = CurrencyConverterUtils.ConvertToCurrency(commsCostByMaterial.NorthernIrelandCost),
                HouseholdPackagingWasteTonnageTotal   = Math.Round(commsCostByMaterial.HouseholdPackagingWasteTonnage, 3),
                PublicBinTonnageTotal                 = Math.Round(commsCostByMaterial.PublicBinTonnage, 3),
                HouseholdDrinksContainersTonnageTotal = Math.Round(commsCostByMaterial.HouseholdDrinksContainersTonnage ?? 0m, 3),
                LateReportingTonnageTotal             = Math.Round(commsCostByMaterial.LateReportingTonnage, 3),
                TotalCommsCostTotal                   = CurrencyConverterUtils.ConvertToCurrency(commsCostByMaterial.TotalCost),
                TotalTonnageTotal                     = Math.Round(commsCostByMaterial.TotalTonnage, 3)
            };
        }
    }
}
