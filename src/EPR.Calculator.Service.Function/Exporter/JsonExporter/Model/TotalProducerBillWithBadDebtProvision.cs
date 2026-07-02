using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

public record TotalProducerBillWithBadDebtProvision
{
    [JsonPropertyName("totalProducerBillWithoutBadDebtProvision")]
    public required string TotalProducerBillWithoutBadDebtProvision { get; set; }

    [JsonPropertyName("badDebtProvisionForTotalProducerBill")]
    public required string BadDebtProvisionForTotalProducerBill { get; set; }

    [JsonPropertyName("totalProducerBillWithBadDebtProvision")]
    public required string TotalProducerBillWithBadDebtProvisionAmount { get; set; }

    [JsonPropertyName("englandTotalForProducerBillWithBadDebtProvision")]
    public required string EnglandTotalForProducerBillWithBadDebtProvision { get; set; }

    [JsonPropertyName("walesTotalForProducerBillWithBadDebtProvision")]
    public required string WalesTotalForProducerBillWithBadDebtProvision { get; set; }

    [JsonPropertyName("scotlandTotalForProducerBillWithBadDebtProvision")]
    public required string ScotlandTotalForProducerBillWithBadDebtProvision { get; set; }

    [JsonPropertyName("northernIrelandTotalForProducerBillWithBadDebtProvision")]
    public required string NorthernIrelandTotalForProducerBillWithBadDebtProvision { get; set; }

    public static TotalProducerBillWithBadDebtProvision From(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
    {
        var costs = calcResultSummaryProducerDisposalFees.TotalProducerBillBreakdownCosts;
        return new TotalProducerBillWithBadDebtProvision
        {
            TotalProducerBillWithoutBadDebtProvision                = FormatUtils.FormatCurrency(costs.FeeWithoutBadDebtProvision),
            BadDebtProvisionForTotalProducerBill                    = FormatUtils.FormatCurrency(costs.BadDebtProvision),
            TotalProducerBillWithBadDebtProvisionAmount             = FormatUtils.FormatCurrency(costs.FeeWithBadDebtProvision.Total),
            EnglandTotalForProducerBillWithBadDebtProvision         = FormatUtils.FormatCurrency(costs.FeeWithBadDebtProvision.England),
            WalesTotalForProducerBillWithBadDebtProvision           = FormatUtils.FormatCurrency(costs.FeeWithBadDebtProvision.Wales),
            ScotlandTotalForProducerBillWithBadDebtProvision        = FormatUtils.FormatCurrency(costs.FeeWithBadDebtProvision.Scotland),
            NorthernIrelandTotalForProducerBillWithBadDebtProvision = FormatUtils.FormatCurrency(costs.FeeWithBadDebtProvision.NorthernIreland)
        };
    }
}
