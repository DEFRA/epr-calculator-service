using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class CancelledProducerMapper : ICancelledProducerMapper
    {
        public CancelledProducers Map(CalcResultCancelledProducersResponse calcResultCancelledProducersResponse)
        {
            return new CancelledProducers();
        }
    }
}
