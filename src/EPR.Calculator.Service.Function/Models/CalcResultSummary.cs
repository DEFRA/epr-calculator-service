namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultSummary
    {
        //Section-(1) & (2a)
        public required CalcResultSummaryBadDebtProvision LocalAuthorityDisposalCostsSectionOne { get; set; }

        public required CalcResultSummaryBadDebtProvision CommsCostsSectionTwoA { get; set; }

        // TODO rename CommsCostsSectionTwoB
        public required CalcResultSummaryBadDebtProvision CommsCostsHeaderFor2bTitle { get; set; }

        public required CalcResultSummaryBadDebtProvision TwoCCommsCosts { get; set; }

        // Section Total bill (1 + 2a + 2b + 2c)
        public decimal TotalOnePlus2A2B2CFeeWithBadDebtProvision { get; set; }
        // End Section Total bill (1 + 2a + 2b + 2c)

        public required CalcResultSummaryBadDebtProvision LaDataPrepSection4 { get; set; }

        public required CalcResultSummaryBadDebtProvision SchemeAdministratorOperatingCosts{ get; set; }

        public required CalcResultSummaryBadDebtProvision SaSetupCostsSection5 { get; set; }

        public IEnumerable<CalcResultSummaryProducerDisposalFees> ProducerDisposalFees { get; set; }
            = new List<CalcResultSummaryProducerDisposalFees>();
    }
}
