using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class OnePlus2A2B2CProducerTests
{
    private readonly CalcResult calcResult = TestDataHelper.GetCalcResult();

    [TestMethod]
    public void OnePlus2A2B2CProducer_CanCallSetValues()
    {
        // Act
        OnePlus2A2B2CProducer.SetValues(calcResult.CalcResultSummary);

        // Assert
        Assert.AreEqual(8895.9148742165541m, calcResult.CalcResultSummary.TotalOnePlus2A2B2CFeeWithBadDebtProvision);
        Assert.AreEqual(10491.16776684412368m, calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].ProducerTotalOnePlus2A2B2CWithBadDeptProvision);
        Assert.AreEqual(117.93242083791927466960416708m, calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].ProducerOverallPercentageOfCostsForOnePlus2A2B2C);
    }

    [TestMethod]
    public void OnePlus2A2B2CProducer_CanCallSetValues_NullLocalAuthority()
    {
        // Act
        var data = calcResult.CalcResultSummary;
        data.ProducerDisposalFees.ToList()[0].LocalAuthorityDisposalCostsSectionOne = CalcResultSummaryBadDebtProvision.Empty;
        data.ProducerDisposalFees.ToList()[0].CommsCostsSectionTwoA = CalcResultSummaryBadDebtProvision.Empty;
        data.ProducerDisposalFees.ToList()[0].CommsCostsSectionTwoB = CalcResultSummaryBadDebtProvision.Empty;
        data.ProducerDisposalFees.ToList()[0].CommsCostsSectionTwoC = CalcResultSummaryBadDebtProvision.Empty;

        data.LocalAuthorityDisposalCostsSectionOne = CalcResultSummaryBadDebtProvision.Empty;
        data.CommsCostsSectionTwoA = CalcResultSummaryBadDebtProvision.Empty;
        data.CommsCostsHeaderFor2bTitle = CalcResultSummaryBadDebtProvision.Empty;
        data.TwoCCommsCosts = new CalcResultSummaryBadDebtProvision { FeeWithoutBadDebtProvision = 0, BadDebtProvision = 0m, FeeWithBadDebtProvision = ByCountryCost.Empty };

        OnePlus2A2B2CProducer.SetValues(data);

        // Assert
        Assert.AreEqual(0, data.TotalOnePlus2A2B2CFeeWithBadDebtProvision);
        Assert.AreEqual(0, data.ProducerDisposalFees.ToList()[0].ProducerTotalOnePlus2A2B2CWithBadDeptProvision);
        Assert.AreEqual(0, data.ProducerDisposalFees.ToList()[0].ProducerOverallPercentageOfCostsForOnePlus2A2B2C);
    }
}
