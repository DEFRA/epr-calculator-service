using EPR.Calculator.API.Data.DataModels;
﻿using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary;

public static class OnePlus2A2B2CProducer
{
    public static void SetValues(CalcResultSummary summary)
    {
        var headerTotal = summary.OverallTotal.ProducerTotalOnePlus2A2B2CWithBadDebtProvision();
        foreach (var fee in summary.ProducerDisposalFees)
        {
            fee.ProducerOverallPercentageOfCostsForOnePlus2A2B2C = GetOverallProducerPercentage(fee, headerTotal);
        }
        summary.OverallTotal.ProducerOverallPercentageOfCostsForOnePlus2A2B2C = GetOverallProducerPercentage(summary.OverallTotal, headerTotal);
    }

    private static decimal GetOverallProducerPercentage(CalcResultSummaryProducerDisposalFees fee, decimal totalOnePlus2A2B2CFeeWithBadDebtProvision)
    {
        return totalOnePlus2A2B2CFeeWithBadDebtProvision == 0
            ? 0
            : (fee.ProducerTotalOnePlus2A2B2CWithBadDebtProvision() / totalOnePlus2A2B2CFeeWithBadDebtProvision) * 100;
    }
}
