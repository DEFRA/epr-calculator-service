using System.Text;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;

public interface ICommsCost2cExporter : ICalcResultSummaryPartExporter { }

public class CommsCost2cExporter : ICommsCost2cExporter
{
    public IEnumerable<CalcResultSummaryHeader> GetColumnHeaders(IReadOnlyList<MaterialDetail> materials, bool applyModulation)
        => CalcResultSummaryUtil.CommsCost2c();

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
