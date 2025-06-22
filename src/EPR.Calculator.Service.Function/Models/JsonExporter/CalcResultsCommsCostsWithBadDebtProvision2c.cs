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
        
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        [JsonProperty(PropertyName = "totalProducerFeeForCommsCostsByCountryWithoutBadDebtProvision")]
        public decimal TotalProducerFeeForCommsCostsByCountryWithoutBadDebtProvision { get; set; }

        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        [JsonProperty(PropertyName = "badDebProvisionFor2c")]        
        public decimal BadDebProvisionFor2c { get; set; }
        
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        [JsonProperty(PropertyName = "totalProducerFeeForCommsCostsByCountryWithBadDebtProvision")]
        public decimal TotalProducerFeeForCommsCostsByCountryWithBadDebtProvision { get; set; }

        
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        [JsonProperty(PropertyName = "englandTotalWithBadDebtProvision")]
        public decimal EnglandTotalWithBadDebtProvision { get; set; }
        
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        [JsonProperty(PropertyName = "walesTotalWithBadDebtProvision")]
        public decimal WalesTotalWithBadDebtProvision { get; set; }
        
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        [JsonProperty(PropertyName = "scotlandTotalWithBadDebtProvision")]
        public decimal ScotlandTotalWithBadDebtProvision { get; set; }
        
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        [JsonProperty(PropertyName = "northernIrelandTotalWithBadDebtProvision")]
        public decimal NorthernIrelandTotalWithBadDebtProvision { get; set; }

    }
}
