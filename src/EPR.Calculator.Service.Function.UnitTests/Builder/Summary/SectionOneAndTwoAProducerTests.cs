using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class SectionOneAndTwoAProducerTests
{
    private static readonly CalcResultSummaryProducerDisposalFees TotalsRow = new()
    {
        ProducerId   = string.Empty,
        SubsidiaryId = string.Empty,
        ProducerName = string.Empty,
        TotalProducerDisposalFee             = 4423.39438m,
        TotalProducerDisposalFeeWithBadDebtProvision = 4688.7980428m,
        LocalAuthorityDisposalCostsSectionOne = new CalcResultSummaryBadDebtProvision
        {
            BadDebtProvision = 265.4036628m,
        },
        TotalProducerCommsFee                        = 1290.778m,
        TotalProducerCommsFeeWithBadDebtProvision     = 1368.22468m,
        CommunicationCostsSectionTwoA = new CalcResultSummaryBadDebtProvision
        {
            BadDebtProvision = 77.44668m,
        },
    };

    [TestMethod]
    public void SectionOneAndTwoAProducer_CanCallSetValues()
    {
        var summary = TestDataHelper.GetCalcResultSummary();

        SectionOneAndTwoAProducer.SetValues(TotalsRow, summary);

        Assert.AreEqual(4423.39438m,   summary.TotalFeeforLADisposalCostswoBadDebtprovision1);
        Assert.AreEqual(265.4036628m,  summary.BadDebtProvisionFor1);
        Assert.AreEqual(4688.7980428m, summary.TotalFeeforLADisposalCostswithBadDebtprovision1);
        Assert.AreEqual(1290.778m,     summary.TotalFeeforCommsCostsbyMaterialwoBadDebtProvision2A);
        Assert.AreEqual(77.44668m,     summary.BadDebtProvisionFor2A);
        Assert.AreEqual(1368.22468m,   summary.TotalFeeforCommsCostsbyMaterialwithBadDebtprovision2A);
    }
}
