using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary;

public static class TwoCCommsCostProducer
{
    public static void SetValues(CalcResult calcResult, CalcResultSummary summary)
    {
        var commsCostByCountry = calcResult.CalcResultCommsCostReportDetail.CommsCostByCountry;
        var badDebtProvision = calcResult.CalcResultParameterOtherCost.BadDebtValue / 100 * commsCostByCountry;
        summary.OverallTotal.CommsCostsSection2c = new CalcResultSummaryBadDebtProvision
        {
            FeeWithoutBadDebtProvision = commsCostByCountry.Total,
            BadDebtProvision           = badDebtProvision.Total,
            FeeWithBadDebtProvision    = commsCostByCountry + badDebtProvision
        };
    }

    public static void UpdateTwoCRows(CalcResult calcResult, CalcResultSummaryProducerDisposalFees result)
    {
        var commsCost = calcResult.CalcResultCommsCostReportDetail.CommsCostByCountry;

        var badDebtProvisionValue =
            calcResult.CalcResultParameterOtherCost.BadDebtValue / 100
            * calcResult.CalcResultCommsCostReportDetail.CommsCostByCountry;

        result.CommsCostsSection2c = new CalcResultSummaryBadDebtProvision
        {
            FeeWithoutBadDebtProvision =
                commsCost.Total
                * result.PercentageofProducerReportedTonnagevsAllProducers
                / 100,
            BadDebtProvision = badDebtProvisionValue.Total
                * result.PercentageofProducerReportedTonnagevsAllProducers
                / 100,
            FeeWithBadDebtProvision = (commsCost + badDebtProvisionValue)
                * (result.PercentageofProducerReportedTonnagevsAllProducers
                / 100)
        };
    }
}
