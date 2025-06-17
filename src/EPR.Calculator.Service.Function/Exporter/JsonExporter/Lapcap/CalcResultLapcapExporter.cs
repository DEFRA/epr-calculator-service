using EPR.Calculator.Service.Function.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.Lapcap
{
    /// <summary>
    /// Converts lapcap data to JSON string format.
    /// </summary>
    public class CalcResultLapcapExporter : ICalcResultLapcapExporter
    {
        /// <inheritdoc/>
        public string ConvertToJson(CalcResultLapcapData data)
            => JsonSerializer.Serialize(
                new CalcResultLapcapDataToSerialise(data),
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                });

        /// <summary>
        /// Holds the lapcap data in a structure that will be serialised to the expected JSON layout.
        /// </summary>
        private readonly record struct CalcResultLapcapDataToSerialise(
            string Name,
            IEnumerable<CalcResultLapcapDataDetailsToSerialise> CalcResultLapcapDataDetails)
        {
            public CalcResultLapcapDataToSerialise(CalcResultLapcapData data)
                : this(
                      data.Name,
                      data.CalcResultLapcapDataDetails
                        .Select(details => new CalcResultLapcapDataDetailsToSerialise(details)))
            {
            }
        }

        /// <summary>
        /// Holds the lapcap data in a structure that will be serialised to the expected JSON layout.
        /// </summary>
        private readonly record struct CalcResultLapcapDataDetailsToSerialise(
            string MaterialName,
            string EnglandDisposalCost,
            string WalesDisposalCost,
            string ScotlandDisposalCost,
            string NorthernIrelandDisposalCost,
            string OneLaDisposalCostTotal)
        {
            public CalcResultLapcapDataDetailsToSerialise(CalcResultLapcapDataDetails data)
                : this(data.Name,
                      data.EnglandDisposalCost,
                      data.WalesDisposalCost,
                      data.ScotlandDisposalCost,
                      data.NorthernIrelandDisposalCost,
                      data.TotalDisposalCost)
            {
            }
        }
    }
}