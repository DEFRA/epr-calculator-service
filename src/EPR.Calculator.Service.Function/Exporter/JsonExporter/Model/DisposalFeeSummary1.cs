using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Utils;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

public class DisposalFeeSummary1
{
    [JsonPropertyName("totalProducerDisposalFeeWithoutBadDebtProvision")]
    public required string TotalProducerDisposalFeeWithoutBadDebtProvision { get; set; }

    [JsonPropertyName("badDebtProvision")]
    public required string BadDebtProvision { get; set; }

    [JsonPropertyName("totalProducerDisposalFeeWithBadDebtProvision")]
    public required string TotalProducerDisposalFeeWithBadDebtProvision { get; set; }

    [JsonPropertyName("englandTotal")]
    public required string EnglandTotal { get; set; }

    [JsonPropertyName("walesTotal")]
    public required string WalesTotal { get; set; }

    [JsonPropertyName("scotlandTotal")]
    public required string ScotlandTotal { get; set; }

    [JsonPropertyName("northernIrelandTotal")]
    public required string NorthernIrelandTotal { get; set; }

    [JsonPropertyName("tonnageChangeCount")]
    public required string TonnageChangeCount { get; set; }

    [JsonPropertyName("tonnageChangeAdvice")]
    public required string TonnageChangeAdvice { get; set; }

    public static DisposalFeeSummary1 From(CalcResultSummaryProducerDisposalFees summary)
    {
        return new DisposalFeeSummary1
        {
            TotalProducerDisposalFeeWithoutBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(summary.LADisposalCostsSection1.FeeWithoutBadDebtProvision),
            BadDebtProvision                                = CurrencyConverterUtils.ConvertToCurrency(summary.LADisposalCostsSection1.BadDebtProvision),
            TotalProducerDisposalFeeWithBadDebtProvision    = CurrencyConverterUtils.ConvertToCurrency(summary.LADisposalCostsSection1.FeeWithBadDebtProvision.Total),
            EnglandTotal                                    = CurrencyConverterUtils.ConvertToCurrency(summary.LADisposalCostsSection1.FeeWithBadDebtProvision.England),
            WalesTotal                                      = CurrencyConverterUtils.ConvertToCurrency(summary.LADisposalCostsSection1.FeeWithBadDebtProvision.Wales),
            ScotlandTotal                                   = CurrencyConverterUtils.ConvertToCurrency(summary.LADisposalCostsSection1.FeeWithBadDebtProvision.Scotland),
            NorthernIrelandTotal                            = CurrencyConverterUtils.ConvertToCurrency(summary.LADisposalCostsSection1.FeeWithBadDebtProvision.NorthernIreland),
            TonnageChangeCount                              = summary.TonnageChangeCount ?? CommonConstants.Hyphen,
            TonnageChangeAdvice                             = summary.TonnageChangeAdvice ?? CommonConstants.Hyphen,
        };
    }
}
