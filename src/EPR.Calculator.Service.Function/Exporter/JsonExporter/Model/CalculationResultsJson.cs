using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Features.BillingRuns.Contexts;
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
            FeeForLaDisposalCostsWithoutBadDebtprovision1 = FormatUtils.FormatCurrency(data.LADisposalCostsSection1.FeeWithoutBadDebtProvision),
            BadDebtProvision1                             = FormatUtils.FormatCurrency(data.LADisposalCostsSection1.BadDebtProvision),
            FeeForLaDisposalCostsWithBadDebtprovision1    = FormatUtils.FormatCurrency(data.LADisposalCostsSection1.FeeWithBadDebtProvision.Total),

            FeeForCommsCostsByMaterialWithoutBadDebtprovision2a = FormatUtils.FormatCurrency(data.CommsCostsSection2a.FeeWithoutBadDebtProvision),
            BadDebtProvision2a                                  = FormatUtils.FormatCurrency(data.CommsCostsSection2a.BadDebtProvision),
            FeeForCommsCostsByMaterialWitBadDebtprovision2a     = FormatUtils.FormatCurrency(data.CommsCostsSection2a.FeeWithBadDebtProvision.Total),

            FeeForCommsCostsUkWideWithoutBadDebtprovision2b = FormatUtils.FormatCurrency(data.CommsCostsSection2b.FeeWithoutBadDebtProvision),
            BadDebtProvision2b                              = FormatUtils.FormatCurrency(data.CommsCostsSection2b.BadDebtProvision),
            FeeForCommsCostsUkWideWithBadDebtprovision2b    = FormatUtils.FormatCurrency(data.CommsCostsSection2b.FeeWithBadDebtProvision.Total),

            FeeForCommsCostsByCountryWithoutBadDebtprovision2c  = FormatUtils.FormatCurrency(data.CommsCostsSection2c.FeeWithoutBadDebtProvision),
            BadDebtProvision2c                                  = FormatUtils.FormatCurrency(data.CommsCostsSection2c.BadDebtProvision),
            FeeForCommsCostsByCountryWideWithBadDebtprovision2c = FormatUtils.FormatCurrency(data.CommsCostsSection2c.FeeWithBadDebtProvision.Total),

            Total12a2b2cWithBadDebt = FormatUtils.FormatCurrency(data.TotalOnePlus2A2B2CFeeWithBadDebtProvision),

            SaOperatingCostsWithoutBadDebtProvision3 = FormatUtils.FormatCurrency(data.SaOperatingCostsSection3.FeeWithoutBadDebtProvision),
            BadDebtProvision3                        = FormatUtils.FormatCurrency(data.SaOperatingCostsSection3.BadDebtProvision),
            SaOperatingCostsWithBadDebtProvision3    = FormatUtils.FormatCurrency(data.SaOperatingCostsSection3.FeeWithBadDebtProvision.Total),

            LaDataPrepCostsWithoutBadDebtProvision4 = FormatUtils.FormatCurrency(data.LaDataPrepSection4.FeeWithoutBadDebtProvision),
            BadDebtProvision4                       = FormatUtils.FormatCurrency(data.LaDataPrepSection4.BadDebtProvision),
            LaDataPrepCostsWithbadDebtProvision4    = FormatUtils.FormatCurrency(data.LaDataPrepSection4.FeeWithBadDebtProvision.Total),

            OneOffFeeSaSetupCostsWithoutBadDebtProvision5 = FormatUtils.FormatCurrency(data.SaSetupCostsSection5.FeeWithoutBadDebtProvision),
            BadDebtProvision5                             = FormatUtils.FormatCurrency(data.SaSetupCostsSection5.BadDebtProvision),
            OneOffFeeSaSetupCostsWithBadDebtProvision5    = FormatUtils.FormatCurrency(data.SaSetupCostsSection5.FeeWithBadDebtProvision.Total)
        };
    }

    private static List<CalcSummaryProducerCalculationResults> ArrangeProducerCalculationResult(
        BillingRunContext runContext,
        CalcResult calcResult,
        IImmutableList<MaterialDetail> materials)
    {
        var results = new List<CalcSummaryProducerCalculationResults>();

        var filteredProducers = calcResult.CalcResultSummary.ProducerDisposalFees
            .Where(producer => runContext.AcceptedProducerIds.Contains(producer.ProducerId));

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
