using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

public class FeeForSaSetUpCostsWithBadDebtProvision5
{
    [JsonPropertyName("totalProducerOneOffFeeForSASetUpCostsWithoutBadDebtProvision")]
    public string? TotalProducerOneOffFeeForSASetUpCostsWithoutBadDebtProvision { get; set; }

    [JsonPropertyName("badDebtProvisionFor5")]
    public string? BadDebtProvisionFor5 { get; set; }

    [JsonPropertyName("totalProducerOneOffFeeForSASetUpCostsWithBadDebtProvision")]
    public string? TotalProducerOneOffFeeForSASetUpCostsWithBadDebtProvision { get; set; }

    [JsonPropertyName("englandTotalForSASetUpCostsWithBadDebtProvision")]
    public string? EnglandTotalForSASetUpCostsWithBadDebtProvision { get; set; }

    [JsonPropertyName("walesTotalForSASetUpCostsWithBadDebtProvision")]
    public string? WalesTotalForSASetUpCostsWithBadDebtProvision { get; set; }

    [JsonPropertyName("scotlandTotalForSASetUpCostsWithBadDebtProvision")]
    public string? ScotlandTotalForSASetUpCostsWithBadDebtProvision { get; set; }

    [JsonPropertyName("northernIrelandTotalForSASetUpCostsWithBadDebtProvision")]
    public string? NorthernIrelandTotalForSASetUpCostsWithBadDebtProvision { get; set; }

    public static FeeForSaSetUpCostsWithBadDebtProvision5 From(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
    {
        var costs = calcResultSummaryProducerDisposalFees.SaSetupCostsSection5;
        return new FeeForSaSetUpCostsWithBadDebtProvision5
        {
            TotalProducerOneOffFeeForSASetUpCostsWithoutBadDebtProvision = FormatUtils.FormatCurrency(costs.FeeWithoutBadDebtProvision),
            BadDebtProvisionFor5                                         = FormatUtils.FormatCurrency(costs.BadDebtProvision),
            TotalProducerOneOffFeeForSASetUpCostsWithBadDebtProvision    = FormatUtils.FormatCurrency(costs.FeeWithBadDebtProvision.Total),
            EnglandTotalForSASetUpCostsWithBadDebtProvision              = FormatUtils.FormatCurrency(costs.FeeWithBadDebtProvision.England),
            WalesTotalForSASetUpCostsWithBadDebtProvision                = FormatUtils.FormatCurrency(costs.FeeWithBadDebtProvision.Wales),
            ScotlandTotalForSASetUpCostsWithBadDebtProvision             = FormatUtils.FormatCurrency(costs.FeeWithBadDebtProvision.Scotland),
            NorthernIrelandTotalForSASetUpCostsWithBadDebtProvision      = FormatUtils.FormatCurrency(costs.FeeWithBadDebtProvision.NorthernIreland)
        };
    }
}
