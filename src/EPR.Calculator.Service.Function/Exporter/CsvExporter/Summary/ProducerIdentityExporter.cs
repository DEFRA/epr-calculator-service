using System.Text;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;

public class ProducerIdentityExporter : ICalcResultSummaryPartExporter
{
    public IEnumerable<CalcResultSummaryHeader> GetColumnHeaders(IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        return
        [
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ProducerId },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.SubsidiaryId },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ProducerOrSubsidiaryName },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TradingName },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.Level },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ScaledupTonnages },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.PartialCalculation },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.StatusCode },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.JoinersDate },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.LeaversDate },
        ];
    }

    public void AppendRow(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer, bool applyModulation)
    {
        csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerId));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.SubsidiaryId));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerName));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.TradingName));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.Level));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.IsProducerScaledup));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.IsPartialObligation));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.StatusCode));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.JoinerDate));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.LeaverDate));
    }
}
