using EPR.Calculator.Service.Function.Converter;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcResultLaDisposalCostDataJson
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("calcResultLaDisposalCostDataDetails")]
        public required IEnumerable<CalcResultLaDisposalCostDetails> CalcResultLaDisposalCostDetails { get; set; }

        [JsonPropertyName("calcResultLaDisposalCostDataDetailsTotal")]
        public required CalcResultLaDisposalCostDataDetailsTotal CalcResultLaDisposalCostDataDetailsTotal { get; set; }
    }

    public class CalcResultLaDisposalCostDetails : BaseLaDisposalcostAnd2ACommsData
    {
        [JsonPropertyName("materialName")]
        public required string MaterialName { get; init; }

        [JsonPropertyName("englandLaDisposalCost")]
        public required string EnglandLaDisposalCost { get; init; }

        [JsonPropertyName("walesLaDisposalCost")]
        public required string WalesLaDisposalCost { get; init; }

        [JsonPropertyName("scotlandLaDisposalCost")]
        public required string ScotlandLaDisposalCost { get; init; }

        [JsonPropertyName("northernIrelandLaDisposalCost")]
        public required string NorthernIrelandLaDisposalCost { get; init; }

        [JsonPropertyName("totalLaDisposalCost")]
        public required string TotalLaDisposalCost { get; init; }

        [JsonPropertyName("disposalCostPricePerTonne")]
        public required string? DisposalCostPricePerTonne { get; init; }

    }

    public class CalcResultLaDisposalCostDataDetailsTotal
    {
        [JsonPropertyName("total")]
        public required string Total { get; init; }

        [JsonPropertyName("englandLaDisposalCostTotal")]
        public required string EnglandLaDisposalCostTotal { get; init; }

        [JsonPropertyName("walesLaDisposalCostTotal")]
        public required string WalesLaDisposalCostTotal { get; init; }

        [JsonPropertyName("scotlandLaDisposalCostTotal")]
        public required string ScotlandLaDisposalCostTotal { get; init; }

        [JsonPropertyName("northernIrelandLaDisposalCostTotal")]
        public required string NorthernIrelandLaDisposalCostTotal { get; init; }

        [JsonPropertyName("totalLaDisposalCostTotal")]
        public required string TotalLaDisposalCostTotal { get; init; }

        [JsonPropertyName("producerHouseholdPackagingWasteTonnageTotal")]
        [JsonConverter(typeof(DecimalPrecision3Converter))]
        public required decimal ProducerHouseholdPackagingWasteTonnageTotal { get; init; }

        [JsonPropertyName("publicBinTonnage")]
        [JsonConverter(typeof(DecimalPrecision3Converter))]
        public required decimal PublicBinTonnage { get; init; }

        [JsonPropertyName("householdDrinksContainersTonnageTotal")]
        [JsonConverter(typeof(DecimalPrecision3Converter))]
        public required decimal HouseholdDrinksContainersTonnageTotal { get; init; }

        [JsonPropertyName("lateReportingTonnageTotal")]
        [JsonConverter(typeof(DecimalPrecision3Converter))]
        public required decimal LateReportingTonnageTotal { get; init; }

        [JsonPropertyName("totalTonnageTotal")]
        [JsonConverter(typeof(DecimalPrecision3Converter))]
        public required decimal? TotalTonnageTotal { get; init; }

    }
}
