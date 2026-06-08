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
        summary.LADisposalCostsSection1 = totals.LADisposalCostsSection1;
        summary.CommsCostsSection2a     = totals.CommsCostsSection2a;
    }
}
