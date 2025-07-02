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
        public string? TotalProducerFeeForCommsCostsByCountryWithoutBadDebtProvision { get; set; }

        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        [JsonProperty(PropertyName = "badDebProvisionFor2c")]        
        public string? BadDebProvisionFor2c { get; set; }
        
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        [JsonProperty(PropertyName = "totalProducerFeeForCommsCostsByCountryWithBadDebtProvision")]
        public string? TotalProducerFeeForCommsCostsByCountryWithBadDebtProvision { get; set; }

        
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        [JsonProperty(PropertyName = "englandTotalWithBadDebtProvision")]
        public string? EnglandTotalWithBadDebtProvision { get; set; }
        
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        [JsonProperty(PropertyName = "walesTotalWithBadDebtProvision")]
        public string? WalesTotalWithBadDebtProvision { get; set; }
        
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        [JsonProperty(PropertyName = "scotlandTotalWithBadDebtProvision")]
        public string? ScotlandTotalWithBadDebtProvision { get; set; }
        
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        [JsonProperty(PropertyName = "northernIrelandTotalWithBadDebtProvision")]
        public string? NorthernIrelandTotalWithBadDebtProvision { get; set; }

    }
}
