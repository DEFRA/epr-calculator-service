using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

public class CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2B
{
    [JsonPropertyName("totalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision")]
    public string? TotalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision { get; set; }

    [JsonPropertyName("badDebtProvisionFor2b")]
    public string? BadDebtProvisionFor2b { get; set; }

    [JsonPropertyName("totalProducerFeeForCommsCostsUKWideWithBadDebtProvision")]
    public string? TotalProducerFeeForCommsCostsUKWideWithBadDebtProvision { get; set; }

    [JsonPropertyName("englandTotalWithBadDebtProvision")]
    public string? EnglandTotalWithBadDebtProvision { get; set; }

    [JsonPropertyName("walesTotalWithBadDebtProvision")]
    public string? WalesTotalWithBadDebtProvision { get; set; }

    [JsonPropertyName("scotlandTotalWithBadDebtProvision")]
    public string? ScotlandTotalWithBadDebtProvision { get; set; }

    [JsonPropertyName("northernIrelandTotalWithBadDebtProvision")]
    public string? NorthernIrelandTotalWithBadDebtProvision { get; set; }

    public static CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2B From(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
    {
        var costs = calcResultSummaryProducerDisposalFees.CommsCostsSection2b;
        return new CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2B
        {
            TotalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision = FormatUtils.FormatCurrency(costs.FeeWithoutBadDebtProvision),
            BadDebtProvisionFor2b                                      = FormatUtils.FormatCurrency(costs.BadDebtProvision),
            TotalProducerFeeForCommsCostsUKWideWithBadDebtProvision    = FormatUtils.FormatCurrency(costs.FeeWithBadDebtProvision.Total),
            EnglandTotalWithBadDebtProvision                           = FormatUtils.FormatCurrency(costs.FeeWithBadDebtProvision.England),
            WalesTotalWithBadDebtProvision                             = FormatUtils.FormatCurrency(costs.FeeWithBadDebtProvision.Wales),
            ScotlandTotalWithBadDebtProvision                          = FormatUtils.FormatCurrency(costs.FeeWithBadDebtProvision.Scotland),
            NorthernIrelandTotalWithBadDebtProvision                   = FormatUtils.FormatCurrency(costs.FeeWithBadDebtProvision.NorthernIreland)
        };
    }
}
