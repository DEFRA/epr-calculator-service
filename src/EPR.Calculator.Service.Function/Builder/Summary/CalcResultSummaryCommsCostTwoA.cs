using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.Summary.Common;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary;

public static class CalcResultSummaryCommsCostTwoA
{
    public static decimal GetPriceperTonneForComms(
        MaterialDetail material,
        FeesState state
    ) =>
        state.CommsCost.ByMaterial.GetValueOrDefault(material.Code)?.PricePerTonne ?? 0m;

    public static decimal GetTotalReportedTonnage(
        ILookup<(int, string?), ProducerMaterialPackaging> projectedMaterialsLookup,
        ProducerDetail producer,
        MaterialDetail material
    ) =>
        ProducerFeesUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.Household) +
        ProducerFeesUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.PublicBin) +
        (material.Code == MaterialCodes.Glass
            ? ProducerFeesUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.HouseholdDrinksContainers)
            : 0);

    public static FeeWithBadDebt GetCommsFeesCosts(
        ILookup<(int, string?), ProducerMaterialPackaging> projectedMaterialsLookup,
        ProducerDetail producer,
        MaterialDetail material,
        FeesState state
    ) =>
        GetCommsFeesCosts(GetTotalReportedTonnage(projectedMaterialsLookup, producer, material), material, state);

    public static FeeWithBadDebt GetCommsFeesCosts(
        decimal totalReportedTonnage,
        MaterialDetail material,
        FeesState state
    )
    {
        var feeWithoutBadDebt = totalReportedTonnage * GetPriceperTonneForComms(material, state);
        var badDebtRate       = state.OtherCost.BadDebtValue;
        var apportionment     = state.Apportionment.OnePlusFourApportionment;
        return new FeeWithBadDebt
        {
            FeeWithoutBadDebt = feeWithoutBadDebt,
            BadDebt           = (feeWithoutBadDebt * badDebtRate / 100 * apportionment).Total,
            ByCountry    = (feeWithoutBadDebt * (1 + badDebtRate / 100)) * apportionment,
        };
    }
}
