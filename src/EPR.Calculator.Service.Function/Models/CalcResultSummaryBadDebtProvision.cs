using System.Collections.Generic;
using System.Linq;

namespace EPR.Calculator.Service.Function.Models;

public class CalcResultSummaryBadDebtProvision
{
    public decimal FeeWithoutBadDebtProvision { get; set; }

    public decimal BadDebtProvision { get; set; }

    public required ByCountryCost FeeWithBadDebtProvision { get; set; }

    public static readonly CalcResultSummaryBadDebtProvision Empty = new()
    {
        FeeWithoutBadDebtProvision = 0,
        BadDebtProvision           = 0,
        FeeWithBadDebtProvision    = ByCountryCost.Empty
    };

    public static CalcResultSummaryBadDebtProvision operator +(CalcResultSummaryBadDebtProvision a, CalcResultSummaryBadDebtProvision b) =>
        new()
        {
            FeeWithoutBadDebtProvision = a.FeeWithoutBadDebtProvision + b.FeeWithoutBadDebtProvision,
            BadDebtProvision           = a.BadDebtProvision           + b.BadDebtProvision,
            FeeWithBadDebtProvision    = a.FeeWithBadDebtProvision    + b.FeeWithBadDebtProvision,
        };
}

public static class CalcResultSummaryBadDebtProvisionExtensions
{
    public static CalcResultSummaryBadDebtProvision Sum(this IEnumerable<CalcResultSummaryBadDebtProvision> source) =>
        source.Aggregate(CalcResultSummaryBadDebtProvision.Empty, (acc, r) => acc + r);
}
