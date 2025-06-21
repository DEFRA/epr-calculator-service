using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.BillingInstructions
{
    public class CalculationOfSuggestedBillingInstructionsAndInvoiceAmountsExporter : ICalculationOfSuggestedBillingInstructionsAndInvoiceAmountsExporter
    {
        private readonly ICalculationOfSuggestedBillingInstructionsAndInvoiceAmountsMapper mapper;

        public CalculationOfSuggestedBillingInstructionsAndInvoiceAmountsExporter(ICalculationOfSuggestedBillingInstructionsAndInvoiceAmountsMapper mapper)
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
