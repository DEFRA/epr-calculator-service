using System.Text;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Builder.Summary.TwoCCommsCost;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;

public class CommsCost2cExporter : ICalcResultSummaryPartExporter
{
    public IEnumerable<CalcResultSummaryHeader> GetColumnHeaders(IReadOnlyList<MaterialDetail> materials, bool applyModulation)
        => CalcResultSummaryUtil.CommsCost2c();

    public void AppendSectionHeader(StringBuilder csvContent, CalcResultSummary resultSummary, IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        int count = GetColumnHeaders(materials, applyModulation).Count();
        csvContent.Append(CsvSanitiser.SanitiseData(TwoCCommsConstantsHeader.TwoCCommsCostByCountryWithout));
        csvContent.Append(CsvSanitiser.SanitiseData(TwoCCommsConstantsHeader.TwoCCommsCostBadBebtProvision));
        csvContent.Append(CsvSanitiser.SanitiseData(TwoCCommsConstantsHeader.TwoCCommsCostByCountryWithBadDebt));
        csvContent.Append(',', count - 3);
    }

    public void AppendGroupHeader(StringBuilder csvContent, CalcResultSummary resultSummary, IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        int count = GetColumnHeaders(materials, applyModulation).Count();
        csvContent.Append(CsvSanitiser.SanitiseData($"£{Math.Round(resultSummary.TwoCCommsCostsByCountryWithoutBadDebtProvision, 2)}"));
        csvContent.Append(CsvSanitiser.SanitiseData($"£{Math.Round(resultSummary.TwoCBadDebtProvision, 2)}"));
        csvContent.Append(CsvSanitiser.SanitiseData($"£{Math.Round(resultSummary.TwoCCommsCostsByCountryWithBadDebtProvision, 2)}"));
        csvContent.Append(',', count - 3);
    }

    public void AppendRow(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer, bool applyModulation)
    {
        csvContent.Append(CsvSanitiser.SanitiseData(producer.TwoCTotalProducerFeeForCommsCostsWithoutBadDebt, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.TwoCBadDebtProvision, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.TwoCTotalProducerFeeForCommsCostsWithBadDebt, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.TwoCEnglandTotalWithBadDebt, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.TwoCWalesTotalWithBadDebt, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.TwoCScotlandTotalWithBadDebt, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.TwoCNorthernIrelandTotalWithBadDebt, DecimalPlaces.Two, null, true));
    }
}
