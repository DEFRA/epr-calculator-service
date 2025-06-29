using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    [SuppressMessage("Naming", "S101:Types should be named in PascalCase", Justification = "Required for JSON contract")]
    public class FeeForLADataPrepCostsWithBadDebtProvision_4
    {
        [JsonProperty(PropertyName = "totalProducerFeeForLADataPrepCostsWithoutBadDebtProvision")]
        public required string TotalProducerFeeForLADataPrepCostsWithoutBadDebtProvision {get; set;}

        [JsonProperty(PropertyName = "badDebtProvisionFor4")]
        public required string BadDebtProvisionFor4 {get; set;}

        [JsonProperty(PropertyName = "totalProducerFeeForLADataPrepCostsWithBadDebtProvision")]
        public required string TotalProducerFeeForLADataPrepCostsWithBadDebtProvision {get; set;}

        [JsonProperty(PropertyName = "englandTotalForLADataPrepCostsWithBadDebtProvision")]
        public required string EnglandTotalForLADataPrepCostsWithBadDebtProvision {get; set;}

        [JsonProperty(PropertyName = "walesTotalForLADataPrepCostsWithBadDebtProvision")]
        public required string WalesTotalForLADataPrepCostsWithBadDebtProvision {get; set;}

        [JsonProperty(PropertyName = "scotlandTotalForLADataPrepCostsWithBadDebtProvision")]
        public required string ScotlandTotalForLADataPrepCostsWithBadDebtProvision {get; set;}

        [JsonProperty(PropertyName = "northernIrelandTotalForLADataPrepCostsWithBadDebtProvision")]
        public required string NorthernIrelandTotalForLADataPrepCostsWithBadDebtProvision { get; set;}
    }
}