using EPR.Calculator.Service.Function.Converter;
using System.Text.Json.Serialization;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public record TotalProducerBillWithBadDebtProvision
    {
        [JsonPropertyName("totalProducerBillWithoutBadDebtProvision")]
        public required string TotalProducerBillWithoutBadDebtProvision { get; set; }

        [JsonPropertyName("badDebtProvisionForTotalProducerBill")]
        public required string BadDebtProvisionForTotalProducerBill { get; set; }

        [JsonPropertyName("totalProducerBillWithBadDebtProvision")]
        public required string TotalProducerBillWithBadDebtProvisionAmount { get; set; }

        [JsonPropertyName("englandTotalForProducerBillWithBadDebtProvision")]
        public required string EnglandTotalForProducerBillWithBadDebtProvision { get; set; }

        [JsonPropertyName("walesTotalForProducerBillWithBadDebtProvision")]
        public required string WalesTotalForProducerBillWithBadDebtProvision { get; set; }

        [JsonPropertyName("scotlandTotalForProducerBillWithBadDebtProvision")]
        public required string ScotlandTotalForProducerBillWithBadDebtProvision { get; set; }

        [JsonPropertyName("northernIrelandTotalForProducerBillWithBadDebtProvision")]
        public required string NorthernIrelandTotalForProducerBillWithBadDebtProvision { get; set; }
    }
}
