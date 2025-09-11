using System;

namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultRejectedProducer
    {
        public int ProducerId { get; set; }

        public string ProducerName { get; set; }

        public string TradingName { get; set; }

        public string SuggestedBillingInstruction { get; set; }

        public decimal SuggestedInvoiceAmount { get; set; }

        public DateTime? InstructionConfirmedDate { get; set; }

        public string InstructionConfirmedBy { get; set; }

        public string ReasonForRejection { get; set; }
    }
}
