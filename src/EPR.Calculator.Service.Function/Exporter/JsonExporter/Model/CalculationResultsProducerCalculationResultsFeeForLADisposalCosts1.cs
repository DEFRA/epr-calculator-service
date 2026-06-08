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
        var costs = calcResultSummaryProducerDisposalFees.LADisposalCostsSection1;
        return new CalculationResultsProducerCalculationResultsFeeForLADisposalCosts1
        {
            TotalProducerFeeForLADisposalCostsWithoutBadDebtProvision  = CurrencyConverterUtils.ConvertToCurrency(costs.FeeWithoutBadDebtProvision),
            BadDebtProvisionForLADisposalCosts                         = CurrencyConverterUtils.ConvertToCurrency(costs.BadDebtProvision),
            TotalProducerFeeForLADisposalCostsWithBadDebtProvision     = CurrencyConverterUtils.ConvertToCurrency(costs.FeeWithBadDebtProvision.Total),
            EnglandTotalForLADisposalCostsWithBadDebtProvision         = CurrencyConverterUtils.ConvertToCurrency(costs.FeeWithBadDebtProvision.England),
            WalesTotalForLADisposalCostsWithBadDebtProvision           = CurrencyConverterUtils.ConvertToCurrency(costs.FeeWithBadDebtProvision.Wales),
            ScotlandTotalForLADisposalCostsWithBadDebtProvision        = CurrencyConverterUtils.ConvertToCurrency(costs.FeeWithBadDebtProvision.Scotland),
            NorthernIrelandTotalForLADisposalCostsWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs.FeeWithBadDebtProvision.NorthernIreland),
        };
    }
}
