using System.Collections.Generic;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.ScaledupProducers
{
    public interface ICalcResultScaledupProducersJsonExporter
    {
        public string Export(CalcResultScaledupProducers calcResultScaledupProducers, IEnumerable<int> acceptedProducerIds);
    }
}
