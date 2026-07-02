using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;
using EPR.Calculator.API.Data.DataModels;

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
            CalculatorRunId = 0,
            ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>
            {
                new()
                {
                    ProducerFeesByMaterial =
                        new Dictionary<string, CalcResultSummaryProducerFeesByMaterial>(),
                    ProducerId = 1,
                    SubsidiaryId = "1",
                    ProducerName = "Test",
                    CommsCostsSection2c = new CalcResultSummaryBadDebtProvision { FeeWithBadDebtProvision = new ByCountryCost { England = 10, Wales = 0, Scotland = 0, NorthernIreland = 0 } },
                    ProducerOverallPercentageOfCostsForOnePlus2A2B2C = 100,
                    LaDataPrepSection4 = new CalcResultSummaryBadDebtProvision
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
            // LADisposalCostsSection1 = new CalcResultSummaryBadDebtProvision { FeeWithBadDebtProvision = ByCountryCost.Empty with { England = 100 } },
            // CommsCostsSection2a = new CalcResultSummaryBadDebtProvision { FeeWithBadDebtProvision = ByCountryCost.Empty with { England = 100 } },
            // CommsCostsSection2b = new CalcResultSummaryBadDebtProvision { FeeWithBadDebtProvision = ByCountryCost.Empty with { England = 100 }},
            // CommsCostsSection2c = new CalcResultSummaryBadDebtProvision { FeeWithoutBadDebtProvision = 0, BadDebtProvision = 0, FeeWithBadDebtProvision = ByCountryCost.Empty with { England = 100 } }
            OverallTotal = new() { ProducerId = 0, SubsidiaryId = string.Empty, ProducerName = string.Empty }
        },
        CalcResultCommsCostReportDetail = TestDataHelper.GetCalcResultCommsCostReportDetail(),
        CalcResultLateReportingTonnageData = TestDataHelper.GetCalcResultLateReportingTonnage(),
        CalcResultProjectedProducers = new CalcResultProjectedProducers(){
            H1ProjectedProducers = ImmutableList<CalcResultH1ProjectedProducer>.Empty,
            H2ProjectedProducers = ImmutableList<CalcResultH2ProjectedProducer>.Empty,
        }
    };

    // [TestMethod]
    // public void LaDataPrepCostsProducer_CanCallSetValues()
    // {
    //     // Act
    //     LaDataPrepCostsProducer.SetValues(calcResult, calcResult.CalcResultSummary);

    //     // Assert
    //     Assert.AreEqual(100   , calcResult.CalcResultSummary.LaDataPrepSection4.FeeWithoutBadDebtProvision);
    //     Assert.AreEqual(6     , calcResult.CalcResultSummary.LaDataPrepSection4.BadDebtProvision);
    //     Assert.AreEqual(106   , calcResult.CalcResultSummary.LaDataPrepSection4.FeeWithBadDebtProvision.Total);
    //     Assert.AreEqual(100   , calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].LaDataPrepSection4!.FeeWithoutBadDebtProvision);
    //     Assert.AreEqual(6     , calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].LaDataPrepSection4!.BadDebtProvision);
    //     Assert.AreEqual(106   , calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].LaDataPrepSection4!.FeeWithBadDebtProvision.Total);
    //     Assert.AreEqual(42.40m, calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].LaDataPrepSection4!.FeeWithBadDebtProvision.England);
    //     Assert.AreEqual(31.80m, calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].LaDataPrepSection4!.FeeWithBadDebtProvision.Wales);
    //     Assert.AreEqual(21.20m, calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].LaDataPrepSection4!.FeeWithBadDebtProvision.Scotland);
    //     Assert.AreEqual(10.60m, calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].LaDataPrepSection4!.FeeWithBadDebtProvision.NorthernIreland);
    // }
}
