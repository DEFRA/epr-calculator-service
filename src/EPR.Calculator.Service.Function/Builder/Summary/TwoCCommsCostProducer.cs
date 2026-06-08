using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.Summary.Common;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary;

public static class TwoCCommsCostProducer
{
    public static void SetValues(CalcResult calcResult, CalcResultSummary summary)
    {
        // Section 2c
        summary.TwoCCommsCostsByCountryWithoutBadDebtProvision =
          calcResult.CalcResultCommsCostReportDetail.CommsCostByCountry.Total;

        summary.TwoCBadDebtProvision =
            calcResult.CalcResultParameterOtherCost.BadDebtValue
            * summary.TwoCCommsCostsByCountryWithoutBadDebtProvision
            / 100;

        summary.TwoCCommsCostsByCountryWithBadDebtProvision =
            summary.TwoCCommsCostsByCountryWithoutBadDebtProvision + summary.TwoCBadDebtProvision;
    }

    public static void UpdateTwoCRows(CalcResult calcResult, CalcResultSummaryProducerDisposalFees result,
        ProducerDetail producer, IReadOnlyList<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
    {
        var commsWithBadDebt2C = GetCommWithBadDebt2C(calcResult, producer, hhTotalPackagingTonnage);
        var commsCost = calcResult.CalcResultCommsCostReportDetail.CommsCostByCountry;
        // TODO use TwoCWithBadDebt: CostByCountry
        result.TwoCEnglandTotalWithBadDebt         = commsWithBadDebt2C * commsCost.England;
        result.TwoCWalesTotalWithBadDebt           = commsWithBadDebt2C * commsCost.Wales;
        result.TwoCScotlandTotalWithBadDebt        = commsWithBadDebt2C * commsCost.Scotland;
        result.TwoCNorthernIrelandTotalWithBadDebt = commsWithBadDebt2C * commsCost.NorthernIreland;

        result.TwoCTotalProducerFeeForCommsCostsWithoutBadDebt =
            calcResult.CalcResultCommsCostReportDetail.CommsCostByCountry.Total
            * result.PercentageofProducerReportedTonnagevsAllProducers
            / 100;

        var badDebtProvisionValue =
            calcResult.CalcResultParameterOtherCost.BadDebtValue
            * calcResult.CalcResultCommsCostReportDetail.CommsCostByCountry.Total
            / 100;

        result.TwoCBadDebtProvision =
            badDebtProvisionValue
            * result.PercentageofProducerReportedTonnagevsAllProducers
            / 100;

        result.TwoCTotalProducerFeeForCommsCostsWithBadDebt =
            result.TwoCTotalProducerFeeForCommsCostsWithoutBadDebt
            + result.TwoCBadDebtProvision;
    }

    private static decimal GetCommWithBadDebt2C(CalcResult calcResult, ProducerDetail producer, IReadOnlyList<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
    {
        decimal badDebtProvision = calcResult.CalcResultParameterOtherCost.BadDebtValue / 100;
        decimal percentageOfProducerReportedHhTonnageVsAllProducers =
            // TODO review this
            TonnageVsAllProducerUtil.GetPercentageofProducerReportedTonnagevsAllProducers(producer, hhTotalPackagingTonnage) / 100;
        return (1 + badDebtProvision) * percentageOfProducerReportedHhTonnageVsAllProducers;
    }
}
