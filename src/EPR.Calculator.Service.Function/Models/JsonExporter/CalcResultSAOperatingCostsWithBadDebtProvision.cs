using Azure.Identity;
using EPR.Calculator.Service.Function.Converter;
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
        public required string  TotalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithoutBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "badDebProvisionFor3")]
        public required string BadDebProvisionFor3 { get; set; }

        [JsonProperty(PropertyName = "totalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithBadDebtProvision")]
        public required string TotalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "englandTotalForSAOperatingCostsWithBadDebtProvision")]
        public required string EnglandTotalForSAOperatingCostsWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "walesTotalForSAOperatingCostsWithBadDebtProvision")]
        public required string WalesTotalForSAOperatingCostsWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "scotlandTotalForSAOperatingCostsWithBadDebtProvision")]
        public required string ScotlandTotalForSAOperatingCostsWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "northernIrelandTotalForSAOperatingCostsWithBadDebtProvision")]
        public required string NorthernIrelandTotalForSAOperatingCostsWithBadDebtProvision { get; set; }

    }

}
