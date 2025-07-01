using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.CancelledProducersData
{
    public interface ICancelledProducersExporter
    {
        public CancelledProducers Export(CalcResultCancelledProducersResponse calcResultCancelledProducersResponse);
    }
}
