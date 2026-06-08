using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Utils;
using EPR.Calculator.Service.Function.Models;

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
        var costs = calcResultSummaryProducerDisposalFees.CommunicationCostsSectionTwoB;
        return new CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2B
        {
            TotalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs?.FeeWithoutBadDebtProvision),
            BadDebtProvisionFor2b                                      = CurrencyConverterUtils.ConvertToCurrency(costs?.BadDebtProvision),
            TotalProducerFeeForCommsCostsUKWideWithBadDebtProvision    = CurrencyConverterUtils.ConvertToCurrency(costs?.FeeWithBadDebtProvision.Total),
            EnglandTotalWithBadDebtProvision                           = CurrencyConverterUtils.ConvertToCurrency(costs?.FeeWithBadDebtProvision.England),
            WalesTotalWithBadDebtProvision                             = CurrencyConverterUtils.ConvertToCurrency(costs?.FeeWithBadDebtProvision.Wales),
            ScotlandTotalWithBadDebtProvision                          = CurrencyConverterUtils.ConvertToCurrency(costs?.FeeWithBadDebtProvision.Scotland),
            NorthernIrelandTotalWithBadDebtProvision                   = CurrencyConverterUtils.ConvertToCurrency(costs?.FeeWithBadDebtProvision.NorthernIreland)
        };
    }
}
