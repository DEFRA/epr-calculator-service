namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultSummary
    {
        //Section-(1) & (2a)
        public CalcResultSummaryBadDebtProvision LocalAuthorityDisposalCostsSectionOne { get; set; }

        public decimal TotalFeeforCommsCostsbyMaterialwoBadDebtProvision2A { get; set; }

        public decimal BadDebtProvisionFor2A { get; set; }

        public decimal TotalFeeforCommsCostsbyMaterialwithBadDebtprovision2A { get; set; }

        // Section Total bill (1 + 2a + 2b + 2c)
        public decimal TotalOnePlus2A2B2CFeeWithBadDebtProvision { get; set; }
        // End Section Total bill (1 + 2a + 2b + 2c)

        // Section-3
        public decimal SaOperatingCostsWoTitleSection3 { get; set; }

        public decimal SaOperatingCostsWithTitleSection3 { get; set; }

        public decimal BadDebtProvisionTitleSection3 { get; set; }
        //End Section-3

        // Section-4 LA data prep costs
        public decimal LaDataPrepCostsTitleSection4 { get; set; }

        public decimal LaDataPrepCostsBadDebtProvisionTitleSection4 { get; set; }

        public decimal LaDataPrepCostsWithBadDebtProvisionTitleSection4 { get; set; }
        // End Section-4 LA data prep costs

        // Section-5 SA setup costs
        public decimal SaSetupCostsTitleSection5 { get; set; }

        public decimal SaSetupCostsBadDebtProvisionTitleSection5 { get; set; }

        public decimal SaSetupCostsWithBadDebtProvisionTitleSection5 { get; set; }
        // End Section-5 SA setup costs

        public required CalcResultSummaryBadDebtProvision TwoCCommsCosts { get; set; }


        public CalcResultSummaryBadDebtProvision CommsCostsHeaderFor2bTitle { get; set; }

        public IEnumerable<CalcResultSummaryProducerDisposalFees> ProducerDisposalFees { get; set; }
            = new List<CalcResultSummaryProducerDisposalFees>();
    }
}
