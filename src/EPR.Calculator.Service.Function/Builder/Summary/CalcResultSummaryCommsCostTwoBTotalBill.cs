using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.Summary.Common;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary;

public static class CalcResultSummaryCommsCostTwoBTotalBill
{
    public static CalcResultSummaryBadDebtProvision GetCommsCosts(
        CalcResult calcResult,
        ProducerDetail producer,
        IReadOnlyList<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage
    )
    {
        var commsCostHeader = CalcResultSummaryUtil.GetCommsCostHeaderWithoutBadDebtFor2bTitle(calcResult);
        var percentage = TonnageVsAllProducerUtil.GetPercentageofProducerReportedTonnagevsAllProducers(producer, hhTotalPackagingTonnage) / 100;
        var feeWithoutBadDebt = commsCostHeader * percentage;
        var badDebtRate = calcResult.CalcResultParameterOtherCost.BadDebtValue / 100;
        var apportionment = calcResult.CalcResultOnePlusFourApportionment.OnePlusFourApportionment;
        return new CalcResultSummaryBadDebtProvision
        {
            FeeWithoutBadDebtProvision = feeWithoutBadDebt,
            BadDebtProvision           = feeWithoutBadDebt * badDebtRate,
            FeeWithBadDebtProvision    = (feeWithoutBadDebt * (1 + badDebtRate)) * apportionment,
        };
    }
}
