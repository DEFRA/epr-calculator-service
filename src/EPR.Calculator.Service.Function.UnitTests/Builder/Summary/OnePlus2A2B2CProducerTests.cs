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
        Assert.AreEqual(8895.914874216554m, calcResult.CalcResultSummary.TotalOnePlus2A2B2CFeeWithBadDebtProvision);
        Assert.AreEqual(10491.16776684412368m, calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].ProducerTotalOnePlus2A2B2CWithBadDeptProvision);
        Assert.AreEqual(117.93242083791927599529604369m, calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].ProducerOverallPercentageOfCostsForOnePlus2A2B2C);
    }

    [TestMethod]
    public void OnePlus2A2B2CProducer_CanCallSetValues_NullLocalAuthority()
    {
        // Act
        var data = calcResult.CalcResultSummary;
        data.ProducerDisposalFees.ToList()[0].LocalAuthorityDisposalCostsSectionOne = null;
        data.ProducerDisposalFees.ToList()[0].CommunicationCostsSectionTwoA = null;
        data.ProducerDisposalFees.ToList()[0].CommunicationCostsSectionTwoB = null;

        data.TotalFeeforLADisposalCostswithBadDebtprovision1 = 0;
        data.TotalFeeforCommsCostsbyMaterialwithBadDebtprovision2A = 0;
        data.CommsCostsHeaderFor2bTitle = CalcResultSummaryBadDebtProvision.Empty;
        data.TwoCCommsCosts = new CalcResultSummaryBadDebtProvision { FeeWithoutBadDebtProvision = 0, BadDebtProvision = 0m, FeeWithBadDebtProvision = ByCountryCost.Empty };

        OnePlus2A2B2CProducer.SetValues(data);

        // Assert
        Assert.AreEqual(0, data.TotalOnePlus2A2B2CFeeWithBadDebtProvision);
        Assert.AreEqual(0, data.ProducerDisposalFees.ToList()[0].ProducerTotalOnePlus2A2B2CWithBadDeptProvision);
        Assert.AreEqual(0, data.ProducerDisposalFees.ToList()[0].ProducerOverallPercentageOfCostsForOnePlus2A2B2C);
    }
}
