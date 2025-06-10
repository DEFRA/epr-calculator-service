using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.ScaledupProducers
{
    public class CalcResultScaledupProducersExporter : ICalcResultScaledupProducersExporter
    {
        private ICalcResultScaledupProducersJsonMapper mapper;

        public CalcResultScaledupProducersExporter(ICalcResultScaledupProducersJsonMapper mapper)
        {
            this.mapper = mapper;
        }

        public string Export(CalcResultScaledupProducers calcResultScaledupProducers)
        {
            var result = this.mapper.Map(calcResultScaledupProducers);
            return JsonConvert.SerializeObject(result);
        }
    }
}
