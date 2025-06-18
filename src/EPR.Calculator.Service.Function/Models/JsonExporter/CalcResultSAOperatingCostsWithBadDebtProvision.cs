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
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal TotalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithoutBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "badDebProvisionFor3")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal BadDebProvisionFor3 { get; set; }

        [JsonProperty(PropertyName = "totalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal TotalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "englandTotalForSAOperatingCostsWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal EnglandTotalForSAOperatingCostsWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "walesTotalForSAOperatingCostsWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal WalesTotalForSAOperatingCostsWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "scotlandTotalForSAOperatingCostsWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal ScotlandTotalForSAOperatingCostsWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "northernIrelandTotalForSAOperatingCostsWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal NorthernIrelandTotalForSAOperatingCostsWithBadDebtProvision { get; set; }

    }

}
