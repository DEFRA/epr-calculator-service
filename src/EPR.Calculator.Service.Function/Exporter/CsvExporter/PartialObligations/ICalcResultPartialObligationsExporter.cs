using System.Text;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.PartialObligations
{
    public interface ICalcResultPartialObligationsExporter
    {
        public void Export(CalcResultPartialObligations calcResultPartialObligations, StringBuilder stringBuilder);
    }
}