using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.Summary.Common;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary;

public static class TwoCCommsCostProducer
{
    public static void SetValues(CalcResult calcResult, CalcResultSummary result)
    {
        // Section 2c
        result.TwoCCommsCostsByCountryWithoutBadDebtProvision =
          calcResult.CalcResultCommsCostReportDetail.CommsCostByCountry.Total;

        result.TwoCBadDebtProvision =
            calcResult.CalcResultParameterOtherCost.BadDebtValue
            * result.TwoCCommsCostsByCountryWithoutBadDebtProvision
            / 100;

        result.TwoCCommsCostsByCountryWithBadDebtProvision =
            result.TwoCCommsCostsByCountryWithoutBadDebtProvision + result.TwoCBadDebtProvision;
    }

    public static void UpdateTwoCRows(CalcResult calcResult, CalcResultSummaryProducerDisposalFees result,
        ProducerDetail producer, IReadOnlyList<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
    {
        result.TwoCEnglandTotalWithBadDebt =
            GetCommEnglandWithBadDebt2C(calcResult, producer, hhTotalPackagingTonnage);
        result.TwoCWalesTotalWithBadDebt =
            GetCommWalesWithBadDebt2C(calcResult, producer, hhTotalPackagingTonnage);
        result.TwoCNorthernIrelandTotalWithBadDebt =
            GetCommNIWithBadDebt2C(calcResult, producer, hhTotalPackagingTonnage);
        result.TwoCScotlandTotalWithBadDebt =
            GetCommScotlandWithBadDebt2C(calcResult, producer, hhTotalPackagingTonnage);

        result.TwoCTotalProducerFeeForCommsCostsWithoutBadDebt =
            calcResult.CalcResultCommsCostReportDetail.CommsCostByCountry.Total
            * result.PercentageofProducerReportedTonnagevsAllProducers
            / 100;

        var badDebtProvisionValue =
            calcResult.CalcResultParameterOtherCost.BadDebtValue
            * calcResult.CalcResultCommsCostReportDetail.CommsCostByCountry.Total
            / 100;
        result.TwoCBadDebtProvision = badDebtProvisionValue *
            result.PercentageofProducerReportedTonnagevsAllProducers / 100;

        result.TwoCTotalProducerFeeForCommsCostsWithBadDebt =
            result.TwoCTotalProducerFeeForCommsCostsWithoutBadDebt + result.TwoCBadDebtProvision;
    }

    public static decimal GetCommWalesWithBadDebt2C(CalcResult calcResult, ProducerDetail producer, IReadOnlyList<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
    {
        decimal twoCCommCostsByCountryBadDebtProvision = calcResult.CalcResultCommsCostReportDetail.CommsCostByCountry.Wales;
        return twoCCommCostsByCountryBadDebtProvision * GetCommWithBadDebt2C(calcResult, producer, hhTotalPackagingTonnage);
    }

    public static decimal GetCommNIWithBadDebt2C(CalcResult calcResult, ProducerDetail producer, IReadOnlyList<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
    {
        decimal twoCCommCostsByCountryBadDebtProvision = calcResult.CalcResultCommsCostReportDetail.CommsCostByCountry.NorthernIreland;
        return twoCCommCostsByCountryBadDebtProvision * GetCommWithBadDebt2C(calcResult, producer, hhTotalPackagingTonnage);
    }

    public static decimal GetCommScotlandWithBadDebt2C(CalcResult calcResult, ProducerDetail producer, IReadOnlyList<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
    {
        decimal twoCCommCostsByCountryBadDebtProvision = calcResult.CalcResultCommsCostReportDetail.CommsCostByCountry.Scotland;
        return twoCCommCostsByCountryBadDebtProvision * GetCommWithBadDebt2C(calcResult, producer, hhTotalPackagingTonnage);
    }

    public static decimal GetCommEnglandWithBadDebt2C(CalcResult calcResult, ProducerDetail producer, IReadOnlyList<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
    {
        decimal twoCCommCostsByCountryBadDebtProvision = calcResult.CalcResultCommsCostReportDetail.CommsCostByCountry.England;
        return twoCCommCostsByCountryBadDebtProvision * GetCommWithBadDebt2C(calcResult, producer, hhTotalPackagingTonnage);
    }

    public static decimal GetCommWithBadDebt2C(CalcResult calcResult, ProducerDetail producer, IReadOnlyList<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
    {
        decimal badDebtProvision = calcResult.CalcResultParameterOtherCost.BadDebtValue / 100;
        decimal percentageOfProducerReportedHhTonnageVsAllProducers = TonnageVsAllProducerUtil.GetPercentageofProducerReportedTonnagevsAllProducers(producer, hhTotalPackagingTonnage) / 100;
        return (1 + badDebtProvision) * percentageOfProducerReportedHhTonnageVsAllProducers;
    }
}
