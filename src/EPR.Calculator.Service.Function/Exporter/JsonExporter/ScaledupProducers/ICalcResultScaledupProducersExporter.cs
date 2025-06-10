using System.Collections.Generic;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.ScaledupProducers
{
    public interface ICalcResultScaledupProducersExporter
    {
        public string Export(CalcResultScaledupProducers calcResultScaledupProducers, IEnumerable<int> acceptedProducerIds);
    }
}
