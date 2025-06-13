namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.ScaledupProducers
{
    using System.Text;
    using EPR.Calculator.Service.Function.Models;

    public interface ICalcResultCancelledProducersExporter
    {
        public void Export(CalcResultCancelledProducersResponse calcResultCancelledProducers, StringBuilder stringBuilder);
    }
}