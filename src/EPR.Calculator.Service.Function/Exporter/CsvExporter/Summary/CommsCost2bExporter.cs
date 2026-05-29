using System.Text;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;

public class CommsCost2bExporter : ICalcResultSummaryPartExporter
{
    public IEnumerable<CalcResultSummaryHeader> GetColumnHeaders(IReadOnlyList<MaterialDetail> materials, bool applyModulation)
        => CalcResultSummaryUtil.CommsCost2b();

    public void AppendSectionHeader(StringBuilder csvContent, CalcResultSummary resultSummary, IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        int count = GetColumnHeaders(materials, applyModulation).Count();
        csvContent.Append(CsvSanitiser.SanitiseData(CalcResultSummaryHeaders.CommsCostHeaderWithoutBadDebtFor2bTitle));
        csvContent.Append(CsvSanitiser.SanitiseData(CalcResultSummaryHeaders.CommsCostHeaderBadDebtProvisionFor2bTitle));
        csvContent.Append(CsvSanitiser.SanitiseData(CalcResultSummaryHeaders.CommsCostHeaderWithBadDebtFor2bTitle));
        csvContent.Append(',', count - 3);
    }

    public void AppendGroupHeader(StringBuilder csvContent, CalcResultSummary resultSummary, IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        int count = GetColumnHeaders(materials, applyModulation).Count();
        csvContent.Append(CsvSanitiser.SanitiseData($"£{Math.Round(resultSummary.CommsCostHeaderWithoutBadDebtFor2bTitle, 2)}"));
        csvContent.Append(CsvSanitiser.SanitiseData($"£{Math.Round(resultSummary.CommsCostHeaderBadDebtProvisionFor2bTitle, 2)}"));
        csvContent.Append(CsvSanitiser.SanitiseData($"£{Math.Round(resultSummary.CommsCostHeaderWithBadDebtFor2bTitle, 2)}"));
        csvContent.Append(',', count - 3);
    }

    public void AppendRow(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer, bool applyModulation)
    {
        var costs = producer.CommunicationCostsSectionTwoB;
        csvContent.Append(CsvSanitiser.SanitiseData(costs?.TotalProducerFeeWithoutBadDebtProvision, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(costs?.BadDebtProvision, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(costs?.TotalProducerFeeWithBadDebtProvision, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(costs?.EnglandTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(costs?.WalesTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(costs?.ScotlandTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(costs?.NorthernIrelandTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
    }
}
