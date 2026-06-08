using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.Summary.Common;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary;

public static class CalcResultSummaryCommsCostTwoA
{
    // TODO clean this up

    public static decimal GetProducerTotalCostWithoutBadDebtProvisionTotal(
        ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
        IReadOnlyList<ProducerDetail> producers,
        MaterialDetail material,
        CalcResult calcResult
    )
    {
        return producers.Sum(producer => GetProducerTotalCostWithoutBadDebtProvision(projectedMaterialsLookup, producer, material, calcResult));
    }

    public static decimal GetBadDebtProvisionForCommsCostTotal(
        ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
        IReadOnlyList<ProducerDetail> producers,
        MaterialDetail material,
        CalcResult calcResult
    )
    {
        return producers.Sum(producer => GetBadDebtProvisionForCommsCost(projectedMaterialsLookup, producer, material, calcResult).Total);
    }

    public static decimal GetPriceperTonneForComms(MaterialDetail material, CalcResult calcResult)
    {
        var commsCostDataDetail = calcResult.CalcResultCommsCostReportDetail.ByMaterial.GetValueOrDefault(material.Code);
        return commsCostDataDetail?.PricePerTonne ?? 0m;
    }

    public static ByCountryCost GetProducerTotalCostwithBadDebtProvision(
        ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
        ProducerDetail producer,
        MaterialDetail material,
        CalcResult calcResult
    )
    {
        var badDebtProvision = calcResult.CalcResultParameterOtherCost.BadDebtValue;
        var total = GetProducerTotalCostWithoutBadDebtProvision(projectedMaterialsLookup, producer, material, calcResult) * (1 + badDebtProvision / 100);
        return total * calcResult.CalcResultOnePlusFourApportionment.OnePlusFourApportionment;
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

    public static ByCountryCost GetBadDebtProvisionForCommsCost(
        ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
        ProducerDetail producer,
        MaterialDetail material,
        CalcResult calcResult
    )
    {
        var badDebtProvision = calcResult.CalcResultParameterOtherCost.BadDebtValue;
        var totalBadDebt = GetProducerTotalCostWithoutBadDebtProvision(projectedMaterialsLookup, producer, material, calcResult) * badDebtProvision / 100;
        return totalBadDebt * calcResult.CalcResultOnePlusFourApportionment.OnePlusFourApportionment;
    }

    public static decimal GetTotalReportedTonnageTotal(
        ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
        IReadOnlyList<ProducerDetail> producers,
        MaterialDetail material
    )
    {
        return producers.Sum(producer => GetTotalReportedTonnage(projectedMaterialsLookup, producer, material));
    }
}
