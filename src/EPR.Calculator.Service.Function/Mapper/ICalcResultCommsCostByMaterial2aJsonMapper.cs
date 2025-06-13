using System.Collections.Generic;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Mapper
{
    public interface ICalcResultCommsCostByMaterial2aJsonMapper
    {
        public CalcResultCommsCostByMaterial2aJson Map(Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostByMaterial);
    }
}
