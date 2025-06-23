using EPR.Calculator.Service.Function.Converter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcResult2ACommsDataByMaterial
    {
        [JsonProperty(PropertyName = "calcResult2aCommsDataDetails")]
        public IEnumerable<CalcResult2ACommsDataDetails> CalcResult2ACommsDataDetails {  get; set; }
    }

    public class CalcResult2ACommsDataDetails
    {
        [JsonProperty(PropertyName = "materialName")]

        public required string MaterialName { get; init; }


        [JsonProperty(PropertyName = "englandCommsCost")]

        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public required decimal EnglandCommsCost { get; init; }

        [JsonProperty(PropertyName = "walesCommsCost")]

        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public required decimal WalesCommsCost { get; init; }

        [JsonProperty(PropertyName = "scotlandCommsCost")]

        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public required decimal ScotlandCommsCost { get; init; }

        [JsonProperty(PropertyName = "northernIrelandCommsCost")]

        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public required decimal NorthernIrelandCommsCost { get; init; }

        [JsonProperty(PropertyName = "totalCommsCost")]

        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public required decimal TotalCommsCost { get; init; }

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

        [JsonProperty(PropertyName = "commsCostByMaterialPricePerTonne")]

        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public required decimal CommsCostByMaterialPricePerTonne { get; init; }

    }
}
