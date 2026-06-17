using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.Summary.Common;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary;

public static class CalcResultSummaryCommsCostTwoA
{
    public static decimal GetPriceperTonneForComms(
        MaterialDetail material,
        CalcResult calcResult
    ) =>
        calcResult.CalcResultCommsCostReportDetail.ByMaterial.GetValueOrDefault(material.Code)?.PricePerTonne ?? 0m;

    public static decimal GetTotalReportedTonnage(
        ILookup<(int, string?), TransformProducerReportedMaterial> projectedMaterialsLookup,
        ProducerDetail producer,
        MaterialDetail material
    ) =>
        CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.Household) +
        CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.PublicBin) +
        (material.Code == MaterialCodes.Glass
            ? CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.HouseholdDrinksContainers)
            : 0);

    public static CalcResultSummaryBadDebtProvision GetCommsFeesCosts(
        ILookup<(int, string?), TransformProducerReportedMaterial> projectedMaterialsLookup,
        ProducerDetail producer,
        MaterialDetail material,
        CalcResult calcResult
    ) =>
        GetCommsFeesCosts(GetTotalReportedTonnage(projectedMaterialsLookup, producer, material), material, calcResult);

    public static CalcResultSummaryBadDebtProvision GetCommsFeesCosts(
        decimal totalReportedTonnage,
        MaterialDetail material,
        CalcResult calcResult
    )
    {
        var feeWithoutBadDebt = totalReportedTonnage * GetPriceperTonneForComms(material, calcResult);
        var badDebtRate       = calcResult.CalcResultParameterOtherCost.BadDebtValue;
        var apportionment     = calcResult.CalcResultOnePlusFourApportionment.OnePlusFourApportionment;
        return new CalcResultSummaryBadDebtProvision
        {
            FeeWithoutBadDebtProvision = feeWithoutBadDebt,
            BadDebtProvision           = (feeWithoutBadDebt * badDebtRate / 100 * apportionment).Total,
            FeeWithBadDebtProvision    = (feeWithoutBadDebt * (1 + badDebtRate / 100)) * apportionment,
        };
    }
}
