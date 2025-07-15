using EPR.Calculator.Service.Function.Converter;
using System.Text.Json.Serialization;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public record TotalProducerBillWithBadDebtProvision
    {
        [JsonPropertyName("totalProducerBillWithoutBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecision2Converter))]
        public required string TotalProducerBillWithoutBadDebtProvision { get; set; }

        [JsonPropertyName("badDebtProvisionForTotalProducerBill")]
        [JsonConverter(typeof(DecimalPrecision2Converter))]
        public required string BadDebtProvisionForTotalProducerBill { get; set; }

        [JsonPropertyName("totalProducerBillWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecision2Converter))]
        public required string TotalProducerBillWithBadDebtProvisionAmount { get; set; }

        [JsonPropertyName("englandTotalForProducerBillWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecision2Converter))]
        public required string EnglandTotalForProducerBillWithBadDebtProvision { get; set; }

        [JsonPropertyName("walesTotalForProducerBillWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecision2Converter))]
        public required string WalesTotalForProducerBillWithBadDebtProvision { get; set; }

        [JsonPropertyName("scotlandTotalForProducerBillWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecision2Converter))]
        public required string ScotlandTotalForProducerBillWithBadDebtProvision { get; set; }

        [JsonPropertyName("northernIrelandTotalForProducerBillWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecision2Converter))]
        public required string NorthernIrelandTotalForProducerBillWithBadDebtProvision { get; set; }
    }
}
