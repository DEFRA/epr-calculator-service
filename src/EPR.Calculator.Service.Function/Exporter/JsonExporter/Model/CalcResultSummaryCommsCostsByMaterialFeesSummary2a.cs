using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Utils;
using EPR.Calculator.Service.Function.Models;

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
        var costs = calcResultSummaryProducerDisposalFees.CommunicationCostsSectionTwoA;
        return new CalcResultSummaryCommsCostsByMaterialFeesSummary2A
        {
            TotalProducerFeeForCommsCostsWithoutBadDebtProvision2a = CurrencyConverterUtils.ConvertToCurrency(costs?.FeeWithoutBadDebtProvision),
            TotalBadDebtProvision                                  = CurrencyConverterUtils.ConvertToCurrency(costs?.BadDebtProvision),
            TotalProducerFeeForCommsCostsWithBadDebtProvision2a    = CurrencyConverterUtils.ConvertToCurrency(costs?.FeeWithBadDebtProvision?.Total),
            EnglandTotalWithBadDebtProvision                       = CurrencyConverterUtils.ConvertToCurrency(costs?.FeeWithBadDebtProvision?.England),
            WalesTotalWithBadDebtProvision                         = CurrencyConverterUtils.ConvertToCurrency(costs?.FeeWithBadDebtProvision?.Wales),
            ScotlandTotalWithBadDebtProvision                      = CurrencyConverterUtils.ConvertToCurrency(costs?.FeeWithBadDebtProvision?.Scotland),
            NorthernIrelandTotalWithBadDebtProvision               = CurrencyConverterUtils.ConvertToCurrency(costs?.FeeWithBadDebtProvision?.NorthernIreland)
        };
    }
}
