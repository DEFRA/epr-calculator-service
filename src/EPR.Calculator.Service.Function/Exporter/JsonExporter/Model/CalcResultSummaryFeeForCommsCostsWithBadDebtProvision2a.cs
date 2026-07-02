using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

public class CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2A
{
    [JsonPropertyName("totalProducerFeeForCommsCostsWithoutBadDebtProvision")]
    public string? TotalProducerFeeForCommsCostsWithoutBadDebtProvision { get; set; }

    [JsonPropertyName("badDebtProvisionFor2a")]
    public string? BadDebtProvisionFor2a { get; set; }

    [JsonPropertyName("totalProducerFeeForCommsCostsWithBadDebtProvision")]
    public string? TotalProducerFeeForCommsCostsWithBadDebtProvision { get; set; }

    [JsonPropertyName("englandTotalWithBadDebtProvision")]
    public string? EnglandTotalWithBadDebtProvision { get; set; }

    [JsonPropertyName("walesTotalWithBadDebtProvision")]
    public string? WalesTotalWithBadDebtProvision { get; set; }

    [JsonPropertyName("scotlandTotalWithBadDebtProvision")]
    public string? ScotlandTotalWithBadDebtProvision { get; set; }

    [JsonPropertyName("northernIrelandTotalWithBadDebtProvision")]
    public string? NorthernIrelandTotalWithBadDebtProvision { get; set; }

    [JsonPropertyName("percentageOfProducerTonnageVsAllProducers")]
    public string? PercentageOfProducerTonnageVsAllProducers { get; set; }

    public static CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2A From(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
    {
        var costs = calcResultSummaryProducerDisposalFees.CommsCostsSection2a;
        return new CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2A
        {
            TotalProducerFeeForCommsCostsWithoutBadDebtProvision = FormatUtils.FormatCurrency(costs.FeeWithoutBadDebtProvision),
            BadDebtProvisionFor2a                                = FormatUtils.FormatCurrency(costs.BadDebtProvision),
            TotalProducerFeeForCommsCostsWithBadDebtProvision    = FormatUtils.FormatCurrency(costs.FeeWithBadDebtProvision.Total),
            EnglandTotalWithBadDebtProvision                     = FormatUtils.FormatCurrency(costs.FeeWithBadDebtProvision.England),
            WalesTotalWithBadDebtProvision                       = FormatUtils.FormatCurrency(costs.FeeWithBadDebtProvision.Wales),
            ScotlandTotalWithBadDebtProvision                    = FormatUtils.FormatCurrency(costs.FeeWithBadDebtProvision.Scotland),
            NorthernIrelandTotalWithBadDebtProvision             = FormatUtils.FormatCurrency(costs.FeeWithBadDebtProvision.NorthernIreland),
            PercentageOfProducerTonnageVsAllProducers            = FormatUtils.FormatPercentage(calcResultSummaryProducerDisposalFees.PercentageofProducerReportedTonnagevsAllProducers)
        };
    }
}
