using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.Summary.Common;

namespace EPR.Calculator.Service.Function.Builder.Summary;

public static class CalcResultSummaryCommsCostTwoBTotalBill
{
    public static FeeWithBadDebt GetCommsCosts(
        FeesState state,
        ProducerDetail producer,
        IReadOnlyList<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage
    )
    {
        var commsCostHeader = ProducerFeesUtil.GetCommsCostHeaderWithoutBadDebtFor2bTitle(state);
        var percentage = TonnageVsAllProducerUtil.GetPercentageofProducerReportedTonnagevsAllProducers(producer, hhTotalPackagingTonnage) / 100;
        var feeWithoutBadDebt = commsCostHeader * percentage;
        var badDebtRate = state.OtherCost.BadDebtValue / 100;
        var apportionment = state.Apportionment.OnePlusFourApportionment;
        return new FeeWithBadDebt
        {
            FeeWithoutBadDebt = feeWithoutBadDebt,
            BadDebt           = feeWithoutBadDebt * badDebtRate,
            ByCountry    = (feeWithoutBadDebt * (1 + badDebtRate)) * apportionment,
        };
    }
}
