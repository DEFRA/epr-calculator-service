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
        public decimal TotalProducerFeeForCommsCostsWithoutBadDebtProvision2a { get; set; }

        [JsonProperty(PropertyName = "totalBadDebtProvision")]
        public decimal TotalBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "totalProducerFeeForCommsCostsWithBadDebtProvision2a")]
        public decimal TotalProducerFeeForCommsCostsWithBadDebtProvision2a { get; set; }

        [JsonProperty(PropertyName = "englandTotalWithBadDebtProvision")]
        public decimal EnglandTotalWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "walesTotalWithBadDebtProvision")]
        public decimal WalesTotalWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "scotlandTotalWithBadDebtProvision")]
        public decimal ScotlandTotalWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "northernIrelandTotalWithBadDebtProvision")]
        public decimal NorthernIrelandTotalWithBadDebtProvision { get; set; }
    }
}
