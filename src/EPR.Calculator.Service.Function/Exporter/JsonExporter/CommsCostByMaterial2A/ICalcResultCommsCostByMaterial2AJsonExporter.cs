using System.Collections.Generic;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.CommsCostByMaterial2A
{
    public interface ICalcResultCommsCostByMaterial2AJsonExporter
    {
        public string Export(Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostByMaterial);
    }
}
