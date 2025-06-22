using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.CancelledProducers
{
    public interface ICancelledProducersExporter
    {
        public string Export(CalcResultCancelledProducersResponse calcResultCancelledProducersResponse);
    }
}
