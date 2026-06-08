using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Utils;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

public class CalculationResultsProducerCalculationResultsFeeForLADisposalCosts1
{
    [JsonPropertyName("totalProducerFeeForLADisposalCostsWithoutBadDebtProvision")]
    public required string TotalProducerFeeForLADisposalCostsWithoutBadDebtProvision { get; set; }

    [JsonPropertyName("badDebtProvisionForLADisposalCosts")]
    public required string BadDebtProvisionForLADisposalCosts { get; set; }

    [JsonPropertyName("totalProducerFeeForLADisposalCostsWithBadDebtProvision")]
    public required string TotalProducerFeeForLADisposalCostsWithBadDebtProvision { get; set; }

    [JsonPropertyName("englandTotalForLADisposalCostsWithBadDebtProvision")]
    public required string EnglandTotalForLADisposalCostsWithBadDebtProvision { get; set; }

    [JsonPropertyName("walesTotalForLADisposalCostsWithBadDebtProvision")]
    public required string WalesTotalForLADisposalCostsWithBadDebtProvision { get; set; }

    [JsonPropertyName("scotlandTotalForLADisposalCostsWithBadDebtProvision")]
    public required string ScotlandTotalForLADisposalCostsWithBadDebtProvision { get; set; }

    [JsonPropertyName("northernIrelandTotalForLADisposalCostsWithBadDebtProvision")]
    public required string NorthernIrelandTotalForLADisposalCostsWithBadDebtProvision { get; set; }

    public static CalculationResultsProducerCalculationResultsFeeForLADisposalCosts1 From(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
    {
        var costs = calcResultSummaryProducerDisposalFees.LocalAuthorityDisposalCostsSectionOne;
        return new CalculationResultsProducerCalculationResultsFeeForLADisposalCosts1
        {
            TotalProducerFeeForLADisposalCostsWithoutBadDebtProvision  = CurrencyConverterUtils.ConvertToCurrency(costs?.TotalProducerFeeWithoutBadDebtProvision),
            BadDebtProvisionForLADisposalCosts                         = CurrencyConverterUtils.ConvertToCurrency(costs?.BadDebtProvision),
            TotalProducerFeeForLADisposalCostsWithBadDebtProvision     = CurrencyConverterUtils.ConvertToCurrency(costs?.TotalProducerFeeWithBadDebtProvision.Total),
            EnglandTotalForLADisposalCostsWithBadDebtProvision         = CurrencyConverterUtils.ConvertToCurrency(costs?.TotalProducerFeeWithBadDebtProvision.England),
            WalesTotalForLADisposalCostsWithBadDebtProvision           = CurrencyConverterUtils.ConvertToCurrency(costs?.TotalProducerFeeWithBadDebtProvision.Wales),
            ScotlandTotalForLADisposalCostsWithBadDebtProvision        = CurrencyConverterUtils.ConvertToCurrency(costs?.TotalProducerFeeWithBadDebtProvision.Scotland),
            NorthernIrelandTotalForLADisposalCostsWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.TotalProducerFeeWithBadDebtProvision.NorthernIreland),
        };
    }
}
