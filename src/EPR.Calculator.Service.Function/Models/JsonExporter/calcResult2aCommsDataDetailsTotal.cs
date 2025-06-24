using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public record calcResult2aCommsDataDetailsTotal
    {
        [JsonProperty(PropertyName = "total")]

        public required string Total { get; init; }

        [JsonProperty(PropertyName = "englandCommsCostTotal")]

        public required string EnglandCommsCostTotal { get; init; }

        [JsonProperty(PropertyName = "walesCommsCostTotal")]

        public required string WalesCommsCostTotal { get; init; }

        [JsonProperty(PropertyName = "scotlandCommsCostTotal")]

        public required string ScotlandCommsCostTotal { get; init; }

        [JsonProperty(PropertyName = "northernIrelandCommsCostTotal")]

        public required string NorthernIrelandCommsCostTotal { get; init; }

        [JsonProperty(PropertyName = "totalCommsCostTotal")]

        public required string TotalCommsCostTotal { get; init; }

        [JsonProperty(PropertyName = "producerHouseholdPackagingWasteTonnageTotal")]

        public required decimal ProducerHouseholdPackagingWasteTonnageTotal { get; init; }

        [JsonProperty(PropertyName = "publicBinTonnage")]

        public required decimal PublicBinTonnage { get; init; }

        [JsonProperty(PropertyName = "householdDrinksContainersTonnageTotal")]

        public required decimal HouseholdDrinksContainersTonnageTotal { get; init; }

        [JsonProperty(PropertyName = "lateReportingTonnageTotal")]

        public required decimal LateReportingTonnageTotal { get; init; }

        [JsonProperty(PropertyName = "totalTonnageTotal")]

        public required decimal TotalTonnageTotal { get; init; }

    }
}
