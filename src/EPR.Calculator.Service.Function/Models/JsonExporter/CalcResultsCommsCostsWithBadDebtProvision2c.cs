using EPR.Calculator.Service.Function.Converter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcResultsCommsCostsWithBadDebtProvision2c
    {
        [JsonProperty(PropertyName = "totalProducerFeeForCommsCostsByCountryWithoutBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal TotalProducerFeeForCommsCostsByCountryWithoutBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "badDebProvisionFor2c")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal BadDebProvisionFor2c { get; set; }

        [JsonProperty(PropertyName = "totalProducerFeeForCommsCostsByCountryWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal TotalProducerFeeForCommsCostsByCountryWithBadDebtProvision { get; set; }


        [JsonProperty(PropertyName = "englandTotalWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal EnglandTotalWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "walesTotalWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal WalesTotalWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "scotlandTotalWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal ScotlandTotalWithBadDebtProvision { get; set; }

        [JsonProperty(PropertyName = "northernIrelandTotalWithBadDebtProvision")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal NorthernIrelandTotalWithBadDebtProvision { get; set; }

    }
}
