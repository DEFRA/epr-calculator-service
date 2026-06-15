using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

public class CalcResultSummaryCommsCostsByMaterialFeesSummary2A
{
    [JsonPropertyName("totalProducerFeeForCommsCostsWithoutBadDebtProvision2a")]
    public required string  TotalProducerFeeForCommsCostsWithoutBadDebtProvision2a { get; set; }

    [JsonPropertyName("totalBadDebtProvision")]
    public required string TotalBadDebtProvision { get; set; }

    [JsonPropertyName("totalProducerFeeForCommsCostsWithBadDebtProvision2a")]
    public required string TotalProducerFeeForCommsCostsWithBadDebtProvision2a { get; set; }

    [JsonPropertyName("englandTotalWithBadDebtProvision")]
    public required string EnglandTotalWithBadDebtProvision { get; set; }

    [JsonPropertyName("walesTotalWithBadDebtProvision")]
    public required string WalesTotalWithBadDebtProvision { get; set; }

    [JsonPropertyName("scotlandTotalWithBadDebtProvision")]
    public required string ScotlandTotalWithBadDebtProvision { get; set; }

    [JsonPropertyName("northernIrelandTotalWithBadDebtProvision")]
    public required string NorthernIrelandTotalWithBadDebtProvision { get; set; }

    public static CalcResultSummaryCommsCostsByMaterialFeesSummary2A From(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
    {
        var costs = calcResultSummaryProducerDisposalFees.CommsCostsSection2a;
        return new CalcResultSummaryCommsCostsByMaterialFeesSummary2A
        {
            TotalProducerFeeForCommsCostsWithoutBadDebtProvision2a = FormatUtils.FormatCurrency(costs.FeeWithoutBadDebtProvision),
            TotalBadDebtProvision                                  = FormatUtils.FormatCurrency(costs.BadDebtProvision),
            TotalProducerFeeForCommsCostsWithBadDebtProvision2a    = FormatUtils.FormatCurrency(costs.FeeWithBadDebtProvision.Total),
            EnglandTotalWithBadDebtProvision                       = FormatUtils.FormatCurrency(costs.FeeWithBadDebtProvision.England),
            WalesTotalWithBadDebtProvision                         = FormatUtils.FormatCurrency(costs.FeeWithBadDebtProvision.Wales),
            ScotlandTotalWithBadDebtProvision                      = FormatUtils.FormatCurrency(costs.FeeWithBadDebtProvision.Scotland),
            NorthernIrelandTotalWithBadDebtProvision               = FormatUtils.FormatCurrency(costs.FeeWithBadDebtProvision.NorthernIreland)
        };
    }
}
