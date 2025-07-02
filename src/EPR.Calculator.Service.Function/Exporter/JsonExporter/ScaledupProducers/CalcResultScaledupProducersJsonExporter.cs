using System.Collections.Generic;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.ScaledupProducers
{
    public class CalcResultScaledupProducersJsonExporter : ICalcResultScaledupProducersJsonExporter
    {
        private readonly ICalcResultScaledupProducersJsonMapper mapper;

        public CalcResultScaledupProducersJsonExporter(ICalcResultScaledupProducersJsonMapper mapper)
        {
            this.mapper = mapper;
        }

        public CalcResultScaledupProducersJson Export(
            CalcResultScaledupProducers calcResultScaledupProducers,
            IEnumerable<int> acceptedProducerIds,
            List<MaterialDetail> materials)
        {
            return this.mapper.Map(calcResultScaledupProducers, acceptedProducerIds, materials);
        }
    }
}
