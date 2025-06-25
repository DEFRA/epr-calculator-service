namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.CancelledProducers
{
    using System.Text;
    using EPR.Calculator.Service.Function.Models;

    public interface ICalcResultCancelledProducersExporter
    {
        public void Export(CalcResultCancelledProducersResponse calcResultCancelledProducers, StringBuilder csvContent);
    }
}