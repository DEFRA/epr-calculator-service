using EPR.Calculator.Service.Function.Converter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcResultLaDisposalCostDataJson
    {
        [JsonProperty(PropertyName = "calcResultLaDisposalCostDataDetails")]
        public required IEnumerable<CalcResultLaDisposalCostDetails> CalcResultLaDisposalCostDetails { get; set; }

        [JsonProperty(PropertyName = "calcResultLaDisposalCostDataDetailsTotal")]
        public required CalcResultLaDisposalCostDataDetailsTotal CalcResultLaDisposalCostDataDetailsTotal { get; set; }
    }

    public class CalcResultLaDisposalCostDetails
    {
        [JsonProperty(PropertyName = "materialName")]
        public required string MaterialName { get; init; }

        [JsonProperty(PropertyName = "englandLaDisposalCost")]
        public required string EnglandLaDisposalCost { get; init; }

        [JsonProperty(PropertyName = "walesLaDisposalCost")]
        public required string WalesLaDisposalCost { get; init; }

        [JsonProperty(PropertyName = "scotlandLaDisposalCost")]
        public required string ScotlandLaDisposalCost { get; init; }

        [JsonProperty(PropertyName = "northernIrelandLaDisposalCost")]
        public required string NorthernIrelandLaDisposalCost { get; init; }

        [JsonProperty(PropertyName = "totalLaDisposalCost")]
        public required string TotalLaDisposalCost { get; init; }

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

        [JsonProperty(PropertyName = "disposalCostPricePerTonne")]
        public required string? DisposalCostPricePerTonne { get; init; }

    }

    public class CalcResultLaDisposalCostDataDetailsTotal
    {
        [JsonProperty(PropertyName = "total")]
        public required string Total { get; init; }

        [JsonProperty(PropertyName = "englandLaDisposalCostTotal")]
        public required string EnglandLaDisposalCostTotal { get; init; }

        [JsonProperty(PropertyName = "walesLaDisposalCostTotal")]
        public required string WalesLaDisposalCostTotal { get; init; }

        [JsonProperty(PropertyName = "scotlandLaDisposalCostTotal")]
        public required string ScotlandLaDisposalCostTotal { get; init; }

        [JsonProperty(PropertyName = "northernIrelandLaDisposalCostTotal")]
        public required string NorthernIrelandLaDisposalCostTotal { get; init; }

        [JsonProperty(PropertyName = "totalLaDisposalCostTotal")]
        public required string TotalLaDisposalCostTotal { get; init; }

        [JsonProperty(PropertyName = "producerHouseholdPackagingWasteTonnageTotal")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public required decimal ProducerHouseholdPackagingWasteTonnageTotal { get; init; }

        [JsonProperty(PropertyName = "publicBinTonnage")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public required decimal PublicBinTonnage { get; init; }

        [JsonProperty(PropertyName = "householdDrinksContainersTonnageTotal")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public required decimal HouseholdDrinksContainersTonnageTotal { get; init; }

        [JsonProperty(PropertyName = "lateReportingTonnageTotal")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public required decimal LateReportingTonnageTotal { get; init; }

        [JsonProperty(PropertyName = "totalTonnageTotal")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public required decimal? TotalTonnageTotal { get; init; }

    }
}
