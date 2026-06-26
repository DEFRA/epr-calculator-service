using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class ThreeSaCostsProducerTests
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
            SaOperatingCost = new ByCountryCost { England = 40, Wales = 30, Scotland = 20, NorthernIreland = 10 },
            SchemeSetupCost = new ByCountryCost { England = 40, Wales = 30, Scotland = 20, NorthernIreland = 10 }
        },
        CalcResultDetail = new CalcResultDetail { RunId = 1, RelativeYear = new RelativeYear(2024) },
        CalcResultLaDisposalCostData = new CalcResultLaDisposalCostData
        {
            ByMaterial = new Dictionary<string, CalcResultLaDisposalCostDataDetail>
            {
                ["AL"] =
                    new()
                    {
                        Cost = ByCountryCost.Empty,
                        HouseholdPackagingWasteTonnage = 0,
                        PublicBinTonnage = 0,
                        HouseholdDrinkContainersTonnage = 0
                    },
                ["PL"] =
                    new()
                    {
                        Cost = ByCountryCost.Empty,
                        HouseholdPackagingWasteTonnage = 0,
                        PublicBinTonnage = 0,
                        HouseholdDrinkContainersTonnage = 0
                    }
            }
        },
        CalcResultLapcapData = new CalcResultLapcapData
        {
            ByMaterial = []
        },
        CalcResultOnePlusFourApportionment = TestDataHelper.GetCalcResultOnePlusFourApportionment(),
        CalcResultSummary = new CalcResultSummary
        {
            ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>
            {
                new()
                {
                    CalculatorRunId = 0,
                    ProducerFeesByMaterial = new Dictionary<string, CalcResultSummaryProducerFeesByMaterial>(),
                    ProducerId = 1,
                    ProducerName = "Test",
                    SubsidiaryId = "1",
                    ProducerOverallPercentageOfCostsForOnePlus2A2B2C = 1
                }
            }
        },
        CalcResultCommsCostReportDetail = TestDataHelper.GetCalcResultCommsCostReportDetail(),
        CalcResultLateReportingTonnageData = new CalcResultLateReportingTonnage { ByMaterial = [] },
        CalcResultProjectedProducers = new CalcResultProjectedProducers(){
            H1ProjectedProducers = ImmutableList<CalcResultH1ProjectedProducer>.Empty,
            H2ProjectedProducers = ImmutableList<CalcResultH2ProjectedProducer>.Empty,
        }
    };

    // [TestMethod]
    // public void ThreeSaCostsProducer_CanCallSetValues()
    // {
    //     // Act
    //     ThreeSaCostsProducer.SetValues(calcResult, calcResult.CalcResultSummary);

    //     // Assert
    //     Assert.AreEqual(100    , calcResult.CalcResultSummary.SaOperatingCostsSection3.FeeWithoutBadDebtProvision);
    //     Assert.AreEqual(6      , calcResult.CalcResultSummary.SaOperatingCostsSection3.BadDebtProvision);
    //     Assert.AreEqual(106    , calcResult.CalcResultSummary.SaOperatingCostsSection3.FeeWithBadDebtProvision.Total);
    //     Assert.AreEqual(1      , calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].SaOperatingCostsSection3!.FeeWithoutBadDebtProvision);
    //     Assert.AreEqual(0.06m  , calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].SaOperatingCostsSection3!.BadDebtProvision);
    //     Assert.AreEqual(1.06m  , calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].SaOperatingCostsSection3!.FeeWithBadDebtProvision.Total);
    //     Assert.AreEqual(0.4240m, calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].SaOperatingCostsSection3!.FeeWithBadDebtProvision.England);
    //     Assert.AreEqual(0.1060m, calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].SaOperatingCostsSection3!.FeeWithBadDebtProvision.Wales);
    //     Assert.AreEqual(0.1590m, calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].SaOperatingCostsSection3!.FeeWithBadDebtProvision.Scotland);
    //     Assert.AreEqual(0.3710m, calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].SaOperatingCostsSection3!.FeeWithBadDebtProvision.NorthernIreland);
    // }
}
