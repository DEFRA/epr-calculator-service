using EPR.Calculator.Service.Function.Converter;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public record TotalProducerBillWithBadDebtProvision
    {
        [JsonPropertyName("totalProducerBillWithoutBadDebtProvision")]
        [Newtonsoft.Json.JsonConverter(typeof(DecimalPrecisionConverter), 2)]
        public required decimal TotalProducerBillWithoutBadDebtProvision { get; set; }

        [JsonPropertyName("badDebtProvisionForTotalProducerBill")]
        [Newtonsoft.Json.JsonConverter(typeof(DecimalPrecisionConverter), 2)]
        public required decimal BadDebtProvisionForTotalProducerBill { get; set; }

        [JsonPropertyName("totalProducerBillWithBadDebtProvision")]
        [Newtonsoft.Json.JsonConverter(typeof(DecimalPrecisionConverter), 2)]
        public required decimal TotalProducerBillWithBadDebtProvisionAmount { get; set; }

        [JsonPropertyName("englandTotalForProducerBillWithBadDebtProvision")]
        [Newtonsoft.Json.JsonConverter(typeof(DecimalPrecisionConverter), 2)]
        public required decimal EnglandTotalForProducerBillWithBadDebtProvision { get; set; }

        [JsonPropertyName("walesTotalForProducerBillWithBadDebtProvision")]
        [Newtonsoft.Json.JsonConverter(typeof(DecimalPrecisionConverter), 2)]
        public required decimal WalesTotalForProducerBillWithBadDebtProvision { get; set; }

        [JsonPropertyName("scotlandTotalForProducerBillWithBadDebtProvision")]
        [Newtonsoft.Json.JsonConverter(typeof(DecimalPrecisionConverter), 2)]
        public required decimal ScotlandTotalForProducerBillWithBadDebtProvision { get; set; }

        [JsonPropertyName("northernIrelandTotalForProducerBillWithBadDebtProvision")]
        [Newtonsoft.Json.JsonConverter(typeof(DecimalPrecisionConverter), 2)]
        public required decimal NorthernIrelandTotalForProducerBillWithBadDebtProvision { get; set; }
    }
}
