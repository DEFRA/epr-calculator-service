using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary;

public static class OnePlus2A2B2CProducer
{
    public static void SetValues(CalcResultSummary summary)
    {
        summary.TotalOnePlus2A2B2CFeeWithBadDebtProvision = GetHeaderTotalFeeWithBadDebtProvision(summary);
        foreach (var fee in summary.ProducerDisposalFees)
        {
            fee.ProducerTotalOnePlus2A2B2CWithBadDeptProvision = GetTotalWithBadDebtProvision(fee) ?? 0;
            fee.ProducerOverallPercentageOfCostsForOnePlus2A2B2C = GetOverallProducerPercentage(fee, summary.TotalOnePlus2A2B2CFeeWithBadDebtProvision);
        }
    }

    private static decimal GetHeaderTotalFeeWithBadDebtProvision(CalcResultSummary summary)
    {
        return summary.TotalFeeforLADisposalCostswithBadDebtprovision1
            + summary.TotalFeeforCommsCostsbyMaterialwithBadDebtprovision2A
            + summary.CommsCostsHeaderFor2bTitle.FeeWithBadDebtProvision.Total
            + summary.TwoCCommsCosts.FeeWithBadDebtProvision.Total;
    }

    private static decimal? GetTotalWithBadDebtProvision(CalcResultSummaryProducerDisposalFees fee)
    {
        return fee.LocalAuthorityDisposalCostsSectionOne?.FeeWithBadDebtProvision?.Total // TODO this need to be optional?
            + fee.CommsCostsSectionTwoA.FeeWithBadDebtProvision.Total
            + fee.CommsCostsSectionTwoB?.FeeWithBadDebtProvision?.Total
            + fee.CommsCostsSectionTwoC.FeeWithBadDebtProvision.Total;
    }

    private static decimal GetOverallProducerPercentage(CalcResultSummaryProducerDisposalFees fee, decimal totalOnePlus2A2B2CFeeWithBadDebtProvision)
    {
        return totalOnePlus2A2B2CFeeWithBadDebtProvision == 0
            ? 0
            : (fee.ProducerTotalOnePlus2A2B2CWithBadDeptProvision / totalOnePlus2A2B2CFeeWithBadDebtProvision) * 100;
    }
}
