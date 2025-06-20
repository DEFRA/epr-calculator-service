using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.BillingInstructions
{
    public class CalcResultBillingInstructionsExporter : ICalcResultBillingInstructionsExporter
    {
        private readonly IBillingInstructionsJsonMapper mapper;

        public CalcResultBillingInstructionsExporter(IBillingInstructionsJsonMapper mapper)
        {
            this.mapper = mapper;
        }

        public string Export(CalcResultSummaryProducerDisposalFees fees)
        {
            var result = this.mapper.Map(fees);
            return JsonConvert.SerializeObject(result);
        }
    }
}
