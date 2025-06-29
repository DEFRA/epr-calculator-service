using System.Collections.Generic;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Mapper
{
    public interface ICalcResultCommsCostByMaterial2AJsonMapper
    {
        public CalcResultCommsCostByMaterial2AJson Map(
            Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostByMaterial,
            List<MaterialDetail> materials);
    }
}
