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
        public required decimal EnglandCommsCost { get; init; }

        [JsonProperty(PropertyName = "walesCommsCost")]
        public required decimal WalesCommsCost { get; init; }

        [JsonProperty(PropertyName = "scotlandCommsCost")]
        public required decimal ScotlandCommsCost { get; init; }

        [JsonProperty(PropertyName = "northernIrelandCommsCost")]
        public required decimal NorthernIrelandCommsCost { get; init; }

        [JsonProperty(PropertyName = "totalCommsCost")]
        public required decimal TotalCommsCost { get; init; }

        [JsonProperty(PropertyName = "producerHouseholdPackagingWasteTonnage")]
        public required decimal ProducerHouseholdPackagingWasteTonnage { get; init; }

        [JsonProperty(PropertyName = "publicBinTonnage")]
        public required decimal PublicBinTonnage { get; init; }

        [JsonProperty(PropertyName = "householdDrinksContainersTonnage")]
        public required decimal HouseholdDrinksContainersTonnage { get; init; }

        [JsonProperty(PropertyName = "lateReportingTonnage")]
        public required decimal LateReportingTonnage { get; init; }

        [JsonProperty(PropertyName = "totalTonnage")]
        public required decimal TotalTonnage { get; init; }

        [JsonProperty(PropertyName = "commsCostByMaterialPricePerTonne")]
        public required decimal CommsCostByMaterialPricePerTonne { get; init; }

    }
}
