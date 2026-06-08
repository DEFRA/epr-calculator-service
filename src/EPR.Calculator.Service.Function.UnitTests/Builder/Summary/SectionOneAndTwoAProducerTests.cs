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
        LADisposalCostsSection1 = new CalcResultSummaryBadDebtProvision
        {
          FeeWithoutBadDebtProvision = 4423.39438m,
          BadDebtProvision           = 265.4036628m,
          FeeWithBadDebtProvision = new ByCountryCost { England = 4688.7980428m, Wales = 0, Scotland = 0, NorthernIreland = 0 }
        },
        CommsCostsSection2a = new CalcResultSummaryBadDebtProvision
        {
            FeeWithoutBadDebtProvision = 1290.778m,
            BadDebtProvision           = 77.44668m,
            FeeWithBadDebtProvision    = new ByCountryCost { England = 1368.22468m, Wales = 0, Scotland = 0, NorthernIreland = 0 }
        },
        CommsCostsSection2c = CalcResultSummaryBadDebtProvision.Empty,
    };

    [TestMethod]
    public void SectionOneAndTwoAProducer_CanCallSetValues()
    {
        var summary = TestDataHelper.GetCalcResultSummary();

        SectionOneAndTwoAProducer.SetValues(TotalsRow, summary);

        Assert.AreEqual(4423.39438m,   summary.LADisposalCostsSection1.FeeWithoutBadDebtProvision);
        Assert.AreEqual(265.4036628m,  summary.LADisposalCostsSection1.BadDebtProvision);
        Assert.AreEqual(4688.7980428m, summary.LADisposalCostsSection1.FeeWithBadDebtProvision.Total);
        Assert.AreEqual(1290.778m,     summary.CommsCostsSection2a.FeeWithoutBadDebtProvision);
        Assert.AreEqual(77.44668m,     summary.CommsCostsSection2a.BadDebtProvision);
        Assert.AreEqual(1368.22468m,   summary.CommsCostsSection2a.FeeWithBadDebtProvision.Total);
    }
}
