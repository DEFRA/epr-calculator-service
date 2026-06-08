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
    public static void SetValues(
        CalcResult calcResult,
        CalcResultSummary summary
    ) =>
        SectionCosts.Apply(
            badDebtRate: calcResult.CalcResultParameterOtherCost.BadDebtValue,
            summary,
            sectionTotal: calcResult.CalcResultParameterOtherCost.SaOperatingCost.Total,
            apportionment: calcResult.CalcResultOnePlusFourApportionment.OnePlusFourApportionment,
            setHeaders: (wo, bd, with) => {
                summary.SaOperatingCostsWoTitleSection3   = wo;
                summary.BadDebtProvisionTitleSection3     = bd;
                summary.SaOperatingCostsWithTitleSection3 = with;
            },
            setFee: (fee, provision) => fee.SchemeAdministratorOperatingCosts = provision
        );
}

public static class LaDataPrepCostsProducer
{
    public static void SetValues(
        CalcResult calcResult,
        CalcResultSummary summary
    ) =>
        SectionCosts.Apply(
            badDebtRate: calcResult.CalcResultParameterOtherCost.BadDebtValue,
            summary,
            sectionTotal: calcResult.CalcResultParameterOtherCost.LaDataPrepCharge?.Total ?? 0m,
            apportionment: calcResult.CalcResultParameterOtherCost.CountryApportionment,
            setHeaders: (wo, bd, with) => {
                summary.LaDataPrepCostsTitleSection4                     = wo;
                summary.LaDataPrepCostsBadDebtProvisionTitleSection4     = bd;
                summary.LaDataPrepCostsWithBadDebtProvisionTitleSection4 = with;
            },
            setFee: (fee, provision) => fee.LocalAuthorityDataPreparationCosts = provision
        );
}

public static class SaSetupCostsProducer
{
    public static void SetValues(
        CalcResult calcResult,
        CalcResultSummary summary
    ) =>
        SectionCosts.Apply(
            badDebtRate: calcResult.CalcResultParameterOtherCost.BadDebtValue,
            summary,
            sectionTotal: calcResult.CalcResultParameterOtherCost.SchemeSetupCost.Total,
            apportionment: calcResult.CalcResultOnePlusFourApportionment.OnePlusFourApportionment,
            setHeaders: (wo, bd, with) => {
                summary.SaSetupCostsTitleSection5                     = wo;
                summary.SaSetupCostsBadDebtProvisionTitleSection5     = bd;
                summary.SaSetupCostsWithBadDebtProvisionTitleSection5 = with;
            },
            setFee: (fee, provision) => fee.OneOffSchemeAdministrationSetupCosts = provision
        );
}

internal static class SectionCosts
{
    internal static void Apply(
        decimal badDebtRate,
        CalcResultSummary summary,
        decimal sectionTotal,
        ByCountryApportionment apportionment,
        Action<decimal, decimal, decimal> setHeaders,
        Action<CalcResultSummaryProducerDisposalFees, CalcResultSummaryBadDebtProvision> setFee
    )
    {
        var badDebt = sectionTotal * badDebtRate / 100;
        setHeaders(sectionTotal, badDebt, sectionTotal + badDebt);

        foreach (var fee in summary.ProducerDisposalFees)
        {
            var without = fee.ProducerOverallPercentageOfCostsForOnePlus2A2B2C * sectionTotal / 100;
            var feeDebt = without * badDebtRate / 100;
            var apportioned =
                ApplyApportionment(
                    badDebtRate,
                    sectionTotal,
                    fee.ProducerOverallPercentageOfCostsForOnePlus2A2B2C,
                    apportionment
                );

            setFee(fee, new CalcResultSummaryBadDebtProvision
            {
                TotalProducerFeeWithoutBadDebtProvision  = without,
                BadDebtProvision                         = feeDebt,
                TotalProducerFeeWithBadDebtProvision     = without + feeDebt,
                EnglandTotalWithBadDebtProvision         = apportioned.England,
                WalesTotalWithBadDebtProvision           = apportioned.Wales,
                ScotlandTotalWithBadDebtProvision        = apportioned.Scotland,
                NorthernIrelandTotalWithBadDebtProvision = apportioned.NorthernIreland
            });
        }
    }

    // A producer's country-apportioned share of a section cost total, with bad debt applied.
    // Formula: sectionTotal × (1 + badDebt%) × producerPct% × countryApportionment%
    // Used by sections 3, 4, and 5 which share this calculation structure.
    public static ByCountryCost ApplyApportionment(
        decimal badDebt,
        decimal sectionTotal,
        decimal producerPct,
        ByCountryApportionment apportionment
    )
    {
        var factor =
            sectionTotal
                * (1 + (badDebt / 100))
                * (producerPct / 100)
                / 100;
        return new ByCountryCost
        {
            England          = factor * apportionment.England,
            Wales            = factor * apportionment.Wales,
            Scotland         = factor * apportionment.Scotland,
            NorthernIreland  = factor * apportionment.NorthernIreland
        };
    }
}
