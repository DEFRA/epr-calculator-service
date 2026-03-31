namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.ProjectedProducers
{
    using System.Text;
    using EPR.Calculator.Service.Function.Models;

    public interface ICalcResultProjectedProducersExporter
    {
        public void Export(CalcResultProjectedProducers calcResultProjectedProducers, StringBuilder stringBuilder);
    }
}