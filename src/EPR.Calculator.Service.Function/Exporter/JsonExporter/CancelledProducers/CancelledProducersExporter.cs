using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.CancelledProducers
{
    public class CancelledProducersExporter : ICancelledProducersExporter
    {
        private readonly ICancelledProducersMapper mapper;

        public CancelledProducersExporter(ICancelledProducersMapper mapper)
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
