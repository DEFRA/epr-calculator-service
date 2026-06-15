using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Utils;

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
            TotalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithoutBadDebtProvision = FormatUtils.FormatCurrency(costs.FeeWithoutBadDebtProvision),
            BadDebtProvisionFor3                                                   = FormatUtils.FormatCurrency(costs.BadDebtProvision),
            TotalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithBadDebtProvision    = FormatUtils.FormatCurrency(costs.FeeWithBadDebtProvision.Total),
            EnglandTotalForSAOperatingCostsWithBadDebtProvision                    = FormatUtils.FormatCurrency(costs.FeeWithBadDebtProvision.England),
            WalesTotalForSAOperatingCostsWithBadDebtProvision                      = FormatUtils.FormatCurrency(costs.FeeWithBadDebtProvision.Wales),
            ScotlandTotalForSAOperatingCostsWithBadDebtProvision                   = FormatUtils.FormatCurrency(costs.FeeWithBadDebtProvision.Scotland),
            NorthernIrelandTotalForSAOperatingCostsWithBadDebtProvision            = FormatUtils.FormatCurrency(costs.FeeWithBadDebtProvision.NorthernIreland)
        };
    }
}
