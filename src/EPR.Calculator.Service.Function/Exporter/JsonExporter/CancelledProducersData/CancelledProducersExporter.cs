using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.CancelledProducersData
{
    public class CancelledProducersExporter : ICancelledProducersExporter
    {
        private readonly ICancelledProducersMapper mapper;

        public CancelledProducersExporter(ICancelledProducersMapper mapper)
        {
            this.mapper = mapper;
        }

        public CancelledProducers Export(CalcResultCancelledProducersResponse calcResultCancelledProducersResponse)
        {
            return this.mapper.Map(calcResultCancelledProducersResponse);
        }
    }
}
