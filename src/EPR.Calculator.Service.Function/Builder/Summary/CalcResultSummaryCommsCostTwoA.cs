using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.Summary.Common;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary;

public static class CalcResultSummaryCommsCostTwoA
{
    public static decimal GetPriceperTonneForComms(MaterialDetail material, CalcResult calcResult)
    {
        var commsCostDataDetail = calcResult.CalcResultCommsCostReportDetail.ByMaterial.GetValueOrDefault(material.Code);
        return commsCostDataDetail?.PricePerTonne ?? 0m;
    }

    public static decimal GetTotalReportedTonnage(
        ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
        ProducerDetail producer,
        MaterialDetail material
    )
    {
        decimal hdcTonnage = 0;
        var hhPackagingWasteTonnage = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.Household);
        var reportedPublicBinTonnage = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.PublicBin);

        if (material.Code == MaterialCodes.Glass)
        {
            hdcTonnage = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.HouseholdDrinksContainers);
        }

        return material.Code == MaterialCodes.Glass
            ? hdcTonnage + reportedPublicBinTonnage + hhPackagingWasteTonnage
            : hhPackagingWasteTonnage + reportedPublicBinTonnage;
    }

    public static decimal GetProducerTotalCostWithoutBadDebtProvision(
        ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
        ProducerDetail producer,
        MaterialDetail material,
        CalcResult calcResult
    )
    {
        var priceperTonne = GetPriceperTonneForComms(material, calcResult);
        return GetTotalReportedTonnage(projectedMaterialsLookup, producer, material) * priceperTonne;
    }

    public static CalcResultSummaryBadDebtProvision GetCommsFeesCosts(
        ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
        ProducerDetail producer,
        MaterialDetail material,
        CalcResult calcResult
    )
    {
        var feeWithoutBadDebt = GetProducerTotalCostWithoutBadDebtProvision(projectedMaterialsLookup, producer, material, calcResult);
        var badDebtRate = calcResult.CalcResultParameterOtherCost.BadDebtValue;
        var apportionment = calcResult.CalcResultOnePlusFourApportionment.OnePlusFourApportionment;
        return new CalcResultSummaryBadDebtProvision
        {
            FeeWithoutBadDebtProvision = feeWithoutBadDebt,
            BadDebtProvision           = (feeWithoutBadDebt * badDebtRate / 100 * apportionment).Total,
            FeeWithBadDebtProvision    = (feeWithoutBadDebt * (1 + badDebtRate / 100)) * apportionment,
        };
    }
}
