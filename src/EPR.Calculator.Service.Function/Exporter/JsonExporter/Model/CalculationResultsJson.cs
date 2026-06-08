using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Features.BillingRun.Contexts;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

public class CalculationResultsJson
{
    [JsonPropertyName("producerCalculationResultsSummary")]
    public required ProducerCalculationResultsSummary ProducerCalculationResultsSummary { get; set; }

    [JsonPropertyName("producerCalculationResults")]
    public required IEnumerable<CalcSummaryProducerCalculationResults> ProducerCalculationResults { get; set; }

    [JsonPropertyName("producerCalculationResultsTotal")]
    public CalcResultProducerCalculationResultsTotal? ProducerCalculationResultsTotal { get; set; }

    public static CalculationResultsJson From(
        BillingRunContext runContext,
        CalcResult calcResult,
        IImmutableList<MaterialDetail> materials)
    {
        return new CalculationResultsJson
        {
            ProducerCalculationResultsSummary = ArrangeSummary(calcResult.CalcResultSummary),
            ProducerCalculationResults        = ArrangeProducerCalculationResult(runContext, calcResult, materials),
            ProducerCalculationResultsTotal   = ArrangeProducerCalculationResultsTotal(calcResult.CalcResultSummary),
        };
    }

    /// <summary>
    /// Arrange the CalcResultSummary data using the property
    /// names and ordering required for serialisation.
    /// </summary>
    private static ProducerCalculationResultsSummary ArrangeSummary(CalcResultSummary data)
    {
        return new ProducerCalculationResultsSummary
        {
            FeeForLaDisposalCostsWithoutBadDebtprovision1 = CurrencyConverterUtils.ConvertToCurrency(data.LADisposalCostsSection1.FeeWithoutBadDebtProvision),
            BadDebtProvision1                             = CurrencyConverterUtils.ConvertToCurrency(data.LADisposalCostsSection1.BadDebtProvision),
            FeeForLaDisposalCostsWithBadDebtprovision1    = CurrencyConverterUtils.ConvertToCurrency(data.LADisposalCostsSection1.FeeWithBadDebtProvision.Total),

            FeeForCommsCostsByMaterialWithoutBadDebtprovision2a = CurrencyConverterUtils.ConvertToCurrency(data.CommsCostsSection2a.FeeWithoutBadDebtProvision),
            BadDebtProvision2a                                  = CurrencyConverterUtils.ConvertToCurrency(data.CommsCostsSection2a.BadDebtProvision),
            FeeForCommsCostsByMaterialWitBadDebtprovision2a     = CurrencyConverterUtils.ConvertToCurrency(data.CommsCostsSection2a.FeeWithBadDebtProvision.Total),

            FeeForCommsCostsUkWideWithoutBadDebtprovision2b = CurrencyConverterUtils.ConvertToCurrency(data.CommsCostsSection2b.FeeWithoutBadDebtProvision),
            BadDebtProvision2b                              = CurrencyConverterUtils.ConvertToCurrency(data.CommsCostsSection2b.BadDebtProvision),
            FeeForCommsCostsUkWideWithBadDebtprovision2b    = CurrencyConverterUtils.ConvertToCurrency(data.CommsCostsSection2b.FeeWithBadDebtProvision.Total),

            FeeForCommsCostsByCountryWithoutBadDebtprovision2c  = CurrencyConverterUtils.ConvertToCurrency(data.CommsCostsSection2c.FeeWithoutBadDebtProvision),
            BadDebtProvision2c                                  = CurrencyConverterUtils.ConvertToCurrency(data.CommsCostsSection2c.BadDebtProvision),
            FeeForCommsCostsByCountryWideWithBadDebtprovision2c = CurrencyConverterUtils.ConvertToCurrency(data.CommsCostsSection2c.FeeWithBadDebtProvision.Total),

            Total12a2b2cWithBadDebt = CurrencyConverterUtils.ConvertToCurrency(data.TotalOnePlus2A2B2CFeeWithBadDebtProvision),

            SaOperatingCostsWithoutBadDebtProvision3 = CurrencyConverterUtils.ConvertToCurrency(data.SaOperatingCostsSection3.FeeWithoutBadDebtProvision),
            BadDebtProvision3                        = CurrencyConverterUtils.ConvertToCurrency(data.SaOperatingCostsSection3.BadDebtProvision),
            SaOperatingCostsWithBadDebtProvision3    = CurrencyConverterUtils.ConvertToCurrency(data.SaOperatingCostsSection3.FeeWithBadDebtProvision.Total),

            LaDataPrepCostsWithoutBadDebtProvision4 = CurrencyConverterUtils.ConvertToCurrency(data.LaDataPrepSection4.FeeWithoutBadDebtProvision),
            BadDebtProvision4                       = CurrencyConverterUtils.ConvertToCurrency(data.LaDataPrepSection4.BadDebtProvision),
            LaDataPrepCostsWithbadDebtProvision4    = CurrencyConverterUtils.ConvertToCurrency(data.LaDataPrepSection4.FeeWithBadDebtProvision.Total),

            OneOffFeeSaSetupCostsWithoutBadDebtProvision5 = CurrencyConverterUtils.ConvertToCurrency(data.SaSetupCostsSection5.FeeWithoutBadDebtProvision),
            BadDebtProvision5                             = CurrencyConverterUtils.ConvertToCurrency(data.SaSetupCostsSection5.BadDebtProvision),
            OneOffFeeSaSetupCostsWithBadDebtProvision5    = CurrencyConverterUtils.ConvertToCurrency(data.SaSetupCostsSection5.FeeWithBadDebtProvision.Total)
        };
    }

    private static List<CalcSummaryProducerCalculationResults> ArrangeProducerCalculationResult(
        BillingRunContext runContext,
        CalcResult calcResult,
        IImmutableList<MaterialDetail> materials)
    {
        var results = new List<CalcSummaryProducerCalculationResults>();

        var filteredProducers = calcResult.CalcResultSummary.ProducerDisposalFees
            .Where(producer => runContext.AcceptedProducerIds.Contains(producer.ProducerId)
                               && !string.IsNullOrWhiteSpace(producer.Level));

        foreach (var producer in filteredProducers)
        {
            results.Add(CalcSummaryProducerCalculationResults.From(producer, materials, runContext.RequiresModulation));
        }

        return results;
    }

    private static CalcResultProducerCalculationResultsTotal? ArrangeProducerCalculationResultsTotal(CalcResultSummary calcResultSummary)
    {
        return CalcResultProducerCalculationResultsTotal.From(calcResultSummary);
    }
}
