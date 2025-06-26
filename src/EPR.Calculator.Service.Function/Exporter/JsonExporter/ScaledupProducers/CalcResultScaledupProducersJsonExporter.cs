using System.Collections.Generic;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.ScaledupProducers
{
    public class CalcResultScaledupProducersJsonExporter : ICalcResultScaledupProducersJsonExporter
    {
        private readonly ICalcResultScaledupProducersJsonMapper mapper;

        public CalcResultScaledupProducersJsonExporter(ICalcResultScaledupProducersJsonMapper mapper)
        {
            this.mapper = mapper;
        }

        public CalcResultScaledupProducersJson Export(CalcResultScaledupProducers calcResultScaledupProducers, IEnumerable<int> acceptedProducerIds)
        {
            return this.mapper.Map(calcResultScaledupProducers, acceptedProducerIds);
        }
    }
}
