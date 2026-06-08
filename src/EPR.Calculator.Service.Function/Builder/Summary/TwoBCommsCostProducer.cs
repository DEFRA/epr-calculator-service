using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary;

public static class TwoBCommsCostProducer
{
    public static void SetValues(CalcResult calcResult, CalcResultSummary summary)
    {
        summary.CommsCostHeaderWithoutBadDebtFor2bTitle =
            calcResult.CalcResultCommsCostReportDetail.CommsCostUkWide.Total;

        summary.CommsCostHeaderBadDebtProvisionFor2bTitle =
            summary.CommsCostHeaderWithoutBadDebtFor2bTitle
            * calcResult.CalcResultParameterOtherCost.BadDebtValue
            / 100;

        summary.CommsCostHeaderWithBadDebtFor2bTitle =
            summary.CommsCostHeaderWithoutBadDebtFor2bTitle
            + summary.CommsCostHeaderBadDebtProvisionFor2bTitle;
    }
}
