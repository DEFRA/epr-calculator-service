using System;

namespace EPR.Calculator.Service.Function.Models
{
    public class BillingInstruction
    {
        public int Id { get; set; }

        public int CalculatorRunId { get; set; }

        public int ProducerId { get; set; }

        public decimal TotalProducerBillWithBadDebt { get; set; }

        public decimal? CurrentYearInvoiceTotalToDate { get; set; }

        public string? TonnageChangeSinceLastInvoice { get; set; }

        public decimal? AmountLiabilityDifferenceCalcVsPrev { get; set; }

        public string? MaterialPoundThresholdBreached { get; set; }

        public string? TonnagePoundThresholdBreached { get; set; }

        public decimal? PercentageLiabilityDifferenceCalcVsPrev { get; set; }

        public string? MaterialPercentageThresholdBreached { get; set; }

        public string? TonnagePercentageThresholdBreached { get; set; }

        public required string SuggestedBillingInstruction { get; set; }

        public decimal SuggestedInvoiceAmount { get; set; }

        public string? BillingInstructionAcceptReject { get; set; }

        public string? ReasonForRejection { get; set; }

        public string? LastModifiedAcceptRejectBy { get; set; }

        public DateTime? LastModifiedAcceptReject { get; set; }
    }
}
