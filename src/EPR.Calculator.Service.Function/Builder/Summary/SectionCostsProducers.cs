using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary;

// Sections 3, 4, 5 are structurally identical: a fixed cost total is apportioned to producers
// by their ProducerOverallPercentage and country apportionment. The only differences between
// sections are the source of the total, which summary header fields to write, which fee property
// to assign, and which country-apportionment table to use.

public static class ThreeSaCostsProducer
{
    public static void SetValues(CalcResult calcResult, CalcResultSummary summary) =>
        SectionCosts.Apply(
            summary,
            badDebt:       calcResult.CalcResultParameterOtherCost.BadDebtValue,
            total:         calcResult.CalcResultParameterOtherCost.SaOperatingCost.Total,
            apportionment: calcResult.CalcResultOnePlusFourApportionment.OnePlusFourApportionment,
            setHeader: (s, p) => s.SaOperatingCostsSection3 = p, // gitleaks:allow
            setFee:    (f, p) => f.SaOperatingCostsSection3 = p
        );
}

public static class LaDataPrepCostsProducer
{
    public static void SetValues(CalcResult calcResult, CalcResultSummary summary) =>
        SectionCosts.Apply(
            summary,
            badDebt:       calcResult.CalcResultParameterOtherCost.BadDebtValue,
            total:         calcResult.CalcResultParameterOtherCost.LaDataPrepCharge?.Total ?? 0m,
            apportionment: calcResult.CalcResultParameterOtherCost.CountryApportionment,
            setHeader: (s, p) => s.LaDataPrepSection4 = p,
            setFee:    (f, p) => f.LaDataPrepSection4 = p
        );
}

public static class SaSetupCostsProducer
{
    public static void SetValues(CalcResult calcResult, CalcResultSummary summary) =>
        SectionCosts.Apply(
            summary,
            badDebt:       calcResult.CalcResultParameterOtherCost.BadDebtValue,
            total:         calcResult.CalcResultParameterOtherCost.SchemeSetupCost.Total,
            apportionment: calcResult.CalcResultOnePlusFourApportionment.OnePlusFourApportionment,
            setHeader: (s, p) => s.SaSetupCostsSection5 = p,
            setFee:    (f, p) => f.SaSetupCostsSection5 = p
        );
}

internal static class SectionCosts
{
    internal static void Apply(
        CalcResultSummary summary,
        decimal badDebt,
        decimal total,
        ByCountryApportionment apportionment,
        Action<CalcResultSummary, CalcResultSummaryBadDebtProvision> setHeader,
        Action<CalcResultSummaryProducerDisposalFees, CalcResultSummaryBadDebtProvision> setFee
    )
    {
        setHeader(summary, BadDebtProvision(badDebt, total, apportionment, 100m));
        foreach (var fee in summary.ProducerDisposalFees)
            setFee(fee, BadDebtProvision(badDebt, total, apportionment, fee.ProducerOverallPercentageOfCostsForOnePlus2A2B2C));
        if (summary.OverallTotal is not null)
            setFee(summary.OverallTotal, BadDebtProvision(badDebt, total, apportionment, summary.OverallTotal.ProducerOverallPercentageOfCostsForOnePlus2A2B2C));
    }

    internal static CalcResultSummaryBadDebtProvision BadDebtProvision(
        decimal badDebtRate,
        decimal sectionTotal,
        ByCountryApportionment apportionment,
        decimal producerPct
    )
    {
        var without = producerPct * sectionTotal / 100;
        return new CalcResultSummaryBadDebtProvision
        {
            FeeWithoutBadDebtProvision = without,
            BadDebtProvision           = without * badDebtRate / 100,
            FeeWithBadDebtProvision    = ApplyApportionment(badDebtRate, sectionTotal, producerPct, apportionment)
        };
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
