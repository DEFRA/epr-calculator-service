using EPR.Calculator.API.Data.DataModels;
﻿using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary;

public static class OnePlus2A2B2CProducer
{
    public static void SetValues(CalcResultSummary summary)
    {
        var headerTotal = summary.TotalOnePlus2A2B2CFeeWithBadDebtProvision;
        foreach (var fee in summary.ProducerDisposalFees)
        {
            fee.ProducerTotalOnePlus2A2B2CWithBadDeptProvision = GetTotalWithBadDebtProvision(fee);
            fee.ProducerOverallPercentageOfCostsForOnePlus2A2B2C = GetOverallProducerPercentage(fee, headerTotal);
        }
        summary.OverallTotal.ProducerTotalOnePlus2A2B2CWithBadDeptProvision = GetTotalWithBadDebtProvision(summary.OverallTotal);
        summary.OverallTotal.ProducerOverallPercentageOfCostsForOnePlus2A2B2C = GetOverallProducerPercentage(summary.OverallTotal, headerTotal);
    }

    private static decimal GetTotalWithBadDebtProvision(CalcResultSummaryProducerDisposalFees fee)
    {
        return fee.LADisposalCostsSection1.FeeWithBadDebtProvision.Total
            + fee.CommsCostsSection2a.FeeWithBadDebtProvision.Total
            + fee.CommsCostsSection2b.FeeWithBadDebtProvision.Total
            + fee.CommsCostsSection2c.FeeWithBadDebtProvision.Total;
    }

    private static decimal GetOverallProducerPercentage(CalcResultSummaryProducerDisposalFees fee, decimal totalOnePlus2A2B2CFeeWithBadDebtProvision)
    {
        return totalOnePlus2A2B2CFeeWithBadDebtProvision == 0
            ? 0
            : (fee.ProducerTotalOnePlus2A2B2CWithBadDeptProvision / totalOnePlus2A2B2CFeeWithBadDebtProvision) * 100;
    }
}
