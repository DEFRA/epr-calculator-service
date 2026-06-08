using EPR.Calculator.Service.Function.Builder.Summary.Common;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary;

// Sections 3, 4, 5 are structurally identical: a fixed cost total is apportioned to producers
// by their ProducerOverallPercentage and country apportionment. The only differences between
// sections are the source of the total, which summary header fields to write, which fee property
// to assign, and which country-apportionment table to use.

public static class ThreeSaCostsProducer
{
    public static void GetProducerSetUpCostsSection3(
        CalcResult calcResult,
        CalcResultSummary summary
    ) =>
        SectionCosts.Apply(
            calcResult, summary,
            sectionTotal: calcResult.CalcResultParameterOtherCost.SaOperatingCost.Total,
            setHeaders: (wo, bd, with) => {
                summary.SaOperatingCostsWoTitleSection3 = wo;
                summary.BadDebtProvisionTitleSection3 = bd;
                summary.SaOperatingCostsWithTitleSection3 = with;
            },
            setFee: (fee, provision) => fee.SchemeAdministratorOperatingCosts = provision,
            countryApportionment: CalcResultSummaryUtil.GetCountryOnePlusFourApportionment);

    public static decimal GetCountryTotalWithBadDebtProvision(
        CalcResult calcResult,
        decimal sectionTotal,
        decimal producerOverallPercentage,
        Countries country
    ) =>
        CalcResultSummaryUtil.GetApportionedCountryTotal(calcResult, sectionTotal, producerOverallPercentage, country,
            CalcResultSummaryUtil.GetCountryOnePlusFourApportionment);
}

public static class LaDataPrepCostsProducer
{
    public static void SetValues(
        CalcResult calcResult,
        CalcResultSummary summary
    ) =>
        SectionCosts.Apply(
            calcResult,
            summary,
            sectionTotal: calcResult.CalcResultParameterOtherCost.LaDataPrepCharge?.Total ?? 0m,
            setHeaders: (wo, bd, with) => {
                summary.LaDataPrepCostsTitleSection4                     = wo;
                summary.LaDataPrepCostsBadDebtProvisionTitleSection4     = bd;
                summary.LaDataPrepCostsWithBadDebtProvisionTitleSection4 = with;
            },
            setFee: (fee, provision) => fee.LocalAuthorityDataPreparationCosts = provision,
            countryApportionment: CalcResultSummaryUtil.GetParamsOtherFourCountryApportionmentPercentage
        );

    public static decimal GetCountryTotalWithBadDebtProvision(
        CalcResult calcResult,
        decimal sectionTotal,
        decimal producerOverallPercentage,
        Countries country
    ) =>
        CalcResultSummaryUtil.GetApportionedCountryTotal(
            calcResult,
            sectionTotal,
            producerOverallPercentage,
            country,
            CalcResultSummaryUtil.GetParamsOtherFourCountryApportionmentPercentage
        );
}

public static class SaSetupCostsProducer
{
    public static void GetProducerSetUpCosts(
        CalcResult calcResult,
        CalcResultSummary summary
    ) =>
        SectionCosts.Apply(
            calcResult,
            summary,
            sectionTotal: calcResult.CalcResultParameterOtherCost.SchemeSetupCost.Total,
            setHeaders: (wo, bd, with) => {
                summary.SaSetupCostsTitleSection5                     = wo;
                summary.SaSetupCostsBadDebtProvisionTitleSection5     = bd;
                summary.SaSetupCostsWithBadDebtProvisionTitleSection5 = with;
            },
            setFee: (fee, provision) => fee.OneOffSchemeAdministrationSetupCosts = provision,
            countryApportionment: CalcResultSummaryUtil.GetCountryOnePlusFourApportionment);

    public static decimal GetCountryTotalWithBadDebtProvision(
        CalcResult calcResult,
        decimal sectionTotal,
        decimal producerOverallPercentage,
        Countries country
    ) =>
        CalcResultSummaryUtil.GetApportionedCountryTotal(calcResult, sectionTotal, producerOverallPercentage, country,
            CalcResultSummaryUtil.GetCountryOnePlusFourApportionment);
}

internal static class SectionCosts
{
    internal static void Apply(
        CalcResult calcResult,
        CalcResultSummary summary,
        decimal sectionTotal,
        Action<decimal, decimal, decimal> setHeaders,
        Action<CalcResultSummaryProducerDisposalFees, CalcResultSummaryBadDebtProvision> setFee,
        Func<CalcResult, Countries, decimal> countryApportionment
    )
    {
        var badDebtRate = calcResult.CalcResultParameterOtherCost.BadDebtValue;
        var badDebt = sectionTotal * badDebtRate / 100;
        setHeaders(sectionTotal, badDebt, sectionTotal + badDebt);

        foreach (var fee in summary.ProducerDisposalFees)
        {
            var without = fee.ProducerOverallPercentageOfCostsForOnePlus2A2B2C * sectionTotal / 100;
            var feeDebt = without * badDebtRate / 100;

            setFee(fee, new CalcResultSummaryBadDebtProvision
            {
                TotalProducerFeeWithoutBadDebtProvision  = without,
                BadDebtProvision                         = feeDebt,
                TotalProducerFeeWithBadDebtProvision     = without + feeDebt,
                EnglandTotalWithBadDebtProvision         = CalcResultSummaryUtil.GetApportionedCountryTotal(calcResult, sectionTotal, fee.ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries.England,         countryApportionment),
                WalesTotalWithBadDebtProvision           = CalcResultSummaryUtil.GetApportionedCountryTotal(calcResult, sectionTotal, fee.ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries.Wales,           countryApportionment),
                ScotlandTotalWithBadDebtProvision        = CalcResultSummaryUtil.GetApportionedCountryTotal(calcResult, sectionTotal, fee.ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries.Scotland,        countryApportionment),
                NorthernIrelandTotalWithBadDebtProvision = CalcResultSummaryUtil.GetApportionedCountryTotal(calcResult, sectionTotal, fee.ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries.NorthernIreland, countryApportionment)
            });
        }
    }
}
