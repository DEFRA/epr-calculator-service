using System.Text;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;

public interface ISection2aCommsExporter : ICalcResultSummaryPartExporter { }

public class Section2aCommsExporter : ISection2aCommsExporter
{
    public IEnumerable<CalcResultSummaryHeader> GetColumnHeaders(IReadOnlyList<MaterialDetail> materials, bool applyModulation)
        => CalcResultSummaryUtil.Section2aComms();

    public void AppendRow(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer, bool applyModulation)
    {
        csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerCommsFee, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.BadDebtProvisionComms, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerCommsFeeWithBadDebtProvision, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.EnglandTotalComms, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.WalesTotalComms, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.ScotlandTotalComms, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.NorthernIrelandTotalComms, DecimalPlaces.Two, null, true));
    }
}
