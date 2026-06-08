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
}
