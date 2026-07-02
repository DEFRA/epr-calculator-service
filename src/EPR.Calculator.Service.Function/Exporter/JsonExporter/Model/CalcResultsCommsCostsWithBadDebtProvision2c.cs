using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

public class CalcResultsCommsCostsWithBadDebtProvision2C
{
    [JsonPropertyName("totalProducerFeeForCommsCostsByCountryWithoutBadDebtProvision")]
    public string? TotalProducerFeeForCommsCostsByCountryWithoutBadDebtProvision { get; set; }

    [JsonPropertyName("badDebProvisionFor2c")]
    public string? BadDebtProvisionFor2c { get; set; }

    [JsonPropertyName("totalProducerFeeForCommsCostsByCountryWithBadDebtProvision")]
    public string? TotalProducerFeeForCommsCostsByCountryWithBadDebtProvision { get; set; }


    [JsonPropertyName("englandTotalWithBadDebtProvision")]
    public string? EnglandTotalWithBadDebtProvision { get; set; }

    [JsonPropertyName("walesTotalWithBadDebtProvision")]
    public string? WalesTotalWithBadDebtProvision { get; set; }

    [JsonPropertyName("scotlandTotalWithBadDebtProvision")]
    public string? ScotlandTotalWithBadDebtProvision { get; set; }

    [JsonPropertyName("northernIrelandTotalWithBadDebtProvision")]
    public string? NorthernIrelandTotalWithBadDebtProvision { get; set; }

    public static CalcResultsCommsCostsWithBadDebtProvision2C From(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
    {
        return new CalcResultsCommsCostsWithBadDebtProvision2C
        {
            TotalProducerFeeForCommsCostsByCountryWithoutBadDebtProvision = FormatUtils.FormatCurrency(calcResultSummaryProducerDisposalFees.CommsCostsSection2c.FeeWithoutBadDebtProvision),
            BadDebtProvisionFor2c                                         = FormatUtils.FormatCurrency(calcResultSummaryProducerDisposalFees.CommsCostsSection2c.BadDebtProvision),
            TotalProducerFeeForCommsCostsByCountryWithBadDebtProvision    = FormatUtils.FormatCurrency(calcResultSummaryProducerDisposalFees.CommsCostsSection2c.FeeWithBadDebtProvision.Total),
            EnglandTotalWithBadDebtProvision                              = FormatUtils.FormatCurrency(calcResultSummaryProducerDisposalFees.CommsCostsSection2c.FeeWithBadDebtProvision.England),
            WalesTotalWithBadDebtProvision                                = FormatUtils.FormatCurrency(calcResultSummaryProducerDisposalFees.CommsCostsSection2c.FeeWithBadDebtProvision.Wales),
            ScotlandTotalWithBadDebtProvision                             = FormatUtils.FormatCurrency(calcResultSummaryProducerDisposalFees.CommsCostsSection2c.FeeWithBadDebtProvision.Scotland),
            NorthernIrelandTotalWithBadDebtProvision                      = FormatUtils.FormatCurrency(calcResultSummaryProducerDisposalFees.CommsCostsSection2c.FeeWithBadDebtProvision.NorthernIreland)
        };
    }

}
