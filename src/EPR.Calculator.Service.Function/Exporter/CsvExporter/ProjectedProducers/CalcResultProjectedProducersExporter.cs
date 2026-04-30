using System.Text;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.ProjectedProducers
{
    

    public class CalcResultProjectedProducersExporter : ICalcResultProjectedProducersExporter
    {
        public void Export(CalcResultProjectedProducers calcResultProjectedProducers, StringBuilder stringBuilder)
        {
            // Add empty lines
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();

            // Add H2 headers
            PrepareProjectedProducersHeaders(calcResultProjectedProducers.H2ProjectedProducersHeaders!, stringBuilder);

            // Add H2 data
            if (calcResultProjectedProducers.H2ProjectedProducers?.Any() == true)
            {
                H2ProjectedProducersExporterUtils.AppendProjectedProducers(calcResultProjectedProducers.H2ProjectedProducers!, stringBuilder);
            }
            else
            {
                stringBuilder.AppendLine(CsvSanitiser.SanitiseData(CalcResultProjectedProducersHeaders.NoProjectedProducers));
            }

            // Add empty lines
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();

            // Add H1 headers
            PrepareProjectedProducersHeaders(calcResultProjectedProducers.H1ProjectedProducersHeaders!, stringBuilder);

            // Add H1 data
            if (calcResultProjectedProducers.H1ProjectedProducers?.Any() == true)
            {
                H1ProjectedProducersExporterUtils.AppendProjectedProducers(calcResultProjectedProducers.H1ProjectedProducers!, stringBuilder);
            }
            else
            {
                stringBuilder.AppendLine(CsvSanitiser.SanitiseData(CalcResultProjectedProducersHeaders.NoProjectedProducers));
            }
        }

        private void PrepareProjectedProducersHeaders(ProjectedProducersHeaders headers, StringBuilder csvContent)
        {
            // Add projected producers headers
            csvContent.AppendLine(CsvSanitiser.SanitiseData(headers.TitleHeader!.Name));
            csvContent.AppendLine();

            // Add material breakdown headers
            WriteProjectedProducersSecondaryHeaders(headers.MaterialBreakdownHeaders!, csvContent);

            // Add column headers
            WriteProjectedProducersColumnHeaders(headers.ColumnHeaders!, csvContent);
            csvContent.AppendLine();
        }

        private void WriteProjectedProducersSecondaryHeaders(IEnumerable<ProjectedProducersHeader> headers, StringBuilder csvContent)
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

        private void WriteProjectedProducersColumnHeaders(IEnumerable<ProjectedProducersHeader> columnHeaders, StringBuilder csvContent)
        {
            foreach (var item in columnHeaders)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(item.Name));
            }
        }
    }
}