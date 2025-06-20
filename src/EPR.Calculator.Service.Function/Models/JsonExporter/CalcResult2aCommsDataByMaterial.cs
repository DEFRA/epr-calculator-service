using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcResult2aCommsDataByMaterial
    {
        [JsonProperty(PropertyName = "calcResult2aCommsDataDetails")]
        public IEnumerable<CalcResult2aCommsDataDetails> CalcResult2aCommsDataDetails {  get; set; }
    }

    public class CalcResult2aCommsDataDetails
    {
        [JsonProperty(PropertyName = "materialName")]
        public required string MaterialName { get; init; }


        [JsonProperty(PropertyName = "englandCommsCost")]
        public required string EnglandCommsCost { get; init; }

        [JsonProperty(PropertyName = "walesCommsCost")]
        public required string WalesCommsCost { get; init; }

        [JsonProperty(PropertyName = "scotlandCommsCost")]
        public required string ScotlandCommsCost { get; init; }

        [JsonProperty(PropertyName = "northernIrelandCommsCost")]
        public required string NorthernIrelandCommsCost { get; init; }

        [JsonProperty(PropertyName = "totalCommsCost")]
        public required string TotalCommsCost { get; init; }

        [JsonProperty(PropertyName = "producerHouseholdPackagingWasteTonnage")]
        public required string ProducerHouseholdPackagingWasteTonnage { get; init; }

        [JsonProperty(PropertyName = "publicBinTonnage")]
        public required string PublicBinTonnage { get; init; }

        [JsonProperty(PropertyName = "householdDrinksContainersTonnage")]
        public required string HouseholdDrinksContainersTonnage { get; init; }

        [JsonProperty(PropertyName = "lateReportingTonnage")]
        public required string LateReportingTonnage { get; init; }

        [JsonProperty(PropertyName = "totalTonnage")]
        public required string TotalTonnage { get; init; }

        [JsonProperty(PropertyName = "commsCostByMaterialPricePerTonne")]
        public required string CommsCostByMaterialPricePerTonne { get; init; }


    }
}
