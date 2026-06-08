namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultSummary
    {
        public CalcResultSummaryBadDebtProvision LADisposalCostsSection1 { get; set; } = CalcResultSummaryBadDebtProvision.Empty;

        public CalcResultSummaryBadDebtProvision CommsCostsSection2a { get; set; } = CalcResultSummaryBadDebtProvision.Empty;

        public CalcResultSummaryBadDebtProvision CommsCostsSection2b { get; set; } = CalcResultSummaryBadDebtProvision.Empty;

        public CalcResultSummaryBadDebtProvision CommsCostsSection2c { get; set; } = CalcResultSummaryBadDebtProvision.Empty;

        // Section Total bill (1 + 2a + 2b + 2c)
        public decimal TotalOnePlus2A2B2CFeeWithBadDebtProvision { get; set; } // TODO is this just for exporters?
        // End Section Total bill (1 + 2a + 2b + 2c)

        public CalcResultSummaryBadDebtProvision LaDataPrepSection4 { get; set; } = CalcResultSummaryBadDebtProvision.Empty;

        public CalcResultSummaryBadDebtProvision SaOperatingCostsSection3 { get; set; } = CalcResultSummaryBadDebtProvision.Empty;

        public CalcResultSummaryBadDebtProvision SaSetupCostsSection5 { get; set; } = CalcResultSummaryBadDebtProvision.Empty;

        public IEnumerable<CalcResultSummaryProducerDisposalFees> ProducerDisposalFees { get; set; }
            = new List<CalcResultSummaryProducerDisposalFees>();
    }
}
