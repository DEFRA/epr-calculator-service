using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary;

public static class SectionOneAndTwoAProducer
{
    public static void SetValues(
        CalcResultSummaryProducerDisposalFees totals,
        CalcResultSummary summary
    )
    {
        // TODO should this also be a CalcResultSummaryBadDebtProvision
        summary.TotalFeeforLADisposalCostswoBadDebtprovision1   = totals.LocalAuthorityDisposalCostsSectionOne.FeeWithoutBadDebtProvision;
        summary.BadDebtProvisionFor1                            = totals.LocalAuthorityDisposalCostsSectionOne.BadDebtProvision;
        summary.TotalFeeforLADisposalCostswithBadDebtprovision1 = totals.LocalAuthorityDisposalCostsSectionOne.FeeWithBadDebtProvision.Total;

        summary.TotalFeeforCommsCostsbyMaterialwoBadDebtProvision2A   = totals.TotalProducerCommsFee;
        summary.BadDebtProvisionFor2A                                 = totals.CommunicationCostsSectionTwoA?.BadDebtProvision ?? 0m;
        summary.TotalFeeforCommsCostsbyMaterialwithBadDebtprovision2A = totals.TotalProducerCommsFeeWithBadDebtProvision.Total;
    }
}
