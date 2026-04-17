using EPR.Calculator.Service.Function.Features.Billing.FileExports.Json.Parts;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.UnitTests.Features.Billing.FileExports.Json.Parts;

[TestClass]
public class CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2bTests
{
    private readonly CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2B _testClass;

    public CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2bTests()
    {
        _testClass = new CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2B();
    }

    [TestMethod]
    public void CanSetAndGetTotalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision()
    {
        var testValue = TestFixtures.Default.Create<decimal>();
        var totalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(testValue);
        _testClass.TotalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision = totalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision;
        Assert.AreEqual(totalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision, _testClass.TotalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision);
    }

    [TestMethod]
    public void CanSetAndGetBadDebtProvisionFor2b()
    {
        var testValue = TestFixtures.Default.Create<decimal>();
        var badDebtProvisionFor2bComms = CurrencyConverterUtils.ConvertToCurrency(testValue);
        _testClass.BadDebtProvisionFor2b = badDebtProvisionFor2bComms;
        Assert.AreEqual(badDebtProvisionFor2bComms, _testClass.BadDebtProvisionFor2b);
    }

    [TestMethod]
    public void CanSetAndGetTotalProducerFeeForCommsCostsUKWideWithBadDebtProvision()
    {
        var testValue = TestFixtures.Default.Create<decimal>();
        var totalProducerFeeForCommsCostsUKWideWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(testValue);
        _testClass.TotalProducerFeeForCommsCostsUKWideWithBadDebtProvision = totalProducerFeeForCommsCostsUKWideWithBadDebtProvision;
        Assert.AreEqual(totalProducerFeeForCommsCostsUKWideWithBadDebtProvision, _testClass.TotalProducerFeeForCommsCostsUKWideWithBadDebtProvision);
    }

    [TestMethod]
    public void CanSetAndGetEnglandTotalWithBadDebtProvision()
    {
        var testValue = TestFixtures.Default.Create<decimal>();
        var englandTotalWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(testValue);
        _testClass.EnglandTotalWithBadDebtProvision = englandTotalWithBadDebtProvision;
        Assert.AreEqual(englandTotalWithBadDebtProvision, _testClass.EnglandTotalWithBadDebtProvision);
    }

    [TestMethod]
    public void CanSetAndGetWalesTotalWithBadDebtProvision()
    {
        var testValue = TestFixtures.Default.Create<decimal>();
        var walesTotalWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(testValue);
        _testClass.WalesTotalWithBadDebtProvision = walesTotalWithBadDebtProvision;
        Assert.AreEqual(walesTotalWithBadDebtProvision, _testClass.WalesTotalWithBadDebtProvision);
    }

    [TestMethod]
    public void CanSetAndGetScotlandTotalWithBadDebtProvision()
    {
        var testValue = TestFixtures.Default.Create<decimal>();
        var scotlandTotalWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(testValue);
        _testClass.ScotlandTotalWithBadDebtProvision = scotlandTotalWithBadDebtProvision;
        Assert.AreEqual(scotlandTotalWithBadDebtProvision, _testClass.ScotlandTotalWithBadDebtProvision);
    }

    [TestMethod]
    public void CanSetAndGetNorthernIrelandTotalWithBadDebtProvision()
    {
        var testValue = TestFixtures.Default.Create<decimal>();
        var northernIrelandTotalWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(testValue);
        _testClass.NorthernIrelandTotalWithBadDebtProvision = northernIrelandTotalWithBadDebtProvision;
        Assert.AreEqual(northernIrelandTotalWithBadDebtProvision, _testClass.NorthernIrelandTotalWithBadDebtProvision);
    }
}