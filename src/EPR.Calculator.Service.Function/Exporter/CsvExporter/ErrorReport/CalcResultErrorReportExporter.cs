using EPR.Calculator.API.Utils;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.ErrorReport
{
    public class CalcResultErrorReportExporter : ICalcResultErrorReportExporter
    {
        public void Export(IEnumerable<CalcResultErrorReport> errorReport, StringBuilder csvContent)
        {
            csvContent.AppendLine();
            csvContent.AppendLine();

            csvContent.AppendLine(CommonConstants.ErrorReportHeader);
            WriteColumnHeaders(csvContent);

            foreach (CalcResultErrorReport report in errorReport)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(report.ProducerId));
                csvContent.Append(CsvSanitiser.SanitiseData(report.SubsidiaryId));
                csvContent.Append(CsvSanitiser.SanitiseData(report.ProducerName));
                csvContent.Append(CsvSanitiser.SanitiseData(report.TradingName));
                csvContent.Append(CsvSanitiser.SanitiseData(report.LeaverCode));
                csvContent.Append(CsvSanitiser.SanitiseData(report.ErrorCodeText));
            }
        }

        public static void WriteColumnHeaders(StringBuilder csvContent)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.ProducerId));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.SubsidiaryId));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.ProducerSubsidaryName));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.TradingName));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.LeaverCode));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.ErrorCodeText));
        }
    }
}
