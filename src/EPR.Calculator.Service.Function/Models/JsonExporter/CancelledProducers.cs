using Newtonsoft.Json;
using System.Collections.Generic;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public record CancelledProducers
    {
        [JsonProperty(PropertyName = "name")]
        public string? Name { get; init; }

        [JsonProperty(PropertyName = "cancelledProducerTonnageInvoice")]
        public IEnumerable<CancelledProducerTonnageInvoice>? CancelledProducerTonnageInvoice { get; init; }
    }

    public record CancelledProducerTonnageInvoice
    {
        [JsonProperty(PropertyName = "producerId")]
        public int? ProducerId { get; init; }

        [JsonProperty(PropertyName = "subsidiaryId")]
        public string? SubsidiaryId { get; init; }

        [JsonProperty(PropertyName = "producerName")]
        public string? ProducerName { get; init; }

        [JsonProperty(PropertyName = "tradingName")]
        public string? TradingName { get; init; }

        [JsonProperty(PropertyName = "lastProducerTonnages")]
        public required IEnumerable<LastProducerTonnages> LastProducerTonnages { get; init; }
    }

    public record LastProducerTonnages
    {
        [JsonProperty(PropertyName = "materialName")]
        public string? MaterialName { get; init; }

        [JsonProperty(PropertyName = "lastTonnage")]
        public decimal LastTonnage { get; init; }
    }
}
