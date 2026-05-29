using System.Text;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;

public interface ISection1DisposalFeeExporter : ICalcResultSummaryPartExporter { }

public class Section1DisposalFeeExporter : ISection1DisposalFeeExporter
{
    public IEnumerable<CalcResultSummaryHeader> GetColumnHeaders(IReadOnlyList<MaterialDetail> materials, bool applyModulation)
        => CalcResultSummaryUtil.Section1DisposalFee();

    public void AppendRow(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer, bool applyModulation)
    {
        csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerDisposalFee, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.BadDebtProvision, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerDisposalFeeWithBadDebtProvision, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.EnglandTotal, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.WalesTotal, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.ScotlandTotal, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.NorthernIrelandTotal, DecimalPlaces.Two, null, true));
        AppendCsvValue(csvContent, producer.TonnageChangeCount, producer.isOverallTotalRow);
        AppendCsvValue(csvContent, producer.TonnageChangeAdvice, producer.isOverallTotalRow);
    }

    private static void AppendCsvValue(StringBuilder csvContent, object? value, bool isOverallTotalRow = false,
                                       DecimalPlaces decimalPlaces = DecimalPlaces.Zero,
                                       DecimalFormats decimalFormat = DecimalFormats.F2)
    {
        if (value == null && !isOverallTotalRow)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.Hyphen));
        }
        else if (value is int or decimal or double)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(value, decimalPlaces, decimalFormat));
        }
        else
        {
            csvContent.Append(CsvSanitiser.SanitiseData(value));
        }
    }
}
