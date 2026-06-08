using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Utils;
using EPR.Calculator.Service.Function.Models;

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
            TotalProducerFeeForCommsCostsByCountryWithoutBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(calcResultSummaryProducerDisposalFees.CommsCostsSection2c.FeeWithoutBadDebtProvision),
            BadDebtProvisionFor2c                                         = CurrencyConverterUtils.ConvertToCurrency(calcResultSummaryProducerDisposalFees.CommsCostsSection2c.BadDebtProvision),
            TotalProducerFeeForCommsCostsByCountryWithBadDebtProvision    = CurrencyConverterUtils.ConvertToCurrency(calcResultSummaryProducerDisposalFees.CommsCostsSection2c.FeeWithBadDebtProvision.Total),
            EnglandTotalWithBadDebtProvision                              = CurrencyConverterUtils.ConvertToCurrency(calcResultSummaryProducerDisposalFees.CommsCostsSection2c.FeeWithBadDebtProvision.England),
            WalesTotalWithBadDebtProvision                                = CurrencyConverterUtils.ConvertToCurrency(calcResultSummaryProducerDisposalFees.CommsCostsSection2c.FeeWithBadDebtProvision.Wales),
            ScotlandTotalWithBadDebtProvision                             = CurrencyConverterUtils.ConvertToCurrency(calcResultSummaryProducerDisposalFees.CommsCostsSection2c.FeeWithBadDebtProvision.Scotland),
            NorthernIrelandTotalWithBadDebtProvision                      = CurrencyConverterUtils.ConvertToCurrency(calcResultSummaryProducerDisposalFees.CommsCostsSection2c.FeeWithBadDebtProvision.NorthernIreland)
        };
    }

}
