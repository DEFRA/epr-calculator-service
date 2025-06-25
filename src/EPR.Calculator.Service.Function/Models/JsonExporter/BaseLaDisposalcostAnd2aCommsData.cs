using EPR.Calculator.Service.Function.Converter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.Azure.Amqp.Serialization.SerializableType;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class BaseLaDisposalcostAnd2aCommsData
    {
        [JsonProperty(PropertyName = "producerHouseholdPackagingWasteTonnage")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public required decimal ProducerHouseholdPackagingWasteTonnage { get; init; }

        [JsonProperty(PropertyName = "publicBinTonnage")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public required decimal PublicBinTonnage { get; init; }

        [JsonProperty(PropertyName = "householdDrinksContainersTonnage")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public required decimal HouseholdDrinksContainersTonnage { get; init; }

        [JsonProperty(PropertyName = "lateReportingTonnage")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public required decimal LateReportingTonnage { get; init; }

        [JsonProperty(PropertyName = "totalTonnage")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public required decimal TotalTonnage { get; init; }
    }
}
