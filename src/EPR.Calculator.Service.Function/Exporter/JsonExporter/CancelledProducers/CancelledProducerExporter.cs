using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.CancelledProducers
{
    public class CancelledProducerExporter : ICancelledProducersExporter
    {
        private readonly ICancelledProducerMapper mapper;

        public CancelledProducerExporter(ICancelledProducerMapper mapper)
        {
            this.mapper = mapper;
        }

        public string Export(CalcResultCancelledProducersResponse calcResultCancelledProducersResponse)
        {
            var result = this.mapper.Map(calcResultCancelledProducersResponse);
            return JsonConvert.SerializeObject(result);
        }
    }
}
