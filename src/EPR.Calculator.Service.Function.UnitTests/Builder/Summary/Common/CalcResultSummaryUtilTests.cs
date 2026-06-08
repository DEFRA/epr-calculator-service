using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.Modulation;
using EPR.Calculator.Service.Function.Builder.Summary.Common;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary.Common;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class CalcResultSummaryUtilTests
{
    private readonly CalcResult calcResult;

    public CalcResultSummaryUtilTests()
    {
        calcResult = new CalcResult
        {
            CalcResultScaledupProducers = new CalcResultScaledupProducers(){
                    ScaledupProducers = ImmutableList<CalcResultScaledupProducer>.Empty
                },
                CalcResultPartialObligations = new CalcResultPartialObligations(){
                    PartialObligations = ImmutableList<CalcResultPartialObligation>.Empty,
                },
            CalcResultParameterOtherCost       = TestDataHelper.GetCalcResultParameterOtherCost(),
            CalcResultDetail                   = TestDataHelper.GetCalcResultDetail(),
            CalcResultLaDisposalCostData       = TestDataHelper.GetCalcResultLaDisposalCostData(),
            CalcResultLapcapData               = TestDataHelper.GetCalcResultLapcapData(),
            CalcResultOnePlusFourApportionment = TestDataHelper.GetCalcResultOnePlusFourApportionment(),
            CalcResultSummary                  = TestDataHelper.GetCalcResultSummary(),
            CalcResultCommsCostReportDetail    = TestDataHelper.GetCalcResultCommsCostReportDetail(),
            CalcResultLateReportingTonnageData = GetCalcResultLateReportingTonnage(),
            CalcResultProjectedProducers       = new CalcResultProjectedProducers(){
                H1ProjectedProducers = ImmutableList<CalcResultH1ProjectedProducer>.Empty,
                H2ProjectedProducers = ImmutableList<CalcResultH2ProjectedProducer>.Empty,
            }
        };
    }

    private Fixture Fixture { get; } = new();

    public static ILookup<(int, string?), ProducerReportedMaterialProjected> ProjectedMaterialsLookup(List<ProducerDetail> producers)
    {
        // This allows us to retrofit into existing test setup, but ProducerReportedMaterials normally
        // refers to pre-processed data, which is _not_ what we want to display in the ResultsSummary
        ProducerReportedMaterialProjected ToProjected(ProducerReportedMaterial rm) =>
            new()
            {
                MaterialId                   = rm.MaterialId,
                ProducerDetailId             = rm.ProducerDetailId,
                PackagingType                = rm.PackagingType,
                PackagingTonnage             = rm.PackagingTonnage,
                PackagingTonnageRed          = rm.PackagingTonnageRed,
                PackagingTonnageAmber        = rm.PackagingTonnageAmber,
                PackagingTonnageGreen        = rm.PackagingTonnageGreen,
                PackagingTonnageRedMedical   = rm.PackagingTonnageRedMedical,
                PackagingTonnageAmberMedical = rm.PackagingTonnageAmberMedical,
                PackagingTonnageGreenMedical = rm.PackagingTonnageGreenMedical,
                SubmissionPeriod             = rm.SubmissionPeriod
            };

        return producers
            .SelectMany(p => p.ProducerReportedMaterials.Select(rm => (Key: (p.ProducerId, p.SubsidiaryId), Rm: ToProjected(rm))))
            .ToLookup(x => x.Key, x => x.Rm);
    }

    [TestMethod]
    public void CanGetNonTotalRowLevelIndex()
    {
        // Arrange
        var producerDisposalFeesLookup = TestDataHelper.GetProducerDisposalFees();
        var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);

        // Act
        var result = CalcResultSummaryUtil.GetLevelIndex(producerDisposalFeesLookup, producer);

        // Assert
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void CanGetHouseholdPackagingWasteTonnage()
    {
        // Arrange
        var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
        var material = TestDataHelper.GetMaterialDetails().First(m => m.Code == "AL");

        // Act
        var result = CalcResultSummaryUtil.GetTonnage(ProjectedMaterialsLookup(new List<ProducerDetail> { producer }), producer, material, PackagingTypes.Household);

        // Assert
        Assert.AreEqual(1000.00m, result);
    }

    [TestMethod]
    public void CanGetPublicBinTonnage()
    {
        // Arrange
        var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
        var material = TestDataHelper.GetMaterialDetails().First(m => m.Code == "PL");

        // Act
        var result = CalcResultSummaryUtil.GetTonnage(ProjectedMaterialsLookup(new List<ProducerDetail> { producer }), producer, material, PackagingTypes.PublicBin);

        // Assert
        Assert.AreEqual(20.00m, result);
    }

    [TestMethod]
    public void CanGetHouseholdDrinksContainersTonnage()
    {
        // Arrange
        var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
        var material = TestDataHelper.GetMaterialDetails().First(m => m.Code == "GL");

        // Act
        var result = CalcResultSummaryUtil.GetTonnage(ProjectedMaterialsLookup(new List<ProducerDetail> { producer }), producer, material, PackagingTypes.HouseholdDrinksContainers);

        // Assert
        Assert.AreEqual(20.00m, result);
    }

    [TestMethod]
    public void CanGetReportedTonnage()
    {
        // Arrange
        var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
        var material = TestDataHelper.GetMaterialDetails().First(m => m.Code == "AL");

        // Act
        var result = CalcResultSummaryUtil.GetReportedTonnage(ProjectedMaterialsLookup(new List<ProducerDetail> { producer }), producer, material);

        // Assert
        Assert.AreEqual(1000.00m, result);
    }

    [TestMethod]
    public void CanGetHouseholdPackagingWasteTonnageProducerTotal()
    {
        // Arrange
        var producers = TestDataHelper.GetProducers();
        var material = TestDataHelper.GetMaterialDetails().First(m => m.Code == "AL");

        // Act
        var result = CalcResultSummaryUtil.GetTonnageTotal(ProjectedMaterialsLookup(producers), producers, material, PackagingTypes.Household);

        // Assert
        Assert.AreEqual(3000.00m, result);
    }

    [TestMethod]
    public void CanGetPublicBinTonnageProducerTotal()
    {
        // Arrange
        var producers = TestDataHelper.GetProducers();
        var material = TestDataHelper.GetMaterialDetails().First(m => m.Code == "PL");

        // Act
        var result = CalcResultSummaryUtil.GetTonnageTotal(ProjectedMaterialsLookup(producers), producers, material, PackagingTypes.PublicBin);

        // Assert
        Assert.AreEqual(60.00m, result);
    }

    [TestMethod]
    public void CanGetReportedTonnageProducerTotal()
    {
        // Arrange
        var producers = TestDataHelper.GetProducers();
        var material = TestDataHelper.GetMaterialDetails().First(m => m.Code == "AL");

        // Act
        var result = CalcResultSummaryUtil.GetReportedTonnageTotal(ProjectedMaterialsLookup(producers), producers, material);

        // Assert
        Assert.AreEqual(3000.00m, result);
    }

    [TestMethod]
    public void CanGetHouseholdDrinksContainersTonnageProducerTotal()
    {
        // Arrange
        var producers = TestDataHelper.GetProducers();
        var material = TestDataHelper.GetMaterialDetails().First(m => m.Code == "GL");

        // Act
        var result = CalcResultSummaryUtil.GetTonnageTotal(ProjectedMaterialsLookup(producers), producers, material, PackagingTypes.HouseholdDrinksContainers);

        // Assert
        Assert.AreEqual(60.00m, result);
    }

    [TestMethod]
    public void CanGetManagedConsumerWasteTonnage()
    {
        // Arrange
        var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
        var material = TestDataHelper.GetMaterialDetails().First(m => m.Code == "AL");

        // Act
        var result = CalcResultSummaryUtil.GetTonnage(ProjectedMaterialsLookup(new List<ProducerDetail> { producer }), producer, material, PackagingTypes.ConsumerWaste);

        // Assert
        Assert.AreEqual(20.00m, result);
    }

    [TestMethod]
    public void CanGetManagedConsumerWasteTonnageProducerTotal()
    {
        // Arrange
        var producers = TestDataHelper.GetProducers();
        var material = TestDataHelper.GetMaterialDetails().First(m => m.Code == "AL");

        // Act
        var result = CalcResultSummaryUtil.GetTonnageTotal(ProjectedMaterialsLookup(producers), producers, material, PackagingTypes.ConsumerWaste);

        // Assert
        Assert.AreEqual(60.00m, result);
    }

    [TestMethod]
    public void CanGetPricePerTonne_NonMatchingMaterial()
    {
        // Arrange
        var material = Fixture.Create<MaterialDetail>();

        // Act
        var result = CalcResultSummaryUtil.GetPricePerTonne(material, calcResult);

        // Assert
        Assert.AreEqual((total: null, red: null, amber: null, green: null), result);
    }

    [TestMethod]
    public void CanGetPricePerTonne()
    {
        // Arrange
        var material = TestDataHelper.GetMaterialDetails().First(m => m.Code == "AL");

        // Act
        var result = CalcResultSummaryUtil.GetPricePerTonne(material, calcResult);

        // Assert
        Assert.AreEqual((total: 0.5889m, red: null, amber: null, green: null), result);
    }

    [TestMethod]
    public void CanGetProducerDisposalFee()
    {
        // Arrange
        var material = TestDataHelper.GetMaterialDetails().First(m => m.Code == "AL");

        // Act
        var result = CalcResultSummaryUtil.GetProducerDisposalFee(material, calcResult, SelfManagedConsumerWasteData.Zero);

        // Assert
        Assert.AreEqual((total: 0m, red: null, amber: null, green: null), result);
    }

    [TestMethod]
    public void CanGetProducerDisposalFee_WithModulation()
    {
        var material = TestDataHelper.GetMaterialDetails().First(m => m.Code == "AL");

        calcResult.CalcResultModulation = new ModulationResult
        {
            GreenFactor = 2,
            RedFactor = 4,
            MaterialModulation = new Dictionary<MaterialDetail, MaterialModulation>
            {
                [material] = mkMaterialModulation(100, 120, 77.1423m, 90, 220, 550, 22000, 55000)
            }
        };

        var smcw = new SelfManagedConsumerWasteData
        {
            SelfManagedConsumerWasteTonnage = 0,
            ActionedSelfManagedConsumerWasteTonnage = (0, 0m, 0m, 0m),
            ResidualSelfManagedConsumerWasteTonnage = 0,
            NetReportedTonnage = (null, 1m, 2m, 3m)
        };

        var result = CalcResultSummaryUtil.GetProducerDisposalFee(material, calcResult, smcw);

        Assert.AreEqual((total: 551.4269m, red: 120, amber: 200, green: 231.4269m), result);
    }


    [TestMethod]
    public void GetBadDebtProvision_ValidPercentage_WithPercent()
    {
        var result = CalcResultSummaryUtil.GetBadDebtProvision(calcResult, 200m);
        Assert.AreEqual(12m, result);
    }

    [TestMethod]
    public void GetProducerDisposalFeeWithBadDebtProvision_AddsPercentage()
    {
        var result = CalcResultSummaryUtil.GetProducerDisposalFeeWithBadDebtProvision(calcResult, 100m);
        Assert.AreEqual(106m, Math.Round(result.Total, 10));
    }

    [TestMethod]
    public void CanGetCommsCostHeaderWithoutBadDebtFor2bTitle()
    {
        // Act
        var result = CalcResultSummaryUtil.GetCommsCostHeaderWithoutBadDebtFor2bTitle(calcResult);

        // Assert
        Assert.AreEqual(2531, result);
    }

    [TestMethod]
    public void CanGetReportedPublicBinTonnage()
    {
        // Arrange
        var producer = TestDataHelper.GetProducers().First(p => p.Id == 2);
        var material = TestDataHelper.GetMaterialDetails().First(m => m.Code == "PL");

        // Act
        var result = CalcResultSummaryUtil.GetTonnage(ProjectedMaterialsLookup(new List<ProducerDetail> { producer }), producer, material, PackagingTypes.PublicBin);

        // Assert
        Assert.AreEqual(20.00m, result);
    }

    [TestMethod]
    public void CanGetReportedPublicBinTonnageTotal()
    {
        // Arrange
        var producers = TestDataHelper.GetProducers();
        var material = TestDataHelper.GetMaterialDetails().First(m => m.Code == "PL");

        // Act
        var result = CalcResultSummaryUtil.GetTonnageTotal(ProjectedMaterialsLookup(producers), producers, material, PackagingTypes.PublicBin);

        // Assert
        Assert.AreEqual(60.00m, result);
    }

    [TestMethod]
    public void CanGetReportedHDCTonnage()
    {
        // Arrange
        var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
        var material = TestDataHelper.GetMaterialDetails().First(m => m.Code == "GL");

        // Act
        var result = CalcResultSummaryUtil.GetTonnage(ProjectedMaterialsLookup(new List<ProducerDetail> { producer }), producer, material, PackagingTypes.HouseholdDrinksContainers);

        // Assert
        Assert.AreEqual(20.00m, result);
    }

    [TestMethod]
    public void CanGetReportedHDCTonnageTotal()
    {
        // Arrange
        var producers = TestDataHelper.GetProducers();
        var material = TestDataHelper.GetMaterialDetails().First(m => m.Code == "GL");

        // Act
        var result = CalcResultSummaryUtil.GetTonnageTotal(ProjectedMaterialsLookup(producers), producers, material, PackagingTypes.HouseholdDrinksContainers);

        // Assert
        Assert.AreEqual(60.00m, result);
    }

    private CalcResultLateReportingTonnage GetCalcResultLateReportingTonnage() => Fixture.Create<CalcResultLateReportingTonnage>();

    private MaterialModulation mkMaterialModulation(decimal adc, decimal rdc, decimal gdc, decimal at, decimal rt, decimal gt, decimal rAtAdc, decimal gAtAdc)
    {
        return new MaterialModulation
        {
            AmberMaterialDisposalCost             = adc,
            RedMaterialDisposalCost               = rdc,
            GreenMaterialDisposalCost             = gdc,
            AmberMaterialTonnages                 = at,
            RedMaterialTonnages                   = rt,
            GreenMaterialTonnages                 = gt,
            TotalRedMaterialAtAmberDisposalCost   = rAtAdc,
            TotalGreenMaterialAtAmberDisposalCost = gAtAdc
        };
    }
}
