using EPR.Calculator.Service.Function.Mappers;
using EPR.Calculator.Service.Function.Models;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
