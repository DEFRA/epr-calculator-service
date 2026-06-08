using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary;

public static class SectionOneAndTwoAProducer
{
    public static void SetValues(
        CalcResultSummaryProducerDisposalFees totals,
        CalcResultSummary summary
    )
    {
        summary.TotalFeeforLADisposalCostswoBadDebtprovision1   = totals.TotalProducerDisposalFee;
        summary.BadDebtProvisionFor1                            = totals.LocalAuthorityDisposalCostsSectionOne?.BadDebtProvision ?? 0m;
        summary.TotalFeeforLADisposalCostswithBadDebtprovision1 = totals.TotalProducerDisposalFeeWithBadDebtProvision.Total;

        summary.TotalFeeforCommsCostsbyMaterialwoBadDebtProvision2A   = totals.TotalProducerCommsFee;
        summary.BadDebtProvisionFor2A                                 = totals.CommunicationCostsSectionTwoA?.BadDebtProvision ?? 0m;
        summary.TotalFeeforCommsCostsbyMaterialwithBadDebtprovision2A = totals.TotalProducerCommsFeeWithBadDebtProvision.Total;
    }
}
