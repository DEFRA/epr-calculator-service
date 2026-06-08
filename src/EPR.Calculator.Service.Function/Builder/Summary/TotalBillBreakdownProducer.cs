using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary;

public static class TotalBillBreakdownProducer
{
    public static void SetValues(CalcResultSummary summary)
    {
        foreach (var fee in summary.ProducerDisposalFees)
        {
            fee.TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision
            {
                FeeWithoutBadDebtProvision = GetTotalProducerBillWithoutBadDebtProvision(fee) ?? 0,
                BadDebtProvision           = GetBadDebtProvisionForTotalProducerBill(fee) ?? 0,
                FeeWithBadDebtProvision    = GetTotalProducerBillWithBadDebtProvision(fee)
            };
        }
    }

    // TODO add these up once, then separate out the Without/With/BadDebt
    private static decimal? GetTotalProducerBillWithoutBadDebtProvision(CalcResultSummaryProducerDisposalFees fee)
    {
        return fee.LADisposalCostsSection1.FeeWithoutBadDebtProvision
            + fee.CommsCostsSection2a.FeeWithoutBadDebtProvision
            + fee.CommsCostsSection2b.FeeWithoutBadDebtProvision
            + fee.CommsCostsSection2c.FeeWithoutBadDebtProvision
            + fee.SaOperatingCostsSection3.FeeWithoutBadDebtProvision
            + fee.LaDataPrepSection4.FeeWithoutBadDebtProvision
            + fee.SaSetupCostsSection5.FeeWithoutBadDebtProvision;
    }

    private static decimal? GetBadDebtProvisionForTotalProducerBill(CalcResultSummaryProducerDisposalFees fee)
    {
        return fee.LADisposalCostsSection1.BadDebtProvision
            + fee.CommsCostsSection2a.BadDebtProvision
            + fee.CommsCostsSection2b.BadDebtProvision
            + fee.CommsCostsSection2c.BadDebtProvision
            + fee.SaOperatingCostsSection3.BadDebtProvision
            + fee.LaDataPrepSection4.BadDebtProvision
            + fee.SaSetupCostsSection5.BadDebtProvision;
    }

    private static ByCountryCost GetTotalProducerBillWithBadDebtProvision(CalcResultSummaryProducerDisposalFees fee)
    {
        return fee.LADisposalCostsSection1.FeeWithBadDebtProvision
            + fee.CommsCostsSection2a.FeeWithBadDebtProvision
            + fee.CommsCostsSection2b.FeeWithBadDebtProvision
            + fee.CommsCostsSection2c.FeeWithBadDebtProvision
            + fee.SaOperatingCostsSection3.FeeWithBadDebtProvision
            + fee.LaDataPrepSection4.FeeWithBadDebtProvision
            + fee.SaSetupCostsSection5.FeeWithBadDebtProvision;
    }
}
