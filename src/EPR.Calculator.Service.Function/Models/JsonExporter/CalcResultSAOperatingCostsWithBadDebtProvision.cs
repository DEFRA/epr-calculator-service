using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcResultSAOperatingCostsWithBadDebtProvision
    {

        [JsonProperty(PropertyName = "totalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithoutBadDebtProvision")]
        public decimal TotalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithoutBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "badDebProvisionFor3")]
        public decimal BadDebProvisionFor3 { get; set; }

        [JsonProperty(PropertyName = "totalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithBadDebtProvision")]
        public decimal TotalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "englandTotalForSAOperatingCostsWithBadDebtProvision")]
        public decimal EnglandTotalForSAOperatingCostsWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "walesTotalForSAOperatingCostsWithBadDebtProvision")]
        public decimal WalesTotalForSAOperatingCostsWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "scotlandTotalForSAOperatingCostsWithBadDebtProvision")]
        public decimal ScotlandTotalForSAOperatingCostsWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "northernIrelandTotalForSAOperatingCostsWithBadDebtProvision")]
        public decimal NorthernIrelandTotalForSAOperatingCostsWithBadDebtProvision { get; set; }

    }

}
