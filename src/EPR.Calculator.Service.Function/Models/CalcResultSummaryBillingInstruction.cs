namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultSummaryBillingInstruction
    {
        public decimal? CurrentYearInvoiceTotalToDate { get; set; }

        public string? TonnageChangeSinceLastInvoice { get; set; }

        public decimal? LiabilityDifference { get; set; }

        public string? MaterialThresholdBreached { get; set; }

        public string? TonnageThresholdBreached { get; set; }

        public string? PercentageLiabilityDifference { get; set; }

        public string? MaterialPercentageThresholdBreached { get; set; }

        public string? TonnagePercentageThresholdBreached { get; set; }

        public string? SuggestedBillingInstruction { get; set; }

        public string? SuggestedInvoiceAmount { get; set; }
    }
}
