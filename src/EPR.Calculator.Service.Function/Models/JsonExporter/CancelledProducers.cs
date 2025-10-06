using System.Collections.Generic;
using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Converter;

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
        public required int ProducerId { get; init; }

        [JsonPropertyName("subsidiaryId")]
        public string? SubsidiaryId { get; init; }

        [JsonPropertyName("producerName")]
        public required string ProducerName { get; init; }

        [JsonPropertyName("tradingName")]
        public required string TradingName { get; init; }

        [JsonPropertyName("lastProducerTonnages")]
        public required IEnumerable<LastProducerTonnages> LastProducerTonnages { get; init; }

        [JsonPropertyName("lastInvoicedTotal")]
        [JsonConverter(typeof(CurrencyConverter))]
        public required decimal LastInvoicedTotal { get; init; }

        [JsonPropertyName("runNumber")]
        public required int RunNumber { get; init; }

        [JsonPropertyName("runName")]
        public required string RunName { get; init; }

        [JsonPropertyName("billingInstructionID")]
        public required string BillingInstructionID { get; init; }
        
    }

    public record LastProducerTonnages
    {
        [JsonPropertyName("materialName")]
        public required string MaterialName { get; init; }

        [JsonPropertyName("lastTonnage")]
        public required decimal LastTonnage { get; init; }
    }
}
