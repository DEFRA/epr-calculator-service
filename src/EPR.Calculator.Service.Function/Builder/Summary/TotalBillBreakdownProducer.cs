using EPR.Calculator.API.Data.DataModels;
﻿using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary;

public static class TotalBillBreakdownProducer
{
    public static void SetValues(CalcResultSummary summary)
    {
        foreach (var fee in summary.ProducerDisposalFees.Append(summary.OverallTotal).OfType<CalcResultSummaryProducerDisposalFees>())
        {
            fee.TotalProducerBillBreakdownCosts =
                fee.LADisposalCostsSection1
                + fee.CommsCostsSection2a
                + fee.CommsCostsSection2b
                + fee.CommsCostsSection2c
                + fee.SaOperatingCostsSection3
                + fee.LaDataPrepSection4
                + fee.SaSetupCostsSection5;
        }
    }
}
