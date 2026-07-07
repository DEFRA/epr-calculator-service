using System.Text;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;

public interface IProducerFeesPartExporter
{
    IEnumerable<string> GetColumnHeaders(IReadOnlyList<MaterialDetail> materials, bool applyModulation);

    void AppendSectionHeader(StringBuilder csvContent, ProducerFees producerFees, IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        foreach (var _ in GetColumnHeaders(materials, applyModulation))
            csvContent.Append(',');
    }

    void AppendGroupHeader(StringBuilder csvContent, ProducerFees producerFees, IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        foreach (var _ in GetColumnHeaders(materials, applyModulation))
            csvContent.Append(',');
    }

    void AppendRow(StringBuilder csvContent, ProducerFeeDetail producer, bool applyModulation, bool isOverallTotal);
}
