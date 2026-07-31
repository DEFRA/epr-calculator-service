using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class SaSetupCostsProducerTests
{
    private readonly FeesState state = new()
    {
        OtherCost = new CalcResultParameterOtherCost
        {
            BadDebtValue     = 6m,
            LaDataPrepCharge = new ByCountryCost { England = 40, Wales = 30, Scotland = 20, NorthernIreland = 10 },
            SaOperatingCost  = new ByCountryCost { England = 0,  Wales = 0,  Scotland = 0,  NorthernIreland = 0 },
            SchemeSetupCost  = new ByCountryCost { England = 40, Wales = 30, Scotland = 20, NorthernIreland = 10 }
        },
        Apportionment = TestDataHelper.GetCalcResultOnePlusFourApportionment(),
        Materials = null!,
        Smcw = null!,
        CommsCost = null!,
        DisposalCost = null!,
        Modulation = null,
        LapcapData = null!
    };

    private readonly ProducerFees producerFees = new ()
    {
        CalculatorRunId = 0,
        Details = new List<ProducerFeeDetail>
        {
            new()
            {
                FeeDetail = new FeeDetail
                {
                    FeesByMaterial = new Dictionary<string, Fees>(),
                    ProducerId = 1,
                    ProducerName = "Test",
                    SubsidiaryId = "1",
                    TotalOnePlus2A2B2CWithBadDebtPercentage = 1
                }
            }
        },
        Total = new() { ProducerId = 0, SubsidiaryId = string.Empty, ProducerName = string.Empty, TotalOnePlus2A2B2CWithBadDebtPercentage = 100 }
    };

    [TestMethod]
    public void SaSetupCostsProducer_CanCallSetValues()
    {
        // Act
        SaSetupCostsProducer.SetValues(state, producerFees);

        // Assert
        Assert.AreEqual(100    , producerFees.Total.SaSetupCostsSection5.FeeWithoutBadDebt);
        Assert.AreEqual(6      , producerFees.Total.SaSetupCostsSection5.BadDebt);
        Assert.AreEqual(106    , producerFees.Total.SaSetupCostsSection5.ByCountry.Total);
        Assert.AreEqual(1      , producerFees.Details.ToList()[0].FeeDetail.SaSetupCostsSection5.FeeWithoutBadDebt);
        Assert.AreEqual(0.06m  , producerFees.Details.ToList()[0].FeeDetail.SaSetupCostsSection5.BadDebt);
        Assert.AreEqual(1.06m  , producerFees.Details.ToList()[0].FeeDetail.SaSetupCostsSection5.ByCountry.Total);
        Assert.AreEqual(0.4240m, producerFees.Details.ToList()[0].FeeDetail.SaSetupCostsSection5.ByCountry.England);
        Assert.AreEqual(0.1060m, producerFees.Details.ToList()[0].FeeDetail.SaSetupCostsSection5.ByCountry.Wales);
        Assert.AreEqual(0.1590m, producerFees.Details.ToList()[0].FeeDetail.SaSetupCostsSection5.ByCountry.Scotland);
        Assert.AreEqual(0.3710m, producerFees.Details.ToList()[0].FeeDetail.SaSetupCostsSection5.ByCountry.NorthernIreland);
    }
}
