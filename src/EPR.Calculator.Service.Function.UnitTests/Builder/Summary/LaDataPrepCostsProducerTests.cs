using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class LaDataPrepCostsProducerTests
{
    private readonly CalcResult calcResult = new()
    {
        CalcResultScaledupProducers = new CalcResultScaledupProducers(){
            ScaledupProducers = ImmutableList<CalcResultScaledupProducer>.Empty
        },
        CalcResultPartialObligations = new CalcResultPartialObligations(){
            PartialObligations = ImmutableList<CalcResultPartialObligation>.Empty,
        },
        CalcResultParameterOtherCost = new CalcResultParameterOtherCost
        {
            BadDebtValue = 6m,
            LaDataPrepCharge = new ByCountryCost { England = 40, Wales = 30, Scotland = 20, NorthernIreland = 10 },
            CountryApportionment = new ByCountryApportionment { England = 40, Wales = 30, Scotland = 20, NorthernIreland = 10 },
            SaOperatingCost = new ByCountryCost { England = 0, Wales = 0, Scotland = 0, NorthernIreland = 0 },
            SchemeSetupCost = new ByCountryCost { England = 40, Wales = 30, Scotland = 20, NorthernIreland = 10 }
        },
        CalcResultDetail = new CalcResultDetail { RunId = 1, RelativeYear = new RelativeYear(2024) },
        CalcResultLaDisposalCostData = new CalcResultLaDisposalCostData { ByMaterial = [] },
        CalcResultLapcapData = new CalcResultLapcapData { ByMaterial = [] },
        CalcResultOnePlusFourApportionment = new CalcResultOnePlusFourApportionment
        {
            LaDisposalCost = new ByCountryCost { England = 0.10M, Wales = 20M, Scotland = 0.15M, NorthernIreland = 0.15M },
            LADataPrepCharge = new ByCountryCost { England = 0.10M, Wales = 20M, Scotland = 0.15M, NorthernIreland = 0.15M }
        },
        CalcResultSummary = new CalcResultSummary
        {
            ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>
            {
                new()
                {
                    ProducerCommsFeesByMaterial =
                        new Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial>(),
                    ProducerDisposalFeesByMaterial =
                        new Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial>(),
                    ProducerId = "1",
                    ProducerName = "Test",
                    TotalProducerCommsFeeWithBadDebtProvision = ByCountryCost.Empty,
                    SubsidiaryId = "1",
                    TwoCTotalProducerFeeForCommsCostsWithBadDebt = new ByCountryCost { England = 10, Wales = 0, Scotland = 0, NorthernIreland = 0 },
                    ProducerOverallPercentageOfCostsForOnePlus2A2B2C = 100,
                    LocalAuthorityDataPreparationCosts = new CalcResultSummaryBadDebtProvision
                    {
                        FeeWithoutBadDebtProvision = 100,
                        BadDebtProvision           = 20,
                        FeeWithBadDebtProvision    = new ByCountryCost { England = 20, Wales = 20, Scotland = 20, NorthernIreland = 20 }
                    },
                    BillingInstructionSection = new CalcResultSummaryBillingInstruction
                    {
                        SuggestedBillingInstruction = string.Empty
                    }
                }
            },
            TotalFeeforLADisposalCostswithBadDebtprovision1 = 100,
            TotalFeeforCommsCostsbyMaterialwithBadDebtprovision2A = 100,
            CommsCostsHeaderFor2bTitle = new CalcResultSummaryBadDebtProvision { FeeWithBadDebtProvision = ByCountryCost.Empty with { England = 100 }},
            TwoCCommsCosts = new CalcResultSummaryBadDebtProvision { FeeWithoutBadDebtProvision = 0, BadDebtProvision = 0, FeeWithBadDebtProvision = ByCountryCost.Empty with { England = 100 } }
        },
        CalcResultCommsCostReportDetail = TestDataHelper.GetCalcResultCommsCostReportDetail(),
        CalcResultLateReportingTonnageData = TestDataHelper.GetCalcResultLateReportingTonnage(),
        CalcResultProjectedProducers = new CalcResultProjectedProducers(){
            H1ProjectedProducers = ImmutableList<CalcResultH1ProjectedProducer>.Empty,
            H2ProjectedProducers = ImmutableList<CalcResultH2ProjectedProducer>.Empty,
        }
    };

    [TestMethod]
    public void LaDataPrepCostsProducer_CanCallSetValues()
    {
        // Act
        LaDataPrepCostsProducer.SetValues(calcResult, calcResult.CalcResultSummary);

        // Assert
        Assert.AreEqual(100   , calcResult.CalcResultSummary.LaDataPrepCostsTitleSection4);
        Assert.AreEqual(6     , calcResult.CalcResultSummary.LaDataPrepCostsBadDebtProvisionTitleSection4);
        Assert.AreEqual(106   , calcResult.CalcResultSummary.LaDataPrepCostsWithBadDebtProvisionTitleSection4);
        Assert.AreEqual(100   , calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].LocalAuthorityDataPreparationCosts!.FeeWithoutBadDebtProvision);
        Assert.AreEqual(6     , calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].LocalAuthorityDataPreparationCosts!.BadDebtProvision);
        Assert.AreEqual(106   , calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].LocalAuthorityDataPreparationCosts!.FeeWithBadDebtProvision.Total);
        Assert.AreEqual(42.40m, calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].LocalAuthorityDataPreparationCosts!.FeeWithBadDebtProvision.England);
        Assert.AreEqual(31.80m, calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].LocalAuthorityDataPreparationCosts!.FeeWithBadDebtProvision.Wales);
        Assert.AreEqual(21.20m, calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].LocalAuthorityDataPreparationCosts!.FeeWithBadDebtProvision.Scotland);
        Assert.AreEqual(10.60m, calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].LocalAuthorityDataPreparationCosts!.FeeWithBadDebtProvision.NorthernIreland);
    }
}
