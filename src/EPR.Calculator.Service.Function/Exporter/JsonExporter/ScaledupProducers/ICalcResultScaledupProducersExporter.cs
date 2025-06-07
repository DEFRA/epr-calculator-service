namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.ScaledupProducers
{
    using System.Text;
    using EPR.Calculator.Service.Function.Models;

    public interface ICalcResultScaledupProducersExporter
    {
        public string Export(CalcResultScaledupProducers calcResultScaledupProducers, StringBuilder stringBuilder);
    }
}
