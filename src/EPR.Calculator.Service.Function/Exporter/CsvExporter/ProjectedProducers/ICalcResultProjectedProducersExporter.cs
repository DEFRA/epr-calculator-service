using System.Text;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.ProjectedProducers
{
    public interface ICalcResultProjectedProducersExporter
    {
        public void Export(CalcResultProjectedProducers calcResultProjectedProducers, StringBuilder stringBuilder);
    }
}