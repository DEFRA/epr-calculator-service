namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultSummaryProducerDisposalFees
    {
        public required string ProducerId { get; set; }

        public required string SubsidiaryId { get; set; }

        public required string ProducerName { get; set; }

        public string? TradingName { get; set; }

        public string? Level { get; set; }

        public string? StatusCode { get; set; }

        public string IsProducerScaledup { get; set; } = "No";

        public string IsPartialObligation { get; set; } = "No";

        public string? JoinerDate { get; set; }

        public string? LeaverDate { get; set; }

        public bool isTotalRow { get; set; }

        public CalcResultSummaryBadDebtProvision LADisposalCostsSection1 { get; set; } = CalcResultSummaryBadDebtProvision.Empty;

        public CalcResultSummaryBadDebtProvision CommsCostsSection2a { get; set; } = CalcResultSummaryBadDebtProvision.Empty;

        public CalcResultSummaryBadDebtProvision CommsCostsSection2b { get; set; } = CalcResultSummaryBadDebtProvision.Empty;

        public CalcResultSummaryBadDebtProvision CommsCostsSection2c { get; set; } = CalcResultSummaryBadDebtProvision.Empty;

        public CalcResultSummaryBadDebtProvision SaOperatingCostsSection3 { get; set; } = CalcResultSummaryBadDebtProvision.Empty;

        public CalcResultSummaryBadDebtProvision LaDataPrepSection4 { get; set; } = CalcResultSummaryBadDebtProvision.Empty;

        public CalcResultSummaryBadDebtProvision SaSetupCostsSection5 { get; set; } = CalcResultSummaryBadDebtProvision.Empty;

        public CalcResultSummaryBadDebtProvision TotalProducerBillBreakdownCosts { get; set; } = CalcResultSummaryBadDebtProvision.Empty;

        public decimal PercentageofProducerReportedTonnagevsAllProducers { get; set; }

        // Section Total bill (1 + 2a + 2b + 2c)
        public decimal ProducerTotalOnePlus2A2B2CWithBadDeptProvision { get; set; }

        public decimal ProducerOverallPercentageOfCostsForOnePlus2A2B2C { get; set; }
        // End Section Total bill (1 + 2a + 2b + 2c)

        public Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial> ProducerDisposalFeesByMaterial { get; set; } = new();

        public Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial> ProducerCommsFeesByMaterial { get; set; } = new();

        public string? TonnageChangeCount { get; set; }

        public string? TonnageChangeAdvice { get; set; }

        public CalcResultSummaryBillingInstruction? BillingInstructionSection { get; set; }

        public bool isOverallTotalRow { get; set; }
        public int ProducerIdInt { get; set; }
    }
}
