using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class TwoBCommsCostProducerTests
{
    private readonly FeesState state = new ()
    {
        OtherCost = TestDataHelper.GetCalcResultParameterOtherCost(),
        CommsCost = TestDataHelper.GetCalcResultCommsCostReportDetail(),
        Materials = null!,
        Smcw = null!,
        Apportionment = null!,
        DisposalCost = null!,
        Modulation = null,
        LapcapData = null!
    };

    private readonly ProducerFees producerFees = TestDataHelper.GetProducerFees();

    [TestMethod]
    public void TwoBCommsCostProducer_CanCallSetValues()
    {
        // Act
        TwoBCommsCostProducer.SetValues(state, producerFees);

        // Assert
        Assert.AreEqual(2531m   , producerFees.Total.CommsCostsSection2b.FeeWithoutBadDebt);
        Assert.AreEqual(151.86m , producerFees.Total.CommsCostsSection2b.BadDebt);
        Assert.AreEqual(2682.86m, producerFees.Total.CommsCostsSection2b.ByCountry.Total);
    }
}
