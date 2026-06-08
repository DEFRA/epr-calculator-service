using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary;

public static class SectionOneAndTwoAProducer
{
    public static void SetValues(
        CalcResultSummaryProducerDisposalFees totals,
        CalcResultSummary summary
    )
    {
        // TODO why are we copying from totals to summary?
        summary.LocalAuthorityDisposalCostsSectionOne = totals.LocalAuthorityDisposalCostsSectionOne ?? CalcResultSummaryBadDebtProvision.Empty;

        // TODO use CalcResultSummaryBadDebtProvision here
        summary.TotalFeeforCommsCostsbyMaterialwoBadDebtProvision2A   = totals.CommsCostsSectionTwoA.FeeWithoutBadDebtProvision;
        summary.BadDebtProvisionFor2A                                 = totals.CommsCostsSectionTwoA.BadDebtProvision;
        summary.TotalFeeforCommsCostsbyMaterialwithBadDebtprovision2A = totals.CommsCostsSectionTwoA.FeeWithBadDebtProvision.Total;
    }
}
