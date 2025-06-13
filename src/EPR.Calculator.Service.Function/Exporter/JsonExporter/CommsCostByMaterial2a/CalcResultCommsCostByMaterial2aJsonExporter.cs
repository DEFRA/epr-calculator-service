using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.CommsCostByMaterial2a
{
    public class CalcResultCommsCostByMaterial2aJsonExporter : ICalcResultCommsCostByMaterial2aJsonExporter
    {
        private readonly ICalcResultCommsCostByMaterial2aJsonMapper mapper;

        public CalcResultCommsCostByMaterial2aJsonExporter(ICalcResultCommsCostByMaterial2aJsonMapper mapper)
        {
            this.mapper = mapper;
        }

        public string Export(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
        {
            var result = this.mapper.Map(calcResultSummaryProducerDisposalFees);
            return JsonConvert.SerializeObject(result);
        }
    }
}
