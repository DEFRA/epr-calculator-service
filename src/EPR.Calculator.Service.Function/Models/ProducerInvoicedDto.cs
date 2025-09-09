using EPR.Calculator.API.Data.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Models
{
    public class ProducerInvoicedDto
    {
        public ProducerInvoicedMaterialNetTonnage? InvoicedTonnage { get; set; }
        public CalculatorRun? CalculatorRun { get; set; }
        public ProducerDesignatedRunInvoiceInstruction? InvoiceInstruction { get; set; }
        public ProducerDetail? ProducerDetail { get; set; }
        public ProducerResultFileSuggestedBillingInstruction? ResultFileSuggestedBillingInstruction { get; set; }
    }
}
