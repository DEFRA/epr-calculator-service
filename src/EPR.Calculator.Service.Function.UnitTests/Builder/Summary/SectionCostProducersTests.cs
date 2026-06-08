using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Builder.Summary.Common;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary.Common;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class SectionCostsProducersTests
{
    private readonly CalcResult calcResult;

    public SectionCostsProducersTests()
    {
        calcResult = TestDataHelper.GetCalcResult();
    }

[TestMethod]
    public void CanGetCountryOnePlusFourApportionmentForEngland()
    {
        // Act
        var result = SectionCosts.GetCountryOnePlusFourApportionment(calcResult, Countries.England);

        // Assert
        Assert.AreEqual(40m, result);
    }

    [TestMethod]
    public void CanGetCountryOnePlusFourApportionmentForWales()
    {
        // Act
        var result = SectionCosts.GetCountryOnePlusFourApportionment(calcResult, Countries.Wales);

        // Assert
        Assert.AreEqual(10m, result);
    }

    [TestMethod]
    public void CanGetCountryOnePlusFourApportionmentForScotland()
    {
        // Act
        var result = SectionCosts.GetCountryOnePlusFourApportionment(calcResult, Countries.Scotland);

        // Assert
        Assert.AreEqual(15m, result);
    }

    [TestMethod]
    public void CanGetCountryOnePlusFourApportionmentForNorthernIreland()
    {
        // Act
        var result = SectionCosts.GetCountryOnePlusFourApportionment(calcResult, Countries.NorthernIreland);

        // Assert
        Assert.AreEqual(35m, result);
    }

    [TestMethod]
    public void CanGetDefaultOnePlusFourApportionment()
    {
        // Act
        var result = SectionCosts.GetCountryOnePlusFourApportionment(calcResult, (Countries)(-1));

        // Assert
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void GetParamsOtherFourCountryApportionmentPercentageForEngland()
    {
        // Act
        var result = LaDataPrepCostsProducer.GetParamsOtherFourCountryApportionmentPercentage(calcResult, Countries.England);

        // Assert
        Assert.AreEqual(43.83561643835616m, result);
    }

    [TestMethod]
    public void GetParamsOtherFourCountryApportionmentPercentageForWales()
    {
        // Act
        var result = LaDataPrepCostsProducer.GetParamsOtherFourCountryApportionmentPercentage(calcResult, Countries.Wales);

        // Assert
        Assert.AreEqual(19.17808219178082m, result);
    }

    [TestMethod]
    public void GetParamsOtherFourCountryApportionmentPercentageForScotland()
    {
        // Act
        var result = LaDataPrepCostsProducer.GetParamsOtherFourCountryApportionmentPercentage(calcResult, Countries.Scotland);

        // Assert
        Assert.AreEqual(24.65753424657534m, result);
    }

    [TestMethod]
    public void GetParamsOtherFourCountryApportionmentPercentageForNorthernIreland()
    {
        // Act
        var result = LaDataPrepCostsProducer.GetParamsOtherFourCountryApportionmentPercentage(calcResult, Countries.NorthernIreland);

        // Assert
        Assert.AreEqual(12.32876712328767m, result);
    }
}
