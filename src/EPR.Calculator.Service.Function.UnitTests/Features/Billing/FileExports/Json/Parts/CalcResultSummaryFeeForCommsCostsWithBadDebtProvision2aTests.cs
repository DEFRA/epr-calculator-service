using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Features.Billing.FileExports.Json.Parts;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.UnitTests.Features.Billing.FileExports.Json.Parts;

[TestClass]
public class CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2aTests
{
    private CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2A _testClass = null!;

    [TestInitialize]
    public void SetUp()
    {
        _testClass = new CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2A();
    }

    [TestMethod]
    public void CanSetAndGetTotalProducerFeeForCommsCostsWithoutBadDebtProvision()
    {
        // Arrange
        var testValue = TestFixtures.Default.Create<decimal>();
        var totalProducerFeeForCommsCostsWithoutBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(testValue);
        // Act
        _testClass.TotalProducerFeeForCommsCostsWithoutBadDebtProvision = totalProducerFeeForCommsCostsWithoutBadDebtProvision;

        // Assert
        Assert.AreEqual(totalProducerFeeForCommsCostsWithoutBadDebtProvision, _testClass.TotalProducerFeeForCommsCostsWithoutBadDebtProvision);
    }

    [TestMethod]
    public void CanSetAndGetBadDebtProvisionFor2a()
    {
        // Arrange
        var testValue = TestFixtures.Default.Create<decimal>();
        var badDebtProvisionFor2a = CurrencyConverterUtils.ConvertToCurrency(testValue);

        // Act
        _testClass.BadDebtProvisionFor2a = badDebtProvisionFor2a;

        // Assert
        Assert.AreEqual(badDebtProvisionFor2a, _testClass.BadDebtProvisionFor2a);
    }

    [TestMethod]
    public void CanSetAndGetTotalProducerFeeForCommsCostsWithBadDebtProvision()
    {
        // Arrange
        var testValue = TestFixtures.Default.Create<decimal>();
        var totalProducerFeeForCommsCostsWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(testValue);

        // Act
        _testClass.TotalProducerFeeForCommsCostsWithBadDebtProvision = totalProducerFeeForCommsCostsWithBadDebtProvision;

        // Assert
        Assert.AreEqual(totalProducerFeeForCommsCostsWithBadDebtProvision, _testClass.TotalProducerFeeForCommsCostsWithBadDebtProvision);
    }

    [TestMethod]
    public void CanSetAndGetEnglandTotalWithBadDebtProvision()
    {
        // Arrange
        var testValue = TestFixtures.Default.Create<decimal>();
        var englandTotalWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(testValue);

        // Act
        _testClass.EnglandTotalWithBadDebtProvision = englandTotalWithBadDebtProvision;

        // Assert
        Assert.AreEqual(englandTotalWithBadDebtProvision, _testClass.EnglandTotalWithBadDebtProvision);
    }

    [TestMethod]
    public void CanSetAndGetWalesTotalWithBadDebtProvision()
    {
        // Arrange
        var testValue = TestFixtures.Default.Create<decimal>();
        var walesTotalWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(testValue);

        // Act
        _testClass.WalesTotalWithBadDebtProvision = walesTotalWithBadDebtProvision;

        // Assert
        Assert.AreEqual(walesTotalWithBadDebtProvision, _testClass.WalesTotalWithBadDebtProvision);
    }

    [TestMethod]
    public void CanSetAndGetScotlandTotalWithBadDebtProvision()
    {
        // Arrange
        var testValue = TestFixtures.Default.Create<decimal>();
        var scotlandTotalWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(testValue);

        // Act
        _testClass.ScotlandTotalWithBadDebtProvision = scotlandTotalWithBadDebtProvision;

        // Assert
        Assert.AreEqual(scotlandTotalWithBadDebtProvision, _testClass.ScotlandTotalWithBadDebtProvision);
    }

    [TestMethod]
    public void CanSetAndGetNorthernIrelandTotalWithBadDebtProvision()
    {
        // Arrange
        var testValue = TestFixtures.Default.Create<decimal>();
        var northernIrelandTotalWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(testValue);

        // Act
        _testClass.NorthernIrelandTotalWithBadDebtProvision = northernIrelandTotalWithBadDebtProvision;

        // Assert
        Assert.AreEqual(northernIrelandTotalWithBadDebtProvision, _testClass.NorthernIrelandTotalWithBadDebtProvision);
    }

    [TestMethod]
    public void CanSetAndGetPercentageOfProducerTonnageVsAllProducers()
    {
        // Arrange
        var testValue = TestFixtures.Default.Create<decimal>();
        var percentageOfProducerTonnageVsAllProducers = CsvSanitiser.SanitiseData(testValue, DecimalPlaces.Eight, null, false, true);

        // Act
        _testClass.PercentageOfProducerTonnageVsAllProducers = percentageOfProducerTonnageVsAllProducers;

        // Assert
        Assert.AreEqual(percentageOfProducerTonnageVsAllProducers, _testClass.PercentageOfProducerTonnageVsAllProducers);
    }
}