using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Builder.Lapcap;
using System.Globalization;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Converter;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcResultLapcapDataJson
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("calcResultLapcapDataDetails")]
        public required IEnumerable<CalcResultLapcapDataDetailJson> CalcResultLapcapDataDetails { get; set; }

        [JsonPropertyName("calcResultLapcapDataTotal")]
        public CalcResultLapcapDataDetailTotalJson? CalcResultLapcapDataTotal { get; set; }

        [JsonPropertyName("oneCountryApportionmentPercentages")]
        public CalcResultLapcapDataDetailApportionmentJson? OneCountryApportionmentPercentages { get; set; }

        public static CalcResultLapcapDataJson From(CalcResultLapcapData data)
        {
            IEnumerable<string> SeperatedRecords = [CalcResultLapcapDataBuilder.Total, "Material"];

            return new CalcResultLapcapDataJson
            {
                Name = "LAPCAP Data",
                //TODO total should be separate in record
                CalcResultLapcapDataDetails =
                    data.CalcResultLapcapDataDetails
                    .Where(record => !SeperatedRecords.Contains(record.Name))
                    .Select(details => CalcResultLapcapDataDetailJson.From(details)),
                CalcResultLapcapDataTotal = CalcResultLapcapDataDetailTotalJson.From(
                    data.CalcResultLapcapDataDetails.Single(record => record.Name == CalcResultLapcapDataBuilder.Total)
                ),
                OneCountryApportionmentPercentages = CalcResultLapcapDataDetailApportionmentJson.From(data.CountryApportionment)
            };
        }
    }

    public class CalcResultLapcapDataDetailJson
    {
        [JsonPropertyName("materialName")]
        public required string MaterialName { get; set; }

        [JsonPropertyName("englandLaDisposalCost")]
        public required string EnglandLaDisposalCost { get; set; }

        [JsonPropertyName("walesLaDisposalCost")]
        public required string WalesLaDisposalCost { get; set; }

        [JsonPropertyName("scotlandLaDisposalCost")]
        public required string ScotlandLaDisposalCost { get; set; }

        [JsonPropertyName("northernIrelandLaDisposalCost")]
        public required string NorthernIrelandLaDisposalCost { get; set; }

        [JsonPropertyName("oneLaDisposalCostTotal")]
        public required string OneLaDisposalCostTotal { get; set; }

        public static CalcResultLapcapDataDetailJson From(CalcResultLapcapDataDetail record)
        {
            return new CalcResultLapcapDataDetailJson
            {
                MaterialName                  = record.Name,
                EnglandLaDisposalCost         = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(record.EnglandCost        , 2, ","),
                WalesLaDisposalCost           = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(record.WalesCost          , 2, ","),
                ScotlandLaDisposalCost        = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(record.ScotlandCost       , 2, ","),
                NorthernIrelandLaDisposalCost = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(record.NorthernIrelandCost, 2, ","),
                OneLaDisposalCostTotal        = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(record.TotalCost          , 2, ",")
            };
        }
    }

    public class CalcResultLapcapDataDetailTotalJson
    {
        [JsonPropertyName("totalEnglandLaDisposalCost")]
        public required string TotalEnglandLaDisposalCost { get; set; }

        [JsonPropertyName("totalWalesLaDisposalCost")]
        public required string TotalWalesLaDisposalCost { get; set; }

        [JsonPropertyName("totalScotlandLaDisposalCost")]
        public required string TotalScotlandLaDisposalCost { get; set; }

        [JsonPropertyName("totalNorthernIrelandLaDisposalCost")]
        public required string TotalNorthernIrelandLaDisposalCost { get; set; }

        [JsonPropertyName("totalLaDisposalCost")]
        public required string TotalLaDisposalCost { get; set; }

        public static CalcResultLapcapDataDetailTotalJson From(CalcResultLapcapDataDetail record)
        {
            return new CalcResultLapcapDataDetailTotalJson
            {
                TotalEnglandLaDisposalCost         = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(record.EnglandCost        , 2, ","),
                TotalWalesLaDisposalCost           = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(record.WalesCost          , 2, ","),
                TotalScotlandLaDisposalCost        = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(record.ScotlandCost       , 2, ","),
                TotalNorthernIrelandLaDisposalCost = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(record.NorthernIrelandCost, 2, ","),
                TotalLaDisposalCost                = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(record.TotalCost          , 2, ",")
            };
        }
    }

    public class CalcResultLapcapDataDetailApportionmentJson
    {
        [JsonPropertyName("englandApportionment")]
        public required string EnglandApportionment { get; set; }

        [JsonPropertyName("walesApportionment")]
        public required string WalesApportionment { get; set; }

        [JsonPropertyName("scotlandApportionment")]
        public required string ScotlandApportionment { get; set; }

        [JsonPropertyName("northernIrelandApportionment")]
        public required string NorthernIrelandApportionment { get; set; }

        [JsonPropertyName("totalApportionment")]
        public required string TotalApportionment { get; set; }

        public static CalcResultLapcapDataDetailApportionmentJson From(CountryApportionmentData record)
        {
            return new CalcResultLapcapDataDetailApportionmentJson
            {
                EnglandApportionment         = $"{(100 * record.England        ).ToString("N", new NumberFormatInfo { NumberDecimalDigits = 8 })}%",
                WalesApportionment           = $"{(100 * record.Wales          ).ToString("N", new NumberFormatInfo { NumberDecimalDigits = 8 })}%",
                ScotlandApportionment        = $"{(100 * record.Scotland       ).ToString("N", new NumberFormatInfo { NumberDecimalDigits = 8 })}%",
                NorthernIrelandApportionment = $"{(100 * record.NorthernIreland).ToString("N", new NumberFormatInfo { NumberDecimalDigits = 8 })}%",
                TotalApportionment           = $"{(100                         ).ToString("N", new NumberFormatInfo { NumberDecimalDigits = 8 })}%"
            };
        }
    }
}
