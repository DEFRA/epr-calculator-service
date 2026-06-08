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
            FeeForLaDisposalCostsWithoutBadDebtprovision1 = CurrencyConverterUtils.ConvertToCurrency(data.LocalAuthorityDisposalCostsSectionOne.FeeWithoutBadDebtProvision),
            BadDebtProvision1 = CurrencyConverterUtils.ConvertToCurrency(data.LocalAuthorityDisposalCostsSectionOne.BadDebtProvision),
            FeeForLaDisposalCostsWithBadDebtprovision1 = CurrencyConverterUtils.ConvertToCurrency(data.LocalAuthorityDisposalCostsSectionOne.FeeWithBadDebtProvision.Total),

            FeeForCommsCostsByMaterialWithoutBadDebtprovision2a = CurrencyConverterUtils.ConvertToCurrency(data.CommsCostsSectionTwoA.FeeWithoutBadDebtProvision),
            BadDebtProvision2a = CurrencyConverterUtils.ConvertToCurrency(data.CommsCostsSectionTwoA.BadDebtProvision),
            FeeForCommsCostsByMaterialWitBadDebtprovision2a = CurrencyConverterUtils.ConvertToCurrency(data.CommsCostsSectionTwoA.FeeWithBadDebtProvision.Total),

            FeeForCommsCostsUkWideWithoutBadDebtprovision2b = CurrencyConverterUtils.ConvertToCurrency(data.CommsCostsHeaderFor2bTitle.FeeWithoutBadDebtProvision),
            BadDebtProvision2b = CurrencyConverterUtils.ConvertToCurrency(data.CommsCostsHeaderFor2bTitle.BadDebtProvision),
            FeeForCommsCostsUkWideWithBadDebtprovision2b = CurrencyConverterUtils.ConvertToCurrency(data.CommsCostsHeaderFor2bTitle.FeeWithBadDebtProvision.Total),

            FeeForCommsCostsByCountryWithoutBadDebtprovision2c = CurrencyConverterUtils.ConvertToCurrency(data.TwoCCommsCosts.FeeWithoutBadDebtProvision),
            BadDebtProvision2c = CurrencyConverterUtils.ConvertToCurrency(data.TwoCCommsCosts.BadDebtProvision),
            FeeForCommsCostsByCountryWideWithBadDebtprovision2c = CurrencyConverterUtils.ConvertToCurrency(data.TwoCCommsCosts.FeeWithBadDebtProvision.Total),

            Total12a2b2cWithBadDebt = CurrencyConverterUtils.ConvertToCurrency(data.TotalOnePlus2A2B2CFeeWithBadDebtProvision),

            SaOperatingCostsWithoutBadDebtProvision3 = CurrencyConverterUtils.ConvertToCurrency(data.SchemeAdministratorOperatingCosts.FeeWithoutBadDebtProvision),
            BadDebtProvision3 = CurrencyConverterUtils.ConvertToCurrency(data.SchemeAdministratorOperatingCosts.BadDebtProvision),
            SaOperatingCostsWithBadDebtProvision3 = CurrencyConverterUtils.ConvertToCurrency(data.SchemeAdministratorOperatingCosts.FeeWithBadDebtProvision.Total),

            LaDataPrepCostsWithoutBadDebtProvision4 = CurrencyConverterUtils.ConvertToCurrency(data.LaDataPrepSection4.FeeWithoutBadDebtProvision),
            BadDebtProvision4 = CurrencyConverterUtils.ConvertToCurrency(data.LaDataPrepSection4.BadDebtProvision),
            LaDataPrepCostsWithbadDebtProvision4 = CurrencyConverterUtils.ConvertToCurrency(data.LaDataPrepSection4.FeeWithBadDebtProvision.Total),

            OneOffFeeSaSetupCostsWithoutBadDebtProvision5 = CurrencyConverterUtils.ConvertToCurrency(data.SaSetupCostsSection5.FeeWithoutBadDebtProvision),
            BadDebtProvision5 = CurrencyConverterUtils.ConvertToCurrency(data.SaSetupCostsSection5.BadDebtProvision),
            OneOffFeeSaSetupCostsWithBadDebtProvision5 = CurrencyConverterUtils.ConvertToCurrency(data.SaSetupCostsSection5.FeeWithBadDebtProvision.Total)
        };
    }

    private static List<CalcSummaryProducerCalculationResults> ArrangeProducerCalculationResult(
        BillingRunContext runContext,
        CalcResult calcResult,
        IImmutableList<MaterialDetail> materials)
    {
        var results = new List<CalcSummaryProducerCalculationResults>();

        var filteredProducers = calcResult.CalcResultSummary.ProducerDisposalFees
            .Where(producer => runContext.AcceptedProducerIds.Contains(producer.ProducerIdInt)
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
