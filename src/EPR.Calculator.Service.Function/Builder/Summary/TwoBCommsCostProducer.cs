using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary;

public static class TwoBCommsCostProducer
{
    public static void SetValues(CalcResult calcResult, CalcResultSummary result)
    {
        result.CommsCostHeaderWithoutBadDebtFor2bTitle =
            calcResult.CalcResultCommsCostReportDetail.CommsCostUkWide.Total;

        result.CommsCostHeaderBadDebtProvisionFor2bTitle =
            result.CommsCostHeaderWithoutBadDebtFor2bTitle
            * calcResult.CalcResultParameterOtherCost.BadDebtValue
            / 100;

        result.CommsCostHeaderWithBadDebtFor2bTitle =
            result.CommsCostHeaderWithoutBadDebtFor2bTitle
            + result.CommsCostHeaderBadDebtProvisionFor2bTitle;
    }
}
