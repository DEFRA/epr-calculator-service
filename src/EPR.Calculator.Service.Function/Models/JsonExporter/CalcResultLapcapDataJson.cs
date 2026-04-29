using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Builder.Lapcap;

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
            IEnumerable<string> SeperatedRecords = [CalcResultLapcapDataBuilder.Total, CalcResultLapcapDataBuilder.CountryApportionment, "Material"];

            return new CalcResultLapcapDataJson
            {
                Name = data.Name,
                CalcResultLapcapDataDetails =
                    data.CalcResultLapcapDataDetails
                    .Where(record => !SeperatedRecords.Contains(record.Name))
                    .Select(details => CalcResultLapcapDataDetailJson.From(details)),
                CalcResultLapcapDataTotal = CalcResultLapcapDataDetailTotalJson.From(
                    data.CalcResultLapcapDataDetails.Single(record => record.Name == CalcResultLapcapDataBuilder.Total)
                ),
                OneCountryApportionmentPercentages = CalcResultLapcapDataDetailApportionmentJson.From(
                    data.CalcResultLapcapDataDetails.Single(record => record.Name == CalcResultLapcapDataBuilder.CountryApportionment)
                )
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
                MaterialName = record.Name,
                EnglandLaDisposalCost = record.EnglandDisposalCost,
                WalesLaDisposalCost = record.WalesDisposalCost,
                ScotlandLaDisposalCost = record.ScotlandDisposalCost,
                NorthernIrelandLaDisposalCost = record.NorthernIrelandDisposalCost,
                OneLaDisposalCostTotal = record.TotalDisposalCost
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
                TotalEnglandLaDisposalCost = record.EnglandDisposalCost,
                TotalWalesLaDisposalCost = record.WalesDisposalCost,
                TotalScotlandLaDisposalCost = record.ScotlandDisposalCost,
                TotalNorthernIrelandLaDisposalCost = record.NorthernIrelandDisposalCost,
                TotalLaDisposalCost = record.TotalDisposalCost
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

        public static CalcResultLapcapDataDetailApportionmentJson From(CalcResultLapcapDataDetail record)
        {
            return new CalcResultLapcapDataDetailApportionmentJson
            {
                EnglandApportionment = record.EnglandDisposalCost,
                WalesApportionment = record.WalesDisposalCost,
                ScotlandApportionment = record.ScotlandDisposalCost,
                NorthernIrelandApportionment = record.NorthernIrelandDisposalCost,
                TotalApportionment = record.TotalDisposalCost
            };
        }
    }
}