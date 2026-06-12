using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Utils;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

public class CalcResultSAOperatingCostsWithBadDebtProvision
{

    [JsonPropertyName("totalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithoutBadDebtProvision")]
    public required string  TotalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithoutBadDebtProvision { get; set; }

    [JsonPropertyName("badDebProvisionFor3")]
    public required string BadDebtProvisionFor3 { get; set; }

    [JsonPropertyName("totalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithBadDebtProvision")]
    public required string TotalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithBadDebtProvision { get; set; }

    [JsonPropertyName("englandTotalForSAOperatingCostsWithBadDebtProvision")]
    public required string EnglandTotalForSAOperatingCostsWithBadDebtProvision { get; set; }

    [JsonPropertyName("walesTotalForSAOperatingCostsWithBadDebtProvision")]
    public required string WalesTotalForSAOperatingCostsWithBadDebtProvision { get; set; }

    [JsonPropertyName("scotlandTotalForSAOperatingCostsWithBadDebtProvision")]
    public required string ScotlandTotalForSAOperatingCostsWithBadDebtProvision { get; set; }

    [JsonPropertyName("northernIrelandTotalForSAOperatingCostsWithBadDebtProvision")]
    public required string NorthernIrelandTotalForSAOperatingCostsWithBadDebtProvision { get; set; }

     public static CalcResultSAOperatingCostsWithBadDebtProvision From(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
    {
        var costs = calcResultSummaryProducerDisposalFees.SaOperatingCostsSection3;
        return new CalcResultSAOperatingCostsWithBadDebtProvision
        {
            TotalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithoutBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs.FeeWithoutBadDebtProvision),
            BadDebtProvisionFor3                                                   = CurrencyConverterUtils.ConvertToCurrency(costs.BadDebtProvision),
            TotalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithBadDebtProvision    = CurrencyConverterUtils.ConvertToCurrency(costs.FeeWithBadDebtProvision.Total),
            EnglandTotalForSAOperatingCostsWithBadDebtProvision                    = CurrencyConverterUtils.ConvertToCurrency(costs.FeeWithBadDebtProvision.England),
            WalesTotalForSAOperatingCostsWithBadDebtProvision                      = CurrencyConverterUtils.ConvertToCurrency(costs.FeeWithBadDebtProvision.Wales),
            ScotlandTotalForSAOperatingCostsWithBadDebtProvision                   = CurrencyConverterUtils.ConvertToCurrency(costs.FeeWithBadDebtProvision.Scotland),
            NorthernIrelandTotalForSAOperatingCostsWithBadDebtProvision            = CurrencyConverterUtils.ConvertToCurrency(costs.FeeWithBadDebtProvision.NorthernIreland)
        };
    }
}
