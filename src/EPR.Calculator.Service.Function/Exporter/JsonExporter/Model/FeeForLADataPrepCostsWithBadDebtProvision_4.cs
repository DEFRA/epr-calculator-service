using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Utils;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

[SuppressMessage("Naming", "S101:Types should be named in PascalCase", Justification = "Required for JSON contract")]
public class FeeForLADataPrepCostsWithBadDebtProvision_4
{
    [JsonPropertyName("totalProducerFeeForLADataPrepCostsWithoutBadDebtProvision")]
    public required string TotalProducerFeeForLADataPrepCostsWithoutBadDebtProvision {get; set;}

    [JsonPropertyName("badDebtProvisionFor4")]
    public required string BadDebtProvisionFor4 {get; set;}

    [JsonPropertyName("totalProducerFeeForLADataPrepCostsWithBadDebtProvision")]
    public required string TotalProducerFeeForLADataPrepCostsWithBadDebtProvision {get; set;}

    [JsonPropertyName("englandTotalForLADataPrepCostsWithBadDebtProvision")]
    public required string EnglandTotalForLADataPrepCostsWithBadDebtProvision {get; set;}

    [JsonPropertyName("walesTotalForLADataPrepCostsWithBadDebtProvision")]
    public required string WalesTotalForLADataPrepCostsWithBadDebtProvision {get; set;}

    [JsonPropertyName("scotlandTotalForLADataPrepCostsWithBadDebtProvision")]
    public required string ScotlandTotalForLADataPrepCostsWithBadDebtProvision {get; set;}

    [JsonPropertyName("northernIrelandTotalForLADataPrepCostsWithBadDebtProvision")]
    public required string NorthernIrelandTotalForLADataPrepCostsWithBadDebtProvision { get; set;}

    public static FeeForLADataPrepCostsWithBadDebtProvision_4 From(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
    {
        var costs = calcResultSummaryProducerDisposalFees.LaDataPrepSection4;
        return new FeeForLADataPrepCostsWithBadDebtProvision_4
        {
            TotalProducerFeeForLADataPrepCostsWithoutBadDebtProvision  = CurrencyConverterUtils.ConvertToCurrency(costs.FeeWithoutBadDebtProvision),
            BadDebtProvisionFor4                                       = CurrencyConverterUtils.ConvertToCurrency(costs.BadDebtProvision),
            TotalProducerFeeForLADataPrepCostsWithBadDebtProvision     = CurrencyConverterUtils.ConvertToCurrency(costs.FeeWithBadDebtProvision.Total),
            EnglandTotalForLADataPrepCostsWithBadDebtProvision         = CurrencyConverterUtils.ConvertToCurrency(costs.FeeWithBadDebtProvision.England),
            WalesTotalForLADataPrepCostsWithBadDebtProvision           = CurrencyConverterUtils.ConvertToCurrency(costs.FeeWithBadDebtProvision.Wales),
            ScotlandTotalForLADataPrepCostsWithBadDebtProvision        = CurrencyConverterUtils.ConvertToCurrency(costs.FeeWithBadDebtProvision.Scotland),
            NorthernIrelandTotalForLADataPrepCostsWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(costs.FeeWithBadDebtProvision.NorthernIreland)
        };
    }
}
