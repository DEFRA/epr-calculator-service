using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class LaDataPrepCostsProducerTests
{
    private readonly FeesState state = new()
    {
        OtherCost = new CalcResultParameterOtherCost
        {
            BadDebtValue         = 6m,
            LaDataPrepCharge     = new ByCountryCost { England = 40, Wales = 30, Scotland = 20, NorthernIreland = 10 },
            SaOperatingCost      = new ByCountryCost { England = 0,  Wales = 0,  Scotland = 0,  NorthernIreland = 0 },
            SchemeSetupCost      = new ByCountryCost { England = 40, Wales = 30, Scotland = 20, NorthernIreland = 10 },
            CountryApportionment = new ByCountryApportionment { England = 40, Wales = 30, Scotland = 20, NorthernIreland = 10 }
        },
        Materials = null!,
        Smcw = null!,
        CommsCost = null!,
        Apportionment = null!,
        DisposalCost = null!,
        Modulation = null,
        LapcapData = null!
    };

    private readonly ProducerFees producerFees = new()
    {
        CalculatorRunId = 0,
        Details = new List<ProducerFeeDetail>
        {
            new()
            {
                FeeDetail = new FeeDetail
                {
                    FeesByMaterial =
                        new Dictionary<string, Fees>(),
                    ProducerId = 1,
                    SubsidiaryId = "1",
                    ProducerName = "Test",
                    CommsCostsSection2c = new FeeWithBadDebt { ByCountry = new ByCountryCost { England = 10, Wales = 0, Scotland = 0, NorthernIreland = 0 } },
                    TotalOnePlus2A2B2CWithBadDebtPercentage = 100,
                    LaDataPrepSection4 = new FeeWithBadDebt
                    {
                        FeeWithoutBadDebt = 100,
                        BadDebt           = 20,
                        ByCountry    = new ByCountryCost { England = 20, Wales = 20, Scotland = 20, NorthernIreland = 20 }
                    },
                    BillingInstruction = new BillingInstruction
                    {
                        SuggestedBillingInstruction = string.Empty
                    }
                }
            }
        },
        Total = new() { ProducerId = 0, SubsidiaryId = string.Empty, ProducerName = string.Empty, TotalOnePlus2A2B2CWithBadDebtPercentage = 100 }
    };

    [TestMethod]
    public void LaDataPrepCostsProducer_CanCallSetValues()
    {
        // Act
        LaDataPrepCostsProducer.SetValues(state, producerFees);

        // Assert
        Assert.AreEqual(100   , producerFees.Total.LaDataPrepSection4.FeeWithoutBadDebt);
        Assert.AreEqual(6     , producerFees.Total.LaDataPrepSection4.BadDebt);
        Assert.AreEqual(106   , producerFees.Total.LaDataPrepSection4.ByCountry.Total);
        Assert.AreEqual(100   , producerFees.Details.ToList()[0].FeeDetail.LaDataPrepSection4.FeeWithoutBadDebt);
        Assert.AreEqual(6     , producerFees.Details.ToList()[0].FeeDetail.LaDataPrepSection4.BadDebt);
        Assert.AreEqual(106   , producerFees.Details.ToList()[0].FeeDetail.LaDataPrepSection4.ByCountry.Total);
        Assert.AreEqual(42.40m, producerFees.Details.ToList()[0].FeeDetail.LaDataPrepSection4.ByCountry.England);
        Assert.AreEqual(31.80m, producerFees.Details.ToList()[0].FeeDetail.LaDataPrepSection4.ByCountry.Wales);
        Assert.AreEqual(21.20m, producerFees.Details.ToList()[0].FeeDetail.LaDataPrepSection4.ByCountry.Scotland);
        Assert.AreEqual(10.60m, producerFees.Details.ToList()[0].FeeDetail.LaDataPrepSection4.ByCountry.NorthernIreland);
    }
}
