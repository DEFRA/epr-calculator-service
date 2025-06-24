using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.BillingInstructions
{
    public interface ICalculationOfSuggestedBillingInstructionsAndInvoiceAmountsExporter
    {
        public string Export(CalcResultSummaryProducerDisposalFees fees);
    }
}
