using Azure.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcResultSummaryCommsCostsByMaterialFeesSummary2a
    {
        [JsonProperty(PropertyName = "totalProducerFeeForCommsCostsWithoutBadDebtProvision2a")]
        public required string  TotalProducerFeeForCommsCostsWithoutBadDebtProvision2a { get; set; }

        [JsonProperty(PropertyName = "totalBadDebtProvision")]
        public required string TotalBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "totalProducerFeeForCommsCostsWithBadDebtProvision2a")]
        public required string TotalProducerFeeForCommsCostsWithBadDebtProvision2a { get; set; }

        [JsonProperty(PropertyName = "englandTotalWithBadDebtProvision")]
        public required string EnglandTotalWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "walesTotalWithBadDebtProvision")]
        public required string WalesTotalWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "scotlandTotalWithBadDebtProvision")]
        public required string ScotlandTotalWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "northernIrelandTotalWithBadDebtProvision")]
        public required string NorthernIrelandTotalWithBadDebtProvision { get; set; }
    }
}
