using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary;

public static class TwoBCommsCostProducer
{
    public static void SetValues(CalcResult calcResult, CalcResultSummary summary)
    {
        var withoutbadDebtProvision = calcResult.CalcResultCommsCostReportDetail.CommsCostUkWide;
        var badDebtProvision = calcResult.CalcResultParameterOtherCost.BadDebtValue / 100 * withoutbadDebtProvision;
        summary.CommsCostsSection2b = new CalcResultSummaryBadDebtProvision
        {
            FeeWithoutBadDebtProvision = withoutbadDebtProvision.Total,
            BadDebtProvision           = badDebtProvision.Total,
            FeeWithBadDebtProvision    = withoutbadDebtProvision + badDebtProvision
        };
    }
}
