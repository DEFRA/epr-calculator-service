using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Utils;
using EPR.Calculator.Service.Function.Models;

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
        var costs = calcResultSummaryProducerDisposalFees.OneOffSchemeAdministrationSetupCosts;
        return new FeeForSaSetUpCostsWithBadDebtProvision5
        {
            TotalProducerOneOffFeeForSASetUpCostsWithoutBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.TotalProducerFeeWithoutBadDebtProvision),
            BadDebtProvisionFor5                                         = CurrencyConverterUtils.ConvertToCurrency(costs?.BadDebtProvision),
            TotalProducerOneOffFeeForSASetUpCostsWithBadDebtProvision    = CurrencyConverterUtils.ConvertToCurrency(costs?.TotalProducerFeeWithBadDebtProvision.Total),
            EnglandTotalForSASetUpCostsWithBadDebtProvision              = CurrencyConverterUtils.ConvertToCurrency(costs?.TotalProducerFeeWithBadDebtProvision.England),
            WalesTotalForSASetUpCostsWithBadDebtProvision                = CurrencyConverterUtils.ConvertToCurrency(costs?.TotalProducerFeeWithBadDebtProvision.Wales),
            ScotlandTotalForSASetUpCostsWithBadDebtProvision             = CurrencyConverterUtils.ConvertToCurrency(costs?.TotalProducerFeeWithBadDebtProvision.Scotland),
            NorthernIrelandTotalForSASetUpCostsWithBadDebtProvision      = CurrencyConverterUtils.ConvertToCurrency(costs?.TotalProducerFeeWithBadDebtProvision.NorthernIreland)
        };
    }
}
