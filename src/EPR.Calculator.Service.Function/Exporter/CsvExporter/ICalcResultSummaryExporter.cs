using EPR.Calculator.Service.Function.Models;
using System.Text;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter
{
    public interface ICalcResultSummaryExporter
    {
        void Export(CalcResultSummary resultSummary, StringBuilder csvContent);
    }
}
