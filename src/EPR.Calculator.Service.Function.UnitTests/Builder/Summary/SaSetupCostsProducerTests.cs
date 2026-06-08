using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class SaSetupCostsProducerTests
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
            SaOperatingCost = new ByCountryCost { England = 0, Wales = 0, Scotland = 0, NorthernIreland = 0 },
            SchemeSetupCost = new ByCountryCost { England = 40, Wales = 30, Scotland = 20, NorthernIreland = 10 }
        },
        CalcResultDetail = new CalcResultDetail { RunId = 1, RelativeYear = new RelativeYear(2024) },
        CalcResultLaDisposalCostData = new CalcResultLaDisposalCostData
        {
            ByMaterial = new Dictionary<string, CalcResultLaDisposalCostDataDetail>
            {
                ["Material1"] =
                    new()
                    {
                        Cost = ByCountryCost.Empty,
                        HouseholdPackagingWasteTonnage = 33m,
                        PublicBinTonnage = 66m,
                        HouseholdDrinkContainersTonnage = 0
                    },
                ["Material2"] =
                    new()
                    {
                        Cost = ByCountryCost.Empty,
                        HouseholdPackagingWasteTonnage = 133m,
                        PublicBinTonnage = 166m,
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
                    ProducerCommsFeesByMaterial = new Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial>(),
                    ProducerDisposalFeesByMaterial = new Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial>(),
                    ProducerId = "1",
                    ProducerName = "Test",
                    TotalProducerDisposalFeeWithBadDebtProvision = 100,
                    TotalProducerCommsFeeWithBadDebtProvision = 100,
                    SubsidiaryId = "1",
                    ProducerOverallPercentageOfCostsForOnePlus2A2B2C = 1,
                    TwoCTotalProducerFeeForCommsCostsWithBadDebt = ByCountryCost.Empty
                }
            }
        },
        CalcResultCommsCostReportDetail = TestDataHelper.GetCalcResultCommsCostReportDetail(),
        CalcResultLateReportingTonnageData = TestDataHelper.GetCalcResultLateReportingTonnage(),
        CalcResultProjectedProducers = new CalcResultProjectedProducers(){
            H1ProjectedProducers = ImmutableList<CalcResultH1ProjectedProducer>.Empty,
            H2ProjectedProducers = ImmutableList<CalcResultH2ProjectedProducer>.Empty,
        }
    };

    [TestMethod]
    public void SaSetupCostsProducer_CanCallSetValues()
    {
        // Act
        SaSetupCostsProducer.SetValues(calcResult, calcResult.CalcResultSummary);

        // Assert
        Assert.AreEqual(100    , calcResult.CalcResultSummary.SaSetupCostsTitleSection5);
        Assert.AreEqual(6      , calcResult.CalcResultSummary.SaSetupCostsBadDebtProvisionTitleSection5);
        Assert.AreEqual(106    , calcResult.CalcResultSummary.SaSetupCostsWithBadDebtProvisionTitleSection5);
        Assert.AreEqual(1      , calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].OneOffSchemeAdministrationSetupCosts!.TotalProducerFeeWithoutBadDebtProvision);
        Assert.AreEqual(0.06m  , calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].OneOffSchemeAdministrationSetupCosts!.BadDebtProvision);
        Assert.AreEqual(1.06m  , calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].OneOffSchemeAdministrationSetupCosts!.TotalProducerFeeWithBadDebtProvision.Total);
        Assert.AreEqual(0.4240m, calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].OneOffSchemeAdministrationSetupCosts!.TotalProducerFeeWithBadDebtProvision.England);
        Assert.AreEqual(0.1060m, calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].OneOffSchemeAdministrationSetupCosts!.TotalProducerFeeWithBadDebtProvision.Wales);
        Assert.AreEqual(0.1590m, calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].OneOffSchemeAdministrationSetupCosts!.TotalProducerFeeWithBadDebtProvision.Scotland);
        Assert.AreEqual(0.3710m, calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].OneOffSchemeAdministrationSetupCosts!.TotalProducerFeeWithBadDebtProvision.NorthernIreland);
    }
}
