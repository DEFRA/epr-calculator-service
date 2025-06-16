using EPR.Calculator.Service.Function.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter
{
    /// <summary>
    /// Converts a <see cref="CalcResultSummaryProducerDisposalFees"/> to a JSON string representation.
    /// </summary>
    public class CalcResultSummaryProducerDisposalFeesExporter : ICalcResultSummaryProducerDisposalFeesExporter
    {
        public string Export(CalcResultSummaryProducerDisposalFees summary)
          => JsonSerializer.Serialize(
              new
              {
                  disposalFeeSummary1 = new
                  {
                      totalProducerDisposalFeeWithoutBadDebtProvision = summary.TotalProducerDisposalFee,
                      badDebtProvision = summary.BadDebtProvision,
                      totalProducerDisposalFeeWithBadDebtProvision = summary.TotalProducerDisposalFeeWithBadDebtProvision,
                      englandTotal = summary.EnglandTotal.ToString("C2"),
                      walesTotal = summary.WalesTotal.ToString("C2"),
                      scotlandTotal = summary.ScotlandTotal.ToString("C2"),
                      northernIrelandTotal = summary.NorthernIrelandTotal.ToString("C2"),
                      tonnageChangeCount = summary.TonnageChangeCount,
                      tonnageChangeAdvice = summary.TonnageChangeAdvice,
                  },
              },
              new JsonSerializerOptions
              {
                  PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                  WriteIndented = true,
                  Converters = { new Converter.CurrencyConverter() },

                  // This is required in order to output the £ symbol as-is rather than encoding it.
                  Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
              });
    }
}
