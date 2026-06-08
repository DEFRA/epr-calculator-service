using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.Summary.Common;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary;

public static class TwoCCommsCostProducer
{
    public static void SetValues(CalcResult calcResult, CalcResultSummary summary)
    {
        var commsCostByCountry = calcResult.CalcResultCommsCostReportDetail.CommsCostByCountry;
        var badDebtProvision = calcResult.CalcResultParameterOtherCost.BadDebtValue / 100 * commsCostByCountry;
        summary.CommsCostsSection2c = new CalcResultSummaryBadDebtProvision
        {
            FeeWithoutBadDebtProvision = commsCostByCountry.Total,
            BadDebtProvision           = badDebtProvision.Total,
            FeeWithBadDebtProvision    = commsCostByCountry + badDebtProvision
        };
    }

    public static void UpdateTwoCRows(CalcResult calcResult, CalcResultSummaryProducerDisposalFees result,
        ProducerDetail producer, IReadOnlyList<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
    {
        //var commsWithBadDebt2C = GetCommWithBadDebt2C(calcResult, producer, hhTotalPackagingTonnage);
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
                / 100) //commsWithBadDebt2C * commsCost
        };
    }

    /*private static decimal GetCommWithBadDebt2C(CalcResult calcResult, ProducerDetail producer, IReadOnlyList<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
    {
        decimal badDebtProvision = calcResult.CalcResultParameterOtherCost.BadDebtValue / 100;
        decimal percentageOfProducerReportedHhTonnageVsAllProducers =
            // TODO review this
            TonnageVsAllProducerUtil.GetPercentageofProducerReportedTonnagevsAllProducers(producer, hhTotalPackagingTonnage) / 100;
        return (1 + badDebtProvision) * percentageOfProducerReportedHhTonnageVsAllProducers;
    }*/
}
