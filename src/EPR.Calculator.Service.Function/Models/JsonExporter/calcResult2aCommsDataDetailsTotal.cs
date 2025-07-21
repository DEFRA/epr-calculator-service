using System.Text.Json.Serialization;

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

        public required decimal ProducerHouseholdPackagingWasteTonnageTotal { get; init; }

        [JsonPropertyName("publicBinTonnage")]

        public required decimal PublicBinTonnage { get; init; }

        [JsonPropertyName("householdDrinksContainersTonnageTotal")]

        public required decimal HouseholdDrinksContainersTonnageTotal { get; init; }

        [JsonPropertyName("lateReportingTonnageTotal")]

        public required decimal LateReportingTonnageTotal { get; init; }

        [JsonPropertyName("totalTonnageTotal")]

        public required decimal TotalTonnageTotal { get; init; }

    }
}
