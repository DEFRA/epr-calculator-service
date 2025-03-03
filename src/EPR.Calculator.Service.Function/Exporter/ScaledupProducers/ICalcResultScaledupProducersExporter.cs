namespace EPR.Calculator.Service.Function.Exporter.ScaledupProducers
{
    using System.Text;
    using EPR.Calculator.Service.Function.Models;

    public interface ICalcResultScaledupProducersExporter
    {
        public void Export(CalcResultScaledupProducers calcResultScaledupProducers, StringBuilder stringBuilder);
    }
}