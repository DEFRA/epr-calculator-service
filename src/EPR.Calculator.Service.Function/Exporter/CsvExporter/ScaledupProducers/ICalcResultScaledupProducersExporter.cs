using System.Text;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.ScaledupProducers
{
    public interface ICalcResultScaledupProducersExporter
    {
        public void Export(CalcResultScaledupProducers calcResultScaledupProducers, StringBuilder stringBuilder);
    }
}
