using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public record CancelledProducers
    {
        [JsonPropertyName("name")]
        public string? Name { get; init; }

        [JsonPropertyName("cancelledProducerTonnageInvoice")]
        public IEnumerable<CancelledProducerTonnageInvoice>? CancelledProducerTonnageInvoice { get; init; }
    }

    public record CancelledProducerTonnageInvoice
    {
        [JsonPropertyName("producerId")]
        public int? ProducerId { get; init; }

        [JsonPropertyName("subsidiaryId")]
        public string? SubsidiaryId { get; init; }

        [JsonPropertyName("producerName")]
        public string? ProducerName { get; init; }

        [JsonPropertyName("tradingName")]
        public string? TradingName { get; init; }

        [JsonPropertyName("lastProducerTonnages")]
        public required IEnumerable<LastProducerTonnages> LastProducerTonnages { get; init; }
    }

    public record LastProducerTonnages
    {
        [JsonPropertyName("materialName")]
        public string? MaterialName { get; init; }

        [JsonPropertyName("lastTonnage")]
        public decimal LastTonnage { get; init; }
    }
}
