using System.Collections.Generic;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.ScaledupProducers
{
    public interface ICalcResultScaledupProducersJsonExporter
    {
        public CalcResultScaledupProducersJson Export(
            CalcResultScaledupProducers calcResultScaledupProducers,
            IEnumerable<int> acceptedProducerIds,
            List<MaterialDetail> materials);
    }
}
