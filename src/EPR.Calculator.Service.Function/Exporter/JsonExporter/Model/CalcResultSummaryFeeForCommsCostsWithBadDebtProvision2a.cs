using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Utils;
using EPR.Calculator.Service.Function.Models;

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
            TotalProducerFeeForCommsCostsWithoutBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs.FeeWithoutBadDebtProvision),
            BadDebtProvisionFor2a                                = CurrencyConverterUtils.ConvertToCurrency(costs.BadDebtProvision),
            TotalProducerFeeForCommsCostsWithBadDebtProvision    = CurrencyConverterUtils.ConvertToCurrency(costs.FeeWithBadDebtProvision.Total),
            EnglandTotalWithBadDebtProvision                     = CurrencyConverterUtils.ConvertToCurrency(costs.FeeWithBadDebtProvision.England),
            WalesTotalWithBadDebtProvision                       = CurrencyConverterUtils.ConvertToCurrency(costs.FeeWithBadDebtProvision.Wales),
            ScotlandTotalWithBadDebtProvision                    = CurrencyConverterUtils.ConvertToCurrency(costs.FeeWithBadDebtProvision.Scotland),
            NorthernIrelandTotalWithBadDebtProvision             = CurrencyConverterUtils.ConvertToCurrency(costs.FeeWithBadDebtProvision.NorthernIreland),
            PercentageOfProducerTonnageVsAllProducers            = $"{Math.Round(calcResultSummaryProducerDisposalFees.PercentageofProducerReportedTonnagevsAllProducers, (int)DecimalPlaces.Eight).ToString()}%"
        };
    }
}
