using System.Collections.Generic;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Mapper
{
    public interface ICalcResultScaledupProducersJsonMapper
    {
        public CalcResultScaledupProducersJson Map(
            CalcResultScaledupProducers calcResultScaledupProducers,
            IEnumerable<int> acceptedProducerIds,
            List<MaterialDetail> materials);
    }
}
