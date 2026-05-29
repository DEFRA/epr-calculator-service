using System.Text;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.Utils;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.Service.Function.Enums;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;

public interface ICalcResultSummaryExporter
{
    void Export(
        RunContext runContext,
        CalcResultSummary resultSummary,
        IImmutableList<MaterialDetail> materials,
        StringBuilder csvContent
    );
}

public class CalcResultSummaryExporter : ICalcResultSummaryExporter
{
    private readonly IReadOnlyList<ICalcResultSummaryPartExporter> partExporters;

    public CalcResultSummaryExporter(IEnumerable<ICalcResultSummaryPartExporter> partExporters)
    {
        this.partExporters = partExporters.ToList();
    }

    public void Export(
        RunContext runContext,
        CalcResultSummary resultSummary,
        IImmutableList<MaterialDetail> materials,
        StringBuilder csvContent
    )
    {
        csvContent.AppendLine();
        csvContent.AppendLine();

        AddSummaryDataHeader(resultSummary, materials, runContext.RequiresModulation, csvContent);

        foreach (var producer in resultSummary.ProducerDisposalFees)
        {
            AddNewRow(csvContent, producer, runContext.RequiresModulation);
        }
    }

    public void AddNewRow(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer, bool applyModulation)
    {
        foreach (var exporter in partExporters)
        {
            exporter.AppendRow(csvContent, producer, applyModulation);
        }
        csvContent.AppendLine();
    }

    public static void WriteSecondaryHeaders(StringBuilder csvContent, IReadOnlyCollection<CalcResultSummaryHeader> headers)
    {
        var maxColumnSize = headers.MaxBy(h => h.ColumnIndex ?? 0)?.ColumnIndex ?? throw new ArgumentException("No headers specified");

        var headerRows = new string[maxColumnSize];
        foreach (var item in headers.Where(h => h.ColumnIndex.HasValue))
        {
            headerRows[item.ColumnIndex!.Value - 1] = CsvSanitiser.SanitiseData(item.Name, false);
        }

        var headerRow = string.Join(CommonConstants.CsvFileDelimiter, headerRows);
        csvContent.AppendLine(headerRow);
    }

    public void WriteColumnHeaders(List<CalcResultSummaryHeader> columnHeaders, StringBuilder csvContent)
    {
        foreach (var item in columnHeaders)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(item.Name));
        }
    }

    private void AddSummaryDataHeader(CalcResultSummary resultSummary, IReadOnlyList<MaterialDetail> materials, bool applyModulation, StringBuilder csvContent)
    {
        var partColumnHeaders = partExporters
            .Select(e => e.GetColumnHeaders(materials, applyModulation).ToList())
            .ToList();

        // The first part exporter (ProducerIdentityExporter) contributes the identity columns;
        // GetSecondaryHeaders needs that count to compute section start indices.
        int identityColumnCount = partColumnHeaders[0].Count;
        var columnHeaders = partColumnHeaders.SelectMany(h => h).ToList();

        var (producerDisposalFeesHeaders, materialBreakdownHeaders) =
            CalcResultSummaryUtil.GetSecondaryHeaders(resultSummary, materials, applyModulation, identityColumnCount);

        csvContent.AppendLine(CsvSanitiser.SanitiseData(CalcResultSummaryHeaders.CalculationResult))
            .AppendLine()
            .AppendLine();

        csvContent.AppendLine(CsvSanitiser.SanitiseData(CalcResultSummaryHeaders.Notes));

        WriteSecondaryHeaders(csvContent, producerDisposalFeesHeaders);
        WriteSecondaryHeaders(csvContent, materialBreakdownHeaders);
        WriteColumnHeaders(columnHeaders, csvContent);

        csvContent.AppendLine();
    }
}
