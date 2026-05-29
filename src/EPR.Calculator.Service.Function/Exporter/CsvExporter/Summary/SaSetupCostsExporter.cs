using System.Text;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Builder.Summary.SaSetupCosts;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;

public class SaSetupCostsExporter : ICalcResultSummaryPartExporter
{
    public IEnumerable<CalcResultSummaryHeader> GetColumnHeaders(IReadOnlyList<MaterialDetail> materials, bool applyModulation)
        => SaSetupCostsProducer.GetHeaders();

    public void AppendSectionHeader(StringBuilder csvContent, CalcResultSummary resultSummary, IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        int count = GetColumnHeaders(materials, applyModulation).Count();
        csvContent.Append(CsvSanitiser.SanitiseData(SaSetupCostsHeaders.OneOffFeeSetupCostsWithoutBadDebtProvisionTitle));
        csvContent.Append(CsvSanitiser.SanitiseData(SaSetupCostsHeaders.BadDebtProvisionTitle));
        csvContent.Append(CsvSanitiser.SanitiseData(SaSetupCostsHeaders.OneOffFeeSetupCostsWithBadDebtProvisionTitle));
        csvContent.Append(',', count - 3);
    }

    public void AppendGroupHeader(StringBuilder csvContent, CalcResultSummary resultSummary, IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        int count = GetColumnHeaders(materials, applyModulation).Count();
        csvContent.Append(CsvSanitiser.SanitiseData($"£{Math.Round(resultSummary.SaSetupCostsTitleSection5, 2)}"));
        csvContent.Append(CsvSanitiser.SanitiseData($"£{Math.Round(resultSummary.SaSetupCostsBadDebtProvisionTitleSection5, 2)}"));
        csvContent.Append(CsvSanitiser.SanitiseData($"£{Math.Round(resultSummary.SaSetupCostsWithBadDebtProvisionTitleSection5, 2)}"));
        csvContent.Append(',', count - 3);
    }

    public void AppendRow(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer, bool applyModulation)
    {
        var costs = producer.OneOffSchemeAdministrationSetupCosts;
        csvContent.Append(CsvSanitiser.SanitiseData(costs?.TotalProducerFeeWithoutBadDebtProvision, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(costs?.BadDebtProvision, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(costs?.TotalProducerFeeWithBadDebtProvision, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(costs?.EnglandTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(costs?.WalesTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(costs?.ScotlandTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(costs?.NorthernIrelandTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
    }
}
