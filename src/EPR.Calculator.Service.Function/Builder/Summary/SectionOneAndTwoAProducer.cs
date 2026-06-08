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
        // TODO inline them into Summary Builder
        summary.LocalAuthorityDisposalCostsSectionOne = totals.LocalAuthorityDisposalCostsSectionOne ?? CalcResultSummaryBadDebtProvision.Empty;
        summary.CommsCostsSectionTwoA                 = totals.CommsCostsSectionTwoA;
    }
}
