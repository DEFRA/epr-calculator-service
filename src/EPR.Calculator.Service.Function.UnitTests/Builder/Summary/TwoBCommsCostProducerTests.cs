using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class TwoBCommsCostProducerTests
{
    private readonly CalcResult calcResult = TestDataHelper.GetCalcResult();

    [TestMethod]
    public void TwoBCommsCostProducer_CanCallSetValues()
    {
        // Act
        TwoBCommsCostProducer.SetValues(calcResult, calcResult.CalcResultSummary);

        // Assert
        Assert.AreEqual(2531m   , calcResult.CalcResultSummary.CommsCostsHeaderFor2bTitle.FeeWithoutBadDebtProvision);
        Assert.AreEqual(151.86m , calcResult.CalcResultSummary.CommsCostsHeaderFor2bTitle.BadDebtProvision);
        Assert.AreEqual(2682.86m, calcResult.CalcResultSummary.CommsCostsHeaderFor2bTitle.FeeWithBadDebtProvision.Total);
    }
}
