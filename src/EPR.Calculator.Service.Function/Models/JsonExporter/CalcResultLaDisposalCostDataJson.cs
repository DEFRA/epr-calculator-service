using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Converter;
using EPR.Calculator.Service.Function.Utils;
using System.Globalization;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcResultLaDisposalCostDataJson
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("calcResultLaDisposalCostDetails")]
        public required IEnumerable<CalcResultLaDisposalCostDetailsJson> CalcResultLaDisposalCostDetails { get; set; }

        [JsonPropertyName("calcResultLaDisposalCostDataDetailsTotal")]
        public required CalcResultLaDisposalCostDataDetailsTotal CalcResultLaDisposalCostDataDetailsTotal { get; set; }

        public static CalcResultLaDisposalCostDataJson From(
            Dictionary<string, CalcResultLaDisposalCostDataDetail> detailsByMaterial,
            CalcResultLaDisposalCostDataDetail total,
            IImmutableList<MaterialDetail> materials
        )
        {
            return new CalcResultLaDisposalCostDataJson
            {
                Name = CommonConstants.LADisposalCostData,
                CalcResultLaDisposalCostDetails =
                    detailsByMaterial.Select(item =>
                    {
                        Console.WriteLine($"Looking for {item.Key}");
                        var material = materials.First(m => m.Code == item.Key);
                        return CalcResultLaDisposalCostDetailsJson.From(material, item.Value);
                    }).ToList(),
                CalcResultLaDisposalCostDataDetailsTotal = CalcResultLaDisposalCostDataDetailsTotal.From(total)
            };
        }
    }

    public class CalcResultLaDisposalCostDetailsJson : BaseLaDisposalcostAnd2ACommsData
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

        public static CalcResultLaDisposalCostDetailsJson From(MaterialDetail material, CalcResultLaDisposalCostDataDetail item)
        {
            return new CalcResultLaDisposalCostDetailsJson
            {
                MaterialName                           = material.Name,
                EnglandLaDisposalCost                  = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(item.England        , 2, ","),
                WalesLaDisposalCost                    = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(item.Wales          , 2, ","),
                ScotlandLaDisposalCost                 = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(item.Scotland       , 2, ","),
                NorthernIrelandLaDisposalCost          = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(item.NorthernIreland, 2, ","),
                TotalLaDisposalCost                    = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(item.Total          , 2, ","),
                ProducerHouseholdPackagingWasteTonnage = item.ProducerReportedHouseholdPackagingWasteTonnage,
                PublicBinTonnage                       = item.ReportedPublicBinTonnage,
                HouseholdDrinksContainersTonnage       = item.HouseholdDrinkContainers ?? 0m,
                LateReportingTonnage                   = item.LateReportingTonnage ?? 0m,
                TotalTonnage                           = item.ProducerReportedTotalTonnage ?? 0m,
                DisposalCostPricePerTonne              = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(item.DisposalCostPricePerTonne == null ? 0 : item.DisposalCostPricePerTonne.Value, 4, ",")
            };
        }
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

        public static CalcResultLaDisposalCostDataDetailsTotal From(CalcResultLaDisposalCostDataDetail total)
        {
            if (total == null)
            {
                return new CalcResultLaDisposalCostDataDetailsTotal
                {
                    EnglandLaDisposalCostTotal                  = string.Empty,
                    HouseholdDrinksContainersTonnageTotal       = 0,
                    LateReportingTonnageTotal                   = 0,
                    NorthernIrelandLaDisposalCostTotal          = string.Empty,
                    ProducerHouseholdPackagingWasteTonnageTotal = 0,
                    PublicBinTonnage                            = 0,
                    ScotlandLaDisposalCostTotal                 = string.Empty,
                    Total                                       = string.Empty,
                    TotalLaDisposalCostTotal                    = string.Empty,
                    TotalTonnageTotal                           = 0,
                    WalesLaDisposalCostTotal                    = string.Empty
                };
            }
            return new CalcResultLaDisposalCostDataDetailsTotal
            {
                Total                                       = "Total",
                EnglandLaDisposalCostTotal                  = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(total.England        , 2, ","),
                WalesLaDisposalCostTotal                    = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(total.Wales          , 2, ","),
                ScotlandLaDisposalCostTotal                 = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(total.Scotland       , 2, ","),
                NorthernIrelandLaDisposalCostTotal          = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(total.NorthernIreland, 2, ","),
                TotalLaDisposalCostTotal                    = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(total.Total          , 2, ","),
                ProducerHouseholdPackagingWasteTonnageTotal = total.ProducerReportedHouseholdPackagingWasteTonnage,
                PublicBinTonnage                            = total.ReportedPublicBinTonnage,
                HouseholdDrinksContainersTonnageTotal       = total.HouseholdDrinkContainers ?? 0m,
                LateReportingTonnageTotal                   = total.LateReportingTonnage ?? 0m,
                TotalTonnageTotal                           = total.ProducerReportedTotalTonnage != null ? total.ProducerReportedTotalTonnage : 0m
            };

        }
    }
}
