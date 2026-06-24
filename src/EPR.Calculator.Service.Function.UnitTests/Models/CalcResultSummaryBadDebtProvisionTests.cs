using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.UnitTests.Models;

[TestClass]
public class CalcResultSummaryBadDebtProvisionTests
{
    private static ByCountryCost MakeCountryCost(decimal england, decimal wales, decimal scotland, decimal ni) =>
        new() { England = england, Wales = wales, Scotland = scotland, NorthernIreland = ni };

    private static CalcResultSummaryBadDebtProvision Make(
        decimal feeWithout, decimal badDebt, decimal england, decimal wales, decimal scotland, decimal ni) =>
        new()
        {
            FeeWithoutBadDebtProvision = feeWithout,
            BadDebtProvision           = badDebt,
            FeeWithBadDebtProvision    = MakeCountryCost(england, wales, scotland, ni),
        };

    [TestMethod]
    public void Addition_SumsAllFields()
    {
        var a = Make(feeWithout: 10, badDebt: 2, england: 1, wales: 2, scotland: 3, ni: 4);
        var b = Make(feeWithout: 20, badDebt: 3, england: 10, wales: 20, scotland: 30, ni: 40);

        var result = a + b;

        result.FeeWithoutBadDebtProvision.ShouldBe(30);
        result.BadDebtProvision.ShouldBe(5);
        result.FeeWithBadDebtProvision.England.ShouldBe(11);
        result.FeeWithBadDebtProvision.Wales.ShouldBe(22);
        result.FeeWithBadDebtProvision.Scotland.ShouldBe(33);
        result.FeeWithBadDebtProvision.NorthernIreland.ShouldBe(44);
    }

    [TestMethod]
    public void Addition_WithEmpty_ReturnsOriginal()
    {
        var a = Make(feeWithout: 10, badDebt: 2, england: 1, wales: 2, scotland: 3, ni: 4);

        var result = a + CalcResultSummaryBadDebtProvision.Empty;

        result.FeeWithoutBadDebtProvision.ShouldBe(a.FeeWithoutBadDebtProvision);
        result.BadDebtProvision.ShouldBe(a.BadDebtProvision);
        result.FeeWithBadDebtProvision.England.ShouldBe(a.FeeWithBadDebtProvision.England);
        result.FeeWithBadDebtProvision.Wales.ShouldBe(a.FeeWithBadDebtProvision.Wales);
        result.FeeWithBadDebtProvision.Scotland.ShouldBe(a.FeeWithBadDebtProvision.Scotland);
        result.FeeWithBadDebtProvision.NorthernIreland.ShouldBe(a.FeeWithBadDebtProvision.NorthernIreland);
    }

    [TestMethod]
    public void Sum_AggregatesTotalAllFields()
    {
        var items = new[]
        {
            Make(feeWithout: 1,   badDebt: 1, england: 1,   wales: 1,   scotland: 1,   ni: 1),
            Make(feeWithout: 10,  badDebt: 2, england: 10,  wales: 10,  scotland: 10,  ni: 10),
            Make(feeWithout: 100, badDebt: 3, england: 100, wales: 100, scotland: 100, ni: 100),
        };

        var result = items.Sum();

        result.FeeWithoutBadDebtProvision.ShouldBe(111);
        result.BadDebtProvision.ShouldBe(6);
        result.FeeWithBadDebtProvision.England.ShouldBe(111);
        result.FeeWithBadDebtProvision.Wales.ShouldBe(111);
        result.FeeWithBadDebtProvision.Scotland.ShouldBe(111);
        result.FeeWithBadDebtProvision.NorthernIreland.ShouldBe(111);
    }

    [TestMethod]
    public void Sum_EmptyCollection_ReturnsEmpty()
    {
        var result = Array.Empty<CalcResultSummaryBadDebtProvision>().Sum();

        result.FeeWithoutBadDebtProvision.ShouldBe(0);
        result.BadDebtProvision.ShouldBe(0);
        result.FeeWithBadDebtProvision.England.ShouldBe(0);
        result.FeeWithBadDebtProvision.Wales.ShouldBe(0);
        result.FeeWithBadDebtProvision.Scotland.ShouldBe(0);
        result.FeeWithBadDebtProvision.NorthernIreland.ShouldBe(0);
    }
}
