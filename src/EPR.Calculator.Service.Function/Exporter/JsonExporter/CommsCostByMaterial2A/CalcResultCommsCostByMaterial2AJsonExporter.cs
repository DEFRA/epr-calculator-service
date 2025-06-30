using System.Collections.Generic;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.CommsCostByMaterial2A
{
    public class CalcResultCommsCostByMaterial2AJsonExporter : ICalcResultCommsCostByMaterial2AJsonExporter
    {
        private readonly ICalcResultCommsCostByMaterial2AJsonMapper mapper;

        public CalcResultCommsCostByMaterial2AJsonExporter(ICalcResultCommsCostByMaterial2AJsonMapper mapper)
        {
            this.mapper = mapper;
        }

        public string Export(Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostByMaterial, List<MaterialDetail> materials)
        {
            var result = this.mapper.Map(commsCostByMaterial, materials);
            return JsonConvert.SerializeObject(result);
        }
    }
}
