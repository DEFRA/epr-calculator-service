using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.Models
{
    public class ProducerInvoicedDto
    {
        public ProducerInvoicedMaterialNetTonnage? InvoicedTonnage { get; set; }
        public int CalculatorRunId { get; set; }
        public string? CalculatorName { get; set; }
        public ProducerDesignatedRunInvoiceInstruction? InvoiceInstruction { get; set; }
        public ProducerDetail? ProducerDetail { get; set; }
        public ProducerResultFileSuggestedBillingInstruction? ResultFileSuggestedBillingInstruction { get; set; }
    }
}
