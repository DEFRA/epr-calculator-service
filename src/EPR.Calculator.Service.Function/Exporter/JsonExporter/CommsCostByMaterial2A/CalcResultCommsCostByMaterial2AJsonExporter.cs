using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;

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
            return JsonSerializer.Serialize(result);
        }
    }
}
