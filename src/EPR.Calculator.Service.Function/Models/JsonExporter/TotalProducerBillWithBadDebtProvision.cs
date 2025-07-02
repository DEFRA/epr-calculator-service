using EPR.Calculator.Service.Function.Converter;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public record TotalProducerBillWithBadDebtProvision
    {
        [JsonPropertyName("totalProducerBillWithoutBadDebtProvision")]
        [Newtonsoft.Json.JsonConverter(typeof(DecimalPrecisionConverter), 2)]
        public required string TotalProducerBillWithoutBadDebtProvision { get; set; }

        [JsonPropertyName("badDebtProvisionForTotalProducerBill")]
        [Newtonsoft.Json.JsonConverter(typeof(DecimalPrecisionConverter), 2)]
        public required string BadDebtProvisionForTotalProducerBill { get; set; }

        [JsonPropertyName("totalProducerBillWithBadDebtProvision")]
        [Newtonsoft.Json.JsonConverter(typeof(DecimalPrecisionConverter), 2)]
        public required string TotalProducerBillWithBadDebtProvisionAmount { get; set; }

        [JsonPropertyName("englandTotalForProducerBillWithBadDebtProvision")]
        [Newtonsoft.Json.JsonConverter(typeof(DecimalPrecisionConverter), 2)]
        public required string EnglandTotalForProducerBillWithBadDebtProvision { get; set; }

        [JsonPropertyName("walesTotalForProducerBillWithBadDebtProvision")]
        [Newtonsoft.Json.JsonConverter(typeof(DecimalPrecisionConverter), 2)]
        public required string WalesTotalForProducerBillWithBadDebtProvision { get; set; }

        [JsonPropertyName("scotlandTotalForProducerBillWithBadDebtProvision")]
        [Newtonsoft.Json.JsonConverter(typeof(DecimalPrecisionConverter), 2)]
        public required string ScotlandTotalForProducerBillWithBadDebtProvision { get; set; }

        [JsonPropertyName("northernIrelandTotalForProducerBillWithBadDebtProvision")]
        [Newtonsoft.Json.JsonConverter(typeof(DecimalPrecisionConverter), 2)]
        public required string NorthernIrelandTotalForProducerBillWithBadDebtProvision { get; set; }
    }
}
