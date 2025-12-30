namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.PartialObligations
{
    using System.Text;
    using EPR.Calculator.Service.Function.Models;

    public interface ICalcResultPartialObligationsExporter
    {
        public void Export(CalcResultPartialObligations calcResultPartialObligations, StringBuilder stringBuilder);
    }
}