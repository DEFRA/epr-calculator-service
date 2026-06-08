using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.Summary.Common;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary;

public static class CalcResultSummaryCommsCostTwoBTotalBill
{
    #region Single RowbyRow
    public static ByCountryCost GetCommsWithBadDebt(
        CalcResult calcResult,
        ProducerDetail producer,
        IReadOnlyList<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage
    )
    {
        decimal commsCostHeaderWithoutBadDebtFor2bTitle = CalcResultSummaryUtil.GetCommsCostHeaderWithoutBadDebtFor2bTitle(calcResult);
        decimal percentageOfProducerReportedHHTonnagevsAllProducers = TonnageVsAllProducerUtil.GetPercentageofProducerReportedTonnagevsAllProducers(producer, hhTotalPackagingTonnage) / 100;
        var apportionment = calcResult.CalcResultOnePlusFourApportionment.OnePlusFourApportionment;
        decimal badDebtProvision = calcResult.CalcResultParameterOtherCost.BadDebtValue / 100;
        return commsCostHeaderWithoutBadDebtFor2bTitle * (1 + badDebtProvision) * percentageOfProducerReportedHHTonnagevsAllProducers * apportionment;
    }

    public static decimal GetCommsBadDebtProvisionFor2b(CalcResult calcResult, ProducerDetail producer, IReadOnlyList<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
    {
        decimal producerFeeWithoutBadDebtFor2b = GetCommsProducerFeeWithoutBadDebtFor2b(calcResult, producer, hhTotalPackagingTonnage);
        decimal badDebtProvision = calcResult.CalcResultParameterOtherCost.BadDebtValue / 100;
        return producerFeeWithoutBadDebtFor2b * badDebtProvision;
    }

    public static decimal GetCommsProducerFeeWithoutBadDebtFor2b(CalcResult calcResult, ProducerDetail producer, IReadOnlyList<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
    {
        decimal commsCostHeaderWithoutBadDebtFor2bTitle = CalcResultSummaryUtil.GetCommsCostHeaderWithoutBadDebtFor2bTitle(calcResult);
        decimal percentageOfProducerReportedHHTonnagevsAllProducers = TonnageVsAllProducerUtil.GetPercentageofProducerReportedTonnagevsAllProducers(producer, hhTotalPackagingTonnage) / 100;

        return commsCostHeaderWithoutBadDebtFor2bTitle * percentageOfProducerReportedHHTonnagevsAllProducers;
    }

    #endregion
}
