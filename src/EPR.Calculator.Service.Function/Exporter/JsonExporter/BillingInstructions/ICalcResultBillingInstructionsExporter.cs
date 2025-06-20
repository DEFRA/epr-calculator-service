using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.BillingInstructions
{
    public interface ICalcResultBillingInstructionsExporter
    {
        public string Export(CalcResultSummaryProducerDisposalFees fees);
    }
}
