using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Builder.Summary.LaDataPrepCosts;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary.LaDataPrepCosts;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class LaDataPrepCostsProducerTests
{
    private readonly CalcResult calcResult = new()
    {
        CalcResultScaledupProducers = new CalcResultScaledupProducers(),
        CalcResultPartialObligations = new CalcResultPartialObligations(),
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
                    TotalProducerDisposalFeeWithBadDebtProvision = 100,
                    TotalProducerCommsFeeWithBadDebtProvision = 100,
                    SubsidiaryId = "1",
                    TwoCTotalProducerFeeForCommsCostsWithBadDebt = 10,
                    ProducerOverallPercentageOfCostsForOnePlus2A2B2C = 100,
                    LocalAuthorityDataPreparationCosts = new CalcResultSummaryBadDebtProvision
                    {
                        TotalProducerFeeWithoutBadDebtProvision = 100,
                        BadDebtProvision = 20,
                        TotalProducerFeeWithBadDebtProvision = 120,
                        EnglandTotalWithBadDebtProvision = 20,
                        WalesTotalWithBadDebtProvision = 20,
                        ScotlandTotalWithBadDebtProvision = 20,
                        NorthernIrelandTotalWithBadDebtProvision = 20
                    },
                    BillingInstructionSection = new CalcResultSummaryBillingInstruction
                    {
                        SuggestedBillingInstruction = string.Empty
                    }
                }
            },
            TotalFeeforLADisposalCostswithBadDebtprovision1 = 100,
            TotalFeeforCommsCostsbyMaterialwithBadDebtprovision2A = 100,
            CommsCostHeaderWithBadDebtFor2bTitle = 100,
            TwoCCommsCostsByCountryWithBadDebtProvision = 100
        },
        CalcResultCommsCostReportDetail = TestDataHelper.GetCalcResultCommsCostReportDetail(),
        CalcResultLateReportingTonnageData = TestDataHelper.GetCalcResultLateReportingTonnage(),
        CalcResultProjectedProducers = new CalcResultProjectedProducers()
    };

    private readonly int columnIndex = 275;

    [TestMethod]
    public void CanCallGetHeaders()
    {
        // Act
        var result = LaDataPrepCostsProducer.GetHeaders().ToList();
        var expectedResult = new List<CalcResultSummaryHeader>();
        expectedResult.AddRange([
            new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.TotalProducerFeeWithoutBadDebtProvision },
            new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.BadDebtProvision },
            new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.TotalProducerFeeWithBadDebtProvision },
            new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.EnglandTotalWithBadDebtProvision },
            new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.WalesTotalWithBadDebtProvision },
            new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.ScotlandTotalWithBadDebtProvision },
            new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.NorthernIrelandTotalWithBadDebtProvision }
        ]);

        // Assert
        Assert.AreEqual(expectedResult[0].Name, result[0].Name);
        Assert.AreEqual(expectedResult[1].Name, result[1].Name);
        Assert.AreEqual(expectedResult[2].Name, result[2].Name);
        Assert.AreEqual(expectedResult[3].Name, result[3].Name);
        Assert.AreEqual(expectedResult[4].Name, result[4].Name);
        Assert.AreEqual(expectedResult[5].Name, result[5].Name);
        Assert.AreEqual(expectedResult[6].Name, result[6].Name);
    }

    [TestMethod]
    public void CanCallGetSummaryHeaders()
    {
        // Act
        var result = LaDataPrepCostsProducer.GetSummaryHeaders(columnIndex).ToList();

        var expectedResult = new List<CalcResultSummaryHeader>();
        expectedResult.AddRange([
            new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.LaDataPrepCostsWithoutBadDebtProvisionTitle },
            new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.BadDebtProvisionTitle },
            new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.LaDataPrepCostsWithBadDebtProvisionTitle }
        ]);

        // Assert
        Assert.AreEqual(expectedResult[0].Name, result[0].Name);
        Assert.AreEqual(expectedResult[1].Name, result[1].Name);
        Assert.AreEqual(expectedResult[2].Name, result[2].Name);
    }

    [TestMethod]
    public void CanCallSetValues()
    {
        // Act
        LaDataPrepCostsProducer.SetValues(calcResult, calcResult.CalcResultSummary);

        // Assert
        Assert.AreEqual(100, calcResult.CalcResultSummary.LaDataPrepCostsTitleSection4);
        Assert.AreEqual(6, calcResult.CalcResultSummary.LaDataPrepCostsBadDebtProvisionTitleSection4);
        Assert.AreEqual(106, calcResult.CalcResultSummary.LaDataPrepCostsWithBadDebtProvisionTitleSection4);
        Assert.AreEqual(100, calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].LocalAuthorityDataPreparationCosts!.TotalProducerFeeWithoutBadDebtProvision);
        Assert.AreEqual(6, calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].LocalAuthorityDataPreparationCosts!.BadDebtProvision);
        Assert.AreEqual(106, calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].LocalAuthorityDataPreparationCosts!.TotalProducerFeeWithBadDebtProvision);
        Assert.AreEqual(42.40m, calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].LocalAuthorityDataPreparationCosts!.EnglandTotalWithBadDebtProvision);
        Assert.AreEqual(31.80m, calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].LocalAuthorityDataPreparationCosts!.WalesTotalWithBadDebtProvision);
        Assert.AreEqual(21.20m, calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].LocalAuthorityDataPreparationCosts!.ScotlandTotalWithBadDebtProvision);
        Assert.AreEqual(10.60m, calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].LocalAuthorityDataPreparationCosts!.NorthernIrelandTotalWithBadDebtProvision);
    }
}
