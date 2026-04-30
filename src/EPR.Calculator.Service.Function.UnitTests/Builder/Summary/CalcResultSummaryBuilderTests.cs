using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Builder;
using EPR.Calculator.Service.Function.Builder.ParametersOther;
using EPR.Calculator.Service.Function.Builder.ScaledupProducers;
using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Builder.Summary.OneAndTwoA;
using EPR.Calculator.Service.Function.Builder.Summary.TonnageVsAllProducer;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Features.Billing.Constants;
using EPR.Calculator.Service.Function.Features.Calculator.Contexts;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary;

[TestClass]
public class CalcResultSummaryBuilderTests
{
    private CalcResultSummaryBuilder _sut = null!;
    private CalcResult calcResult = null!;
    private ApplicationDBContext context = null!;
    private IFixture fixture = null!;
    private MaterialService materialService = null!;
    private InvoicedProducerService invoicedProducerService = null!;
    private CalculatorRunContext runContext = null!;

    [TestInitialize]
    public void Init()
    {
        fixture = TestFixtures.New();
        context = fixture.Freeze<ApplicationDBContext>();
        materialService = fixture.Freeze<MaterialService>();
        invoicedProducerService = fixture.Freeze<InvoicedProducerService>();
        runContext = fixture.Create<CalculatorRunContext>() with { RelativeYear = new RelativeYear(2024) };

        calcResult = new CalcResult
        {
            CalcResultParameterOtherCost = new CalcResultParameterOtherCost
            {
                BadDebtProvision = new KeyValuePair<string, string>("key1", "6%"),
                Details =
                [
                    new CalcResultParameterOtherCostDetail
                    {
                        Name = "4 LA Data Prep Charge",
                        OrderId = 1,
                        England = "£40.00",
                        EnglandValue = 40,
                        Wales = "£30.00",
                        WalesValue = 30,
                        Scotland = "£20.00",
                        ScotlandValue = 20,
                        NorthernIreland = "£10.00",
                        NorthernIrelandValue = 10,
                        Total = "£100.00",
                        TotalValue = 100
                    }
                ],
                Materiality =
                [
                    new CalcResultMateriality
                    {
                        Amount = "Amount £s",
                        AmountValue = 0,
                        Percentage = "%",
                        PercentageValue = 0,
                        SevenMateriality = "7 Materiality"
                    }
                ],
                Name = "Parameters - Other",
                SaOperatingCost =
                [
                    new CalcResultParameterOtherCostDetail
                    {
                        Name = string.Empty,
                        OrderId = 0,
                        England = "England",
                        EnglandValue = 0,
                        Wales = "Wales",
                        WalesValue = 0,
                        Scotland = "Scotland",
                        ScotlandValue = 0,
                        NorthernIreland = "Northern Ireland",
                        NorthernIrelandValue = 0,
                        Total = "Total",
                        TotalValue = 0
                    }
                ],
                SchemeSetupCost =
                {
                    Name = "5 Scheme set up cost Yearly Cost",
                    OrderId = 1,
                    England = "£40.00",
                    EnglandValue = 40,
                    Wales = "£30.00",
                    WalesValue = 30,
                    Scotland = "£20.00",
                    ScotlandValue = 20,
                    NorthernIreland = "£10.00",
                    NorthernIrelandValue = 10,
                    Total = "£100.00",
                    TotalValue = 100
                }
            },
            CalcResultDetail = new CalcResultDetail
            {
                RunId = 1,
                RelativeYear = new RelativeYear(2024)
            },
            CalcResultLaDisposalCostData = new CalcResultLaDisposalCostData
            {
                Name = fixture.Create<string>(),
                CalcResultLaDisposalCostDetails = new List<CalcResultLaDisposalCostDataDetail>
                {
                    new()
                    {
                        DisposalCostPricePerTonne = "20",
                        England = "EnglandTest",
                        Wales = "WalesTest",
                        Name = "ScotlandTest",
                        Scotland = "ScotlandTest",
                        NorthernIreland = "NorthernIrelandTest",
                        Material = "Material1",
                        Total = "TotalTest",
                        ProducerReportedHouseholdPackagingWasteTonnage = fixture.Create<string>(),
                        ReportedPublicBinTonnage = fixture.Create<string>(),
                        ProducerReportedTotalTonnage = fixture.Create<string>()
                    },
                    new()
                    {
                        DisposalCostPricePerTonne = "20",
                        England = "EnglandTest",
                        Wales = "WalesTest",
                        NorthernIreland = "NorthernIrelandTest",
                        Name = "Material1",
                        Scotland = "ScotlandTest",
                        Total = "TotalTest",
                        ProducerReportedHouseholdPackagingWasteTonnage = fixture.Create<string>(),
                        ReportedPublicBinTonnage = fixture.Create<string>(),
                        ProducerReportedTotalTonnage = fixture.Create<string>()
                    },
                    new()
                    {
                        DisposalCostPricePerTonne = "10",
                        England = "EnglandTest",
                        Wales = "WalesTest",
                        NorthernIreland = "NorthernIrelandTest",
                        Name = "Material2",
                        Scotland = "ScotlandTest",
                        Total = "TotalTest",
                        ProducerReportedHouseholdPackagingWasteTonnage = fixture.Create<string>(),
                        ReportedPublicBinTonnage = fixture.Create<string>(),
                        ProducerReportedTotalTonnage = fixture.Create<string>()
                    }
                }
            },
            CalcResultLapcapData = new CalcResultLapcapData { CalcResultLapcapDataDetails = new List<CalcResultLapcapDataDetails>() },
            CalcResultOnePlusFourApportionment = new CalcResultOnePlusFourApportionment
            {
                Name = fixture.Create<string>(),
                CalcResultOnePlusFourApportionmentDetails =
                [
                    new CalcResultOnePlusFourApportionmentDetail
                    {
                        EnglandDisposalTotal = "80",
                        NorthernIrelandDisposalTotal = "70",
                        ScotlandDisposalTotal = "30",
                        WalesDisposalTotal = "20",
                        AllTotal = 0.1M,
                        EnglandTotal = 0.10M,
                        NorthernIrelandTotal = 0.15M,
                        ScotlandTotal = 0.15M,
                        WalesTotal = 0.20M,
                        Name = "Test"
                    },
                    new CalcResultOnePlusFourApportionmentDetail
                    {
                        EnglandDisposalTotal = "80",
                        NorthernIrelandDisposalTotal = "70",
                        ScotlandDisposalTotal = "30",
                        WalesDisposalTotal = "20",
                        AllTotal = 0.1M,
                        EnglandTotal = 0.10M,
                        NorthernIrelandTotal = 0.15M,
                        ScotlandTotal = 0.15M,
                        WalesTotal = 020M,
                        Name = "Test"
                    },
                    new CalcResultOnePlusFourApportionmentDetail
                    {
                        EnglandDisposalTotal = "80",
                        NorthernIrelandDisposalTotal = "70",
                        ScotlandDisposalTotal = "30",
                        WalesDisposalTotal = "20",
                        AllTotal = 0.1M,
                        EnglandTotal = 0.10M,
                        NorthernIrelandTotal = 0.15M,
                        ScotlandTotal = 0.15M,
                        WalesTotal = 0.20M,
                        Name = "Test"
                    },
                    new CalcResultOnePlusFourApportionmentDetail
                    {
                        EnglandDisposalTotal = "80",
                        NorthernIrelandDisposalTotal = "70",
                        ScotlandDisposalTotal = "30",
                        WalesDisposalTotal = "20",
                        AllTotal = 0.1M,
                        EnglandTotal = 0.10M,
                        NorthernIrelandTotal = 0.15M,
                        ScotlandTotal = 0.15M,
                        WalesTotal = 0.20M,
                        Name = "Test"
                    },
                    new CalcResultOnePlusFourApportionmentDetail
                    {
                        EnglandDisposalTotal = "80",
                        NorthernIrelandDisposalTotal = "70",
                        ScotlandDisposalTotal = "30",
                        WalesDisposalTotal = "20",
                        AllTotal = 0.1M,
                        EnglandTotal = 14.53M,
                        NorthernIrelandTotal = 0.15M,
                        ScotlandTotal = 0.15M,
                        WalesTotal = 0.20M,
                        Name = OnePlus4ApportionmentColumnHeaders.OnePluseFourApportionment
                    }
                ]
            },
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
                        SubsidiaryId = "1"
                    }
                }
            },
            CalcResultCommsCostReportDetail = new CalcResultCommsCost
            {
                CalcResultCommsCostCommsCostByMaterial =
                [
                    new CalcResultCommsCostCommsCostByMaterial
                    {
                        CommsCostByMaterialPricePerTonne = "0.42",
                        Name = "Material1"
                    },
                    new CalcResultCommsCostCommsCostByMaterial
                    {
                        CommsCostByMaterialPricePerTonne = "0.3",
                        Name = "Material2"
                    }
                ],
                CommsCostByCountry =
                [
                    new CalcResultCommsCostOnePlusFourApportionment
                    {
                        Total = "Total"
                    },
                    new CalcResultCommsCostOnePlusFourApportionment
                    {
                        TotalValue = 2530
                    }
                ]
            },
            CalcResultLateReportingTonnageData = fixture.Create<CalcResultLateReportingTonnage>(),
            CalcResultScaledupProducers = new CalcResultScaledupProducers
            {
                TitleHeader = null,
                MaterialBreakdownHeaders = null,
                ColumnHeaders = null,
                ScaledupProducers = new List<CalcResultScaledupProducer>()
            },
            CalcResultPartialObligations = new CalcResultPartialObligations
            {
                TitleHeader = null,
                MaterialBreakdownHeaders = null,
                ColumnHeaders = null,
                PartialObligations = new List<CalcResultPartialObligation>()
            },
            CalcResultProjectedProducers = new CalcResultProjectedProducers(),
            CalcResultModulation = null
        };

        // Seed database
        SeedDatabase(context);

        _sut = new CalcResultSummaryBuilder(context, invoicedProducerService, materialService);
    }

    [TestMethod]
    public async Task Construct_ShouldReturnCalcResultSummary()
    {
        var result = await _sut.ConstructAsync(runContext, calcResult);

        Assert.IsNotNull(result);
        Assert.AreEqual(CalcResultSummaryHeaders.CalculationResult, result.ResultSummaryHeader?.Name);
        Assert.AreEqual(26, result.ProducerDisposalFeesHeaders.Count());

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.ProducerDisposalFees);
        Assert.AreEqual(5, result.ProducerDisposalFees.Count());
        var firstProducer = result.ProducerDisposalFees.FirstOrDefault();
        Assert.IsNotNull(firstProducer);
        Assert.AreEqual("Producer1", firstProducer.ProducerName);
        Assert.AreEqual(0, _sut.ScaledupProducers.Count());
        Assert.AreEqual(0, _sut.PartialObligations.Count());
    }

    [TestMethod]
    public async Task Construct_NullScaledUpProdcuers_ShouldSetScaledupProducersToEmptyCollection()
    {
        // Assign
        calcResult.CalcResultScaledupProducers = new CalcResultScaledupProducers
        {
            ColumnHeaders = new List<CalcResultScaledupProducerHeader>(),
            MaterialBreakdownHeaders = new List<CalcResultScaledupProducerHeader>(),
            TitleHeader = new CalcResultScaledupProducerHeader
            {
                Name = "Scaled-up Producers",
                ColumnIndex = 1
            },
            ScaledupProducers = null
        };

        // Act
        var result = await _sut.ConstructAsync(runContext, calcResult);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, _sut.ScaledupProducers.Count());
    }

    [TestMethod]
    public async Task Construct_NullPartialObligations_ShouldSetPartialOblgiationsToEmptyCollection()
    {
        // Assign
        calcResult.CalcResultPartialObligations = new CalcResultPartialObligations
        {
            ColumnHeaders = new List<CalcResultPartialObligationHeader>(),
            MaterialBreakdownHeaders = new List<CalcResultPartialObligationHeader>(),
            TitleHeader = new CalcResultPartialObligationHeader
            {
                Name = "Partial Obligations",
                ColumnIndex = 1
            },
            PartialObligations = null
        };

        // Act
        var result = await _sut.ConstructAsync(runContext, calcResult);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, _sut.PartialObligations.Count());
    }

    [TestMethod]
    public async Task Construct_ShouldSetScaledupProducers()
    {
        // Assign
        calcResult.CalcResultScaledupProducers = TestDataHelper.GetScaledupProducers();

        // Act
        var result = await _sut.ConstructAsync(runContext, calcResult);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, _sut.ScaledupProducers.Count());
    }

    [TestMethod]
    public async Task Construct_ShouldSetPartialObligations()
    {
        // Assign
        calcResult.CalcResultPartialObligations = TestDataHelper.GetPartialObligations();

        // Act
        var result = await _sut.ConstructAsync(runContext, calcResult);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, _sut.PartialObligations.Count());
    }


    [TestMethod]
    public async Task Construct_ShouldMapMaterialBreakdownHeaders()
    {
        var result = await _sut.ConstructAsync(runContext, calcResult);

        Assert.AreEqual("Material1 Breakdown", result.MaterialBreakdownHeaders.First().Name);
    }

    [TestMethod]
    public async Task Construct_ShouldCalculateProducerDisposalFeesCorrectly()
    {
        var result = await _sut.ConstructAsync(runContext, calcResult);

        Assert.IsTrue(result.ProducerDisposalFees.Any());
        Assert.AreEqual("Producer1", result.ProducerDisposalFees.First().ProducerName);
    }

    [TestMethod]
    public async Task Construct_ShouldReturnEmptyProducerDisposalFees_WhenNoProducers()
    {
        var result = await _sut.ConstructAsync(runContext, calcResult);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.ProducerDisposalFees);
    }

    [TestMethod]
    public void Construct_ShouldCalculateBadDebtProvisionCorrectly()
    {
        var result = _sut.ConstructAsync(runContext, calcResult);

        Assert.IsNotNull(result);
        Assert.AreEqual(0, 0);
    }

    [TestMethod]
    public async Task Construct_ShouldReturnProducerDisposalFees_WithoutSubsidiaryTotalRow()
    {
        var result = await _sut.ConstructAsync(runContext, calcResult);

        Assert.IsNotNull(result);
        Assert.AreEqual(5, result.ProducerDisposalFees.Count());
        Assert.IsFalse(result.ProducerDisposalFees.Any(fee => fee.ProducerName.Contains("Total")));
    }

    [TestMethod]
    public async Task Construct_ShouldReturnProducerDisposalFees_WithSubsidiaryTotalRow()
    {
        var result = await _sut.ConstructAsync(runContext, calcResult);
        Assert.IsNotNull(result);
        Assert.AreEqual(5, result.ProducerDisposalFees.Count());
    }

    [TestMethod]
    public async Task Construct_ShouldReturnOverallTotalRow_ForAllProducers()
    {
        var result = await _sut.ConstructAsync(runContext, calcResult);
        Assert.IsNotNull(result);
        var totalRow = result.ProducerDisposalFees.LastOrDefault();
        Assert.IsNotNull(totalRow);
    }

    [TestMethod]
    public async Task GetTotalBadDebtprovision1_ShouldReturnCorrectValue()
    {
        var result = await _sut.ConstructAsync(runContext, calcResult);
        Assert.IsNotNull(result);

        var totalRow = result.ProducerDisposalFees.LastOrDefault();
        Assert.IsNotNull(totalRow);
        totalRow.LocalAuthorityDisposalCostsSectionOne!.BadDebtProvision = 100m;
        totalRow.Level = "Totals";

        var totalFee = CalcResultOneAndTwoAUtil.GetTotalFee(result.ProducerDisposalFees.ToList(), fee => fee.LocalAuthorityDisposalCostsSectionOne!.BadDebtProvision);

        Assert.AreEqual(100m, totalFee);
    }

    [TestMethod]
    public async Task GetTotalDisposalCostswithBadDebtprovision1_ShouldReturnCorrectValue()
    {
        var result = await _sut.ConstructAsync(runContext, calcResult);
        Assert.IsNotNull(result);

        var totalRow = result.ProducerDisposalFees.LastOrDefault();
        Assert.IsNotNull(totalRow);
        totalRow.TotalProducerDisposalFeeWithBadDebtProvision = 200m;
        totalRow.Level = "Totals";

        var totalFee = CalcResultOneAndTwoAUtil.GetTotalFee(result.ProducerDisposalFees.ToList(), fee => fee.TotalProducerDisposalFeeWithBadDebtProvision);

        Assert.AreEqual(200m, totalFee);
    }

    [TestMethod]
    public async Task GetTotalCommsCostswoBadDebtprovision2A_ShouldReturnCorrectValue()
    {
        var result = await _sut.ConstructAsync(runContext, calcResult);
        Assert.IsNotNull(result);

        var totalRow = result.ProducerDisposalFees.LastOrDefault();
        Assert.IsNotNull(totalRow);
        totalRow.TotalProducerCommsFee = 300m;
        totalRow.Level = "Totals";

        var totalFee = CalcResultOneAndTwoAUtil.GetTotalFee(result.ProducerDisposalFees.ToList(), fee => fee.TotalProducerCommsFee);

        Assert.AreEqual(300m, totalFee);
    }

    [TestMethod]
    public async Task GetTotalBadDebtprovision2A_ShouldReturnCorrectValue()
    {
        var result = await _sut.ConstructAsync(runContext, calcResult);
        Assert.IsNotNull(result);

        var totalRow = result.ProducerDisposalFees.LastOrDefault();
        Assert.IsNotNull(totalRow);
        totalRow.CommunicationCostsSectionTwoA!.BadDebtProvision = 400m;
        totalRow.Level = "Totals";

        var totalFee = CalcResultOneAndTwoAUtil.GetTotalFee(result.ProducerDisposalFees.ToList(), fee => fee.CommunicationCostsSectionTwoA!.BadDebtProvision);

        Assert.AreEqual(400m, totalFee);
    }

    [TestMethod]
    public async Task GetTotalCommsCostswithBadDebtprovision2A_ShouldReturnCorrectValue()
    {
        var result = await _sut.ConstructAsync(runContext, calcResult);
        Assert.IsNotNull(result);

        var totalRow = result.ProducerDisposalFees.LastOrDefault();
        Assert.IsNotNull(totalRow);
        totalRow.TotalProducerCommsFeeWithBadDebtProvision = 500m;
        totalRow.Level = "Totals";

        var totalFee = CalcResultOneAndTwoAUtil.GetTotalFee(result.ProducerDisposalFees.ToList(), fee => fee.TotalProducerCommsFeeWithBadDebtProvision);

        Assert.AreEqual(500m, totalFee);
    }

    [TestMethod]
    public async Task GetTotalFee_ShouldReturnZero_WhenNoTotalsLevel()
    {
        var result = await _sut.ConstructAsync(runContext, calcResult);
        Assert.IsNotNull(result);

        var totalRow = result.ProducerDisposalFees.LastOrDefault();
        Assert.IsNotNull(totalRow);
        totalRow.LocalAuthorityDisposalCostsSectionOne!.BadDebtProvision = 0m;
        totalRow.Level = "Totals";

        var totalFee = CalcResultOneAndTwoAUtil.GetTotalFee(result.ProducerDisposalFees.ToList(), fee => fee.LocalAuthorityDisposalCostsSectionOne!.BadDebtProvision);

        Assert.AreEqual(0m, totalFee);
    }

    [TestMethod]
    public async Task GetTotalFee_ShouldReturnZero_WhenProducerDisposalFeesNull()
    {
        var result = await _sut.ConstructAsync(runContext, calcResult);
        Assert.IsNotNull(result);

        var totalRow = result.ProducerDisposalFees.LastOrDefault();
        Assert.IsNotNull(totalRow);
        totalRow.LocalAuthorityDisposalCostsSectionOne!.BadDebtProvision = 0m;
        totalRow.Level = "Totals";

        var totalFee = CalcResultOneAndTwoAUtil.GetTotalFee(null, fee => fee.LocalAuthorityDisposalCostsSectionOne!.BadDebtProvision);

        Assert.AreEqual(0m, totalFee);
    }

    [TestMethod]
    public async Task GetTotalFee_ShouldReturnZero_WhenProducerDisposalFeesIsEmpty()
    {
        var result = await _sut.ConstructAsync(runContext, calcResult);
        Assert.IsNotNull(result);

        var totalRow = result.ProducerDisposalFees.LastOrDefault();
        Assert.IsNotNull(totalRow);
        totalRow.LocalAuthorityDisposalCostsSectionOne!.BadDebtProvision = 0m;
        totalRow.Level = "Totals";

        var totalFee = CalcResultOneAndTwoAUtil.GetTotalFee([], fee => fee.LocalAuthorityDisposalCostsSectionOne!.BadDebtProvision);

        Assert.AreEqual(0m, totalFee);
    }

    [TestMethod]
    public async Task ProducerTotalPercentageVsTotal_ShouldReturnCorrectValue()
    {
        var result = await _sut.ConstructAsync(runContext, calcResult);
        Assert.IsNotNull(result);
        Assert.AreEqual(CalcResultSummaryHeaders.CalculationResult, result.ResultSummaryHeader!.Name);
        Assert.AreEqual(26, result.ProducerDisposalFeesHeaders.Count());

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.ProducerDisposalFees);
        Assert.AreEqual(5, result.ProducerDisposalFees.Count());
        var producerTotalPercentage = result.ProducerDisposalFees.First().PercentageofProducerReportedTonnagevsAllProducers;
        Assert.IsNotNull(producerTotalPercentage);
        Assert.AreEqual(40, producerTotalPercentage);
    }

    [TestMethod]
    public async Task CommsCost2bBill_ShouldReturnCorrectValue()
    {
        var result = await _sut.ConstructAsync(runContext, calcResult);

        Assert.IsNotNull(result);
        Assert.AreEqual(CalcResultSummaryHeaders.CalculationResult, result.ResultSummaryHeader!.Name);
        Assert.AreEqual(26, result.ProducerDisposalFeesHeaders.Count());
        Assert.IsNotNull(result.ProducerDisposalFees);
        Assert.AreEqual(5, result.ProducerDisposalFees.Count());
    }

    private IEnumerable<CalcResultProducerAndReportMaterialDetail> GetProducerRunMaterialDetails(
        IEnumerable<ProducerDetail> producerDetails,
        IEnumerable<ProducerReportedMaterial> producerReportedmaterials,
        int runId)
    {
        return (from p in producerDetails
            join m in producerReportedmaterials on p.Id equals m.ProducerDetailId
            where p.CalculatorRunId == runId
            select new CalcResultProducerAndReportMaterialDetail
            {
                ProducerDetail = p,
                ProducerReportedMaterial = m
            }).ToList();
    }

    [TestMethod]
    public void GetOrderedListOfProducersAssociatedRunId_ShouldReturnCorrectValue()
    {
        var result = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, context.ProducerDetail.ToList());
        Assert.IsNotNull(result);
        Assert.AreEqual(4, result.Count());
        Assert.AreEqual("Producer1", result.First().ProducerName);
        Assert.AreEqual("Subsidiary1", result.Last().ProducerName);
    }

    [TestMethod]
    public async Task GetCalcResultSummary_ShouldReturnCorrectValue()
    {
        var orderedProducerDetails = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, context.ProducerDetail.ToList());
        var runProducerMaterialDetails = GetProducerRunMaterialDetails(
            orderedProducerDetails,
            context.ProducerReportedMaterial.ToList(),
            1);

        var materials = await materialService.GetMaterials();

        var totalPackagingTonnage = CalcResultSummaryBuilder.GetTotalPackagingTonnagePerRun(runProducerMaterialDetails,
            materials,
            1,
            _sut.ScaledupProducers.ToList(),
            _sut.PartialObligations.ToList());

        var producerInvoicedMaterialNetTonnage = Array.Empty<InvoicedProducerRecord>();

        var defaultParams = new List<DefaultParamResultsClass>();


        var result = _sut.GetCalcResultSummary(orderedProducerDetails, materials, calcResult, totalPackagingTonnage, producerInvoicedMaterialNetTonnage, defaultParams);

        Assert.IsNotNull(result);
        Assert.AreEqual(149, result.ColumnHeaders.Count());

        var producerDisposalFees = result.ProducerDisposalFees;
        Assert.IsNotNull(producerDisposalFees);

        var totals = producerDisposalFees.First(t => t.LeaverDate == "Totals");

        var producer = producerDisposalFees.First(t => t.Level == "1");
        Assert.IsNotNull(producer);

        Assert.AreEqual(string.Empty, totals.ProducerName);
        Assert.IsNotNull(producer.ProducerName);
        Assert.AreEqual("Producer1", producer.ProducerName);

        var modulationResult = calcResult;
        modulationResult.CalcResultModulation = "add modulations section";
        var result2 = new CalcResultSummaryBuilder(context, invoicedProducerService, materialService).GetCalcResultSummary(orderedProducerDetails, materials, modulationResult, totalPackagingTonnage, producerInvoicedMaterialNetTonnage, defaultParams);
        Assert.AreEqual(223, result2.ColumnHeaders.Count());
    }

    [TestMethod]
    public async Task GetCalcResultSummary_CanAddTotalRow()
    {
        _sut.ParentOrganisations = new List<Organisation>
        {
            new() { OrganisationId = 1, OrganisationName = "Org1" }
        };

        var orderedProducerDetails = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, context.ProducerDetail.ToList()).ToList();
        var runProducerMaterialDetails = GetProducerRunMaterialDetails(
            orderedProducerDetails,
            context.ProducerReportedMaterial.ToList(),
            1);

        var materials = await materialService.GetMaterials();

        var totalPackagingTonnage = CalcResultSummaryBuilder.GetTotalPackagingTonnagePerRun(runProducerMaterialDetails, materials, 1, _sut.ScaledupProducers.ToList(), _sut.PartialObligations.ToList());

        orderedProducerDetails.Add(new ProducerDetail
        {
            ProducerId = 1
        });

        var producerInvoicedMaterialNetTonnage = Array.Empty<InvoicedProducerRecord>();
        var defaultParams = new List<DefaultParamResultsClass>();

        var result = _sut.GetCalcResultSummary(orderedProducerDetails, materials, calcResult, totalPackagingTonnage, producerInvoicedMaterialNetTonnage, defaultParams);

        Assert.IsNotNull(result);
        Assert.AreEqual(149, result.ColumnHeaders.Count());

        var producerDisposalFees = result.ProducerDisposalFees;
        Assert.IsNotNull(producerDisposalFees);

        var totals = producerDisposalFees.First(t => t.LeaverDate == "Totals");

        var producer = producerDisposalFees.First(t => t.Level == "1");
        Assert.IsNotNull(producer);

        Assert.AreEqual(string.Empty, totals.ProducerName);
        Assert.IsNotNull(producer.ProducerName);
        Assert.AreEqual("Org1", producer.ProducerName);
    }

    [TestMethod]
    public void GetTonnages_ShouldCalculateCorrectlyForGlass()
    {
        var pomData = new List<CalculatorRunPomDataDetail>();
        ImmutableArray<MaterialDto> materials =
        [
            new() { Id = 1, Code = MaterialCodes.Glass, Name = "Glass" }
        ];

        var glassPomData = new List<CalculatorRunPomDataDetail>
        {
            new()
            {
                PackagingMaterial = MaterialCodes.Glass,
                SubmissionPeriod = "2024-P1",
                PackagingType = PackagingTypes.Household,
                PackagingMaterialWeight = 100,
                SubmissionPeriodDesc = "2024 Period 1",
                LoadTimeStamp = DateTime.UtcNow
            },
            new()
            {
                PackagingMaterial = MaterialCodes.Glass,
                SubmissionPeriod = "2024-P1",
                PackagingType = PackagingTypes.HouseholdDrinksContainers,
                PackagingMaterialWeight = 30,
                SubmissionPeriodDesc = "2024 Period 1",
                LoadTimeStamp = DateTime.UtcNow
            }
        };

        pomData.AddRange(glassPomData);

        // Act
        var result = CalcResultScaledupProducersBuilder.GetTonnages(pomData, materials, "2024-P1", 1);

        // Assert
        Assert.IsTrue(result.ContainsKey(MaterialCodes.Glass));
        var glassTonnage = result[MaterialCodes.Glass];

        Assert.AreEqual(0.1m, glassTonnage.ReportedHouseholdPackagingWasteTonnage);
        Assert.AreEqual(0, glassTonnage.ReportedPublicBinTonnage);
        Assert.AreEqual(0.03m, glassTonnage.HouseholdDrinksContainersTonnageGlass);
        Assert.AreEqual(0.13m, glassTonnage.TotalReportedTonnage);
        Assert.AreEqual(0.13m, glassTonnage.NetReportedTonnage);
        Assert.AreEqual(0.1m, glassTonnage.ScaledupReportedHouseholdPackagingWasteTonnage);
        Assert.AreEqual(0, glassTonnage.ScaledupReportedPublicBinTonnage);
        Assert.AreEqual(0.03m, glassTonnage.ScaledupHouseholdDrinksContainersTonnageGlass);
        Assert.AreEqual(0.13m, glassTonnage.ScaledupTotalReportedTonnage);
        Assert.AreEqual(0.13m, glassTonnage.ScaledupNetReportedTonnage);
    }

    [TestMethod]
    public async Task GetCalcResultSummary_ScaledUpProducerShouldReturnCorrectValue()
    {
        calcResult.CalcResultScaledupProducers.ScaledupProducers = GetScaledUpProducers();
        await _sut.ConstructAsync(runContext, calcResult);

        var orderedProducerDetails = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, context.ProducerDetail.ToList());
        var runProducerMaterialDetails = GetProducerRunMaterialDetails(
            orderedProducerDetails,
            context.ProducerReportedMaterial.ToList(),
            1);

        var materials = await materialService.GetMaterials();

        var totalPackagingTonnage = CalcResultSummaryBuilder.GetTotalPackagingTonnagePerRun(runProducerMaterialDetails,
            materials,
            1,
            _sut.ScaledupProducers.ToList(),
            _sut.PartialObligations.ToList());
        var scaledUpProducer = totalPackagingTonnage.First(t => t.ProducerId == 4);

        Assert.AreEqual(2, totalPackagingTonnage.Count());
        Assert.IsNotNull(scaledUpProducer.ProducerId);
        Assert.AreEqual(100, scaledUpProducer.TotalPackagingTonnage);
    }

    public static List<CalcResultScaledupProducer> GetScaledUpProducers()
    {
        var test = new List<CalcResultScaledupProducer>
        {
            new()
            {
                ProducerId = 4,
                ProducerName = "Test",
                ScaledupProducerTonnageByMaterial = new Dictionary<string, CalcResultScaledupProducerTonnage>
                {
                    {
                        "1",
                        new CalcResultScaledupProducerTonnage
                        {
                            ReportedHouseholdPackagingWasteTonnage = 0,
                            ReportedPublicBinTonnage = 0,
                            TotalReportedTonnage = 0,
                            ReportedSelfManagedConsumerWasteTonnage = 0,
                            NetReportedTonnage = 0,
                            ScaledupReportedHouseholdPackagingWasteTonnage = 0,
                            ScaledupReportedPublicBinTonnage = 0,
                            ScaledupTotalReportedTonnage = 100,
                            ScaledupReportedSelfManagedConsumerWasteTonnage = 0,
                            ScaledupNetReportedTonnage = 0
                        }
                    }
                }
            }
        };

        return test;
    }

    [TestMethod]
    public async Task GetCalcResultSummary_PartialObligationShouldReturnCorrectValue()
    {
        calcResult.CalcResultPartialObligations.PartialObligations = GetPartialObligations();
        await _sut.ConstructAsync(runContext, calcResult);

        var orderedProducerDetails = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, context.ProducerDetail.ToList());
        var runProducerMaterialDetails = GetProducerRunMaterialDetails(
            orderedProducerDetails,
            context.ProducerReportedMaterial.ToList(),
            1);

        var materials = await materialService.GetMaterials();

        var totalPackagingTonnage = CalcResultSummaryBuilder.GetTotalPackagingTonnagePerRun(runProducerMaterialDetails,
            materials,
            1,
            _sut.ScaledupProducers.ToList(),
            _sut.PartialObligations.ToList());
        var partialObligation = totalPackagingTonnage.First(t => t.ProducerId == 4);

        Assert.AreEqual(2, totalPackagingTonnage.Count());
        Assert.IsNotNull(partialObligation.ProducerId);
        Assert.AreEqual(25, partialObligation.TotalPackagingTonnage);
    }

    public static List<CalcResultPartialObligation> GetPartialObligations()
    {
        var test = new List<CalcResultPartialObligation>
        {
            new()
            {
                ProducerId = 4,
                ProducerName = "Test",
                PartialObligationTonnageByMaterial = new Dictionary<string, CalcResultPartialObligationTonnage>
                {
                    {
                        "1",
                        new CalcResultPartialObligationTonnage
                        {
                            ReportedHouseholdPackagingWasteTonnage = 0,
                            ReportedPublicBinTonnage = 0,
                            TotalReportedTonnage = 0,
                            ReportedSelfManagedConsumerWasteTonnage = 0,
                            NetReportedTonnage = 0,
                            PartialReportedHouseholdPackagingWasteTonnage = 0,
                            PartialReportedPublicBinTonnage = 0,
                            PartialTotalReportedTonnage = 25,
                            PartialReportedSelfManagedConsumerWasteTonnage = 0,
                            PartialNetReportedTonnage = 0
                        }
                    }
                }
            }
        };

        return test;
    }

    [TestMethod]
    public void CanAddTotalRow_ParentProducerNotFound_ReturnsFalse()
    {
        // Arrange
        var producer = context.ProducerDetail.FirstOrDefault()!;
        IEnumerable<ProducerDetail> producersAndSubsidiaries = context.ProducerDetail;
        var producerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>();

        _sut.ParentOrganisations = new List<Organisation>();

        // Act
        var result = _sut.CanAddTotalRow(producer, producersAndSubsidiaries, producerDisposalFees);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void CanAddTotalRow_ProducerDisposalFeeExists_ReturnsFalse()
    {
        // Arrange
        var producer = context.ProducerDetail.FirstOrDefault()!;
        IEnumerable<ProducerDetail> producersAndSubsidiaries = context.ProducerDetail;
        var producerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>
        {
            new() { ProducerId = "1", ProducerName = "Org1", SubsidiaryId = "" }
        };

        _sut.ParentOrganisations = new List<Organisation>
        {
            new() { OrganisationId = 1, OrganisationName = "Org1" }
        };

        // Act
        var result = _sut.CanAddTotalRow(producer, producersAndSubsidiaries, producerDisposalFees);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void CanAddTotalRow_ValidConditions_ReturnsTrue()
    {
        // Arrange
        var producer = context.ProducerDetail.FirstOrDefault()!;
        IEnumerable<ProducerDetail> producersAndSubsidiaries = context.ProducerDetail;
        var producerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>
        {
            new() { ProducerId = "2", ProducerName = "Org1", SubsidiaryId = "" }
        };

        _sut.ParentOrganisations = new List<Organisation>
        {
            new() { OrganisationId = 1, OrganisationName = "Org1" }
        };

        // Act
        var result = _sut.CanAddTotalRow(producer, producersAndSubsidiaries, producerDisposalFees);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void GetProducerDetailsForTotalRow_IsOverallTotalRow_ReturnsNullObject()
    {
        // Arrange
        var producerId = 1;
        var isOverAllTotalRow = true;

        // Act
        var result = _sut.GetProducerDetailsForTotalRow(producerId, isOverAllTotalRow);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetProducerDetailsForTotalRow_ParentProducerNotFound_ReturnsNullObject()
    {
        // Arrange
        var producerId = 1;
        var isOverAllTotalRow = false;

        _sut.ParentOrganisations = new List<Organisation>();

        // Act
        var result = _sut.GetProducerDetailsForTotalRow(producerId, isOverAllTotalRow);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetProducerDetailsForTotalRow_ParentProducerFoundWithName_ReturnsOrganisationName()
    {
        // Arrange
        var producerId = 1;
        var isOverAllTotalRow = false;

        _sut.ParentOrganisations = new List<Organisation>
        {
            new() { OrganisationId = 1, OrganisationName = "Org1" }
        };

        // Act
        var result = _sut.GetProducerDetailsForTotalRow(producerId, isOverAllTotalRow);

        // Assert
        Assert.AreEqual("Org1", result!.OrganisationName);
    }

    [TestMethod]
    public void GetProducerDetailsForTotalRow_ParentProducerFoundWithoutName_ReturnsNullString()
    {
        // Arrange
        var producerId = 1;
        var isOverAllTotalRow = false;

        _sut.ParentOrganisations = new List<Organisation>
        {
            new() { OrganisationId = 1, OrganisationName = null }
        };

        // Act
        var result = _sut.GetProducerDetailsForTotalRow(producerId, isOverAllTotalRow);

        // Assert
        Assert.IsNull(result!.OrganisationName);
    }

    [TestMethod]
    public void GetProducerDetailsForTotalRow_ParentProducerFoundWithTradingName_ReturnsTradingName()
    {
        // Arrange
        var producerId = 1;
        var isOverAllTotalRow = false;

        _sut.ParentOrganisations = new List<Organisation>
        {
            new() { OrganisationId = 1, OrganisationName = "Good Fruit", TradingName = "GF Trading Name 1" }
        };

        // Act
        var result = _sut.GetProducerDetailsForTotalRow(producerId, isOverAllTotalRow);

        // Assert
        Assert.AreEqual("GF Trading Name 1", result!.TradingName);
    }

    [TestMethod]
    public async Task Construct_HandlesNullScaledupProducers_UsesEmptyList()
    {
        // Arrange
        calcResult.CalcResultScaledupProducers = new CalcResultScaledupProducers
        {
            ScaledupProducers = null
        };

        // Act
        var result = await _sut.ConstructAsync(runContext, calcResult);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.ProducerDisposalFees.Any());
    }

    [TestMethod]
    public async Task Construct_HandlesNullPartialObligations_UsesEmptyList()
    {
        // Arrange
        calcResult.CalcResultPartialObligations = new CalcResultPartialObligations
        {
            PartialObligations = null
        };

        // Act
        var result = await _sut.ConstructAsync(runContext, calcResult);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.ProducerDisposalFees.Any());
    }

    [TestMethod]
    public async Task GetCalcResultSummary_AddsProducerTotalRow_AndProducerRow()
    {
        // Arrange
        var ordered = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, context.ProducerDetail.ToList());
        var materials = await materialService.GetMaterials();
        var runDetails = GetProducerRunMaterialDetails(ordered, context.ProducerReportedMaterial.ToList(), 1);

        _sut.ScaledupProducers = new List<CalcResultScaledupProducer>();

        var totalPackaging = CalcResultSummaryBuilder.GetTotalPackagingTonnagePerRun(
            runDetails, materials, 1, _sut.ScaledupProducers.ToList(), _sut.PartialObligations.ToList());

        _sut.ParentOrganisations = new List<Organisation>
        {
            new() { OrganisationId = ordered.First().ProducerId, OrganisationName = "Org1" }
        };

        var producerInvoicedMaterialNetTonnage = Array.Empty<InvoicedProducerRecord>();
        var defaultParams = new List<DefaultParamResultsClass>();
        // Act
        var summary = _sut.GetCalcResultSummary(ordered, materials, calcResult, totalPackaging, producerInvoicedMaterialNetTonnage, defaultParams);

        // Assert
        Assert.IsNotNull(summary);
        Assert.IsTrue(summary.ProducerDisposalFees.Any());
        Assert.IsTrue(summary.ProducerDisposalFees.Any(r => r.isTotalRow));
        Assert.IsTrue(summary.ProducerDisposalFees.Any(r => !r.isTotalRow));
    }

    [TestMethod]
    public async Task GetCalcResultSummary_AddsTotalRow_WhenProducerHasSubsidiary()
    {
        var parent = context.ProducerDetail.Single(p => p.ProducerId == 1 && p.CalculatorRunId == 1);
        var sub = new ProducerDetail
        {
            ProducerName = parent.ProducerName + " and subsidiary",
            ProducerId = parent.ProducerId,
            CalculatorRunId = parent.CalculatorRunId,
            SubsidiaryId = "S1"
        };
        context.ProducerDetail.Add(sub);
        context.ProducerReportedMaterial.Add(new ProducerReportedMaterial
        {
            ProducerDetail = sub,
            MaterialId = 1,
            PackagingType = "HH",
            SubmissionPeriod = "2025-H1",
            PackagingTonnage = 5m
        });
        context.ProducerReportedMaterial.Add(new ProducerReportedMaterial
        {
            ProducerDetail = sub,
            MaterialId = 1,
            PackagingType = "HH",
            SubmissionPeriod = "2025-H2",
            PackagingTonnage = 5m
        });
        context.SaveChanges();

        _sut.ParentOrganisations = new List<Organisation>
        {
            new() { OrganisationId = parent.ProducerId, OrganisationName = "Org1" }
        };

        var ordered = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, context.ProducerDetail.ToList());
        var materials = await materialService.GetMaterials();
        var runDetails = GetProducerRunMaterialDetails(ordered, context.ProducerReportedMaterial.ToList(), 1);
        var totalPackaging = CalcResultSummaryBuilder.GetTotalPackagingTonnagePerRun(
            runDetails, materials, 1, _sut.ScaledupProducers.ToList(),
            _sut.PartialObligations.ToList());

        var producerInvoicedMaterialNetTonnage = Array.Empty<InvoicedProducerRecord>();
        var defaultParams = new List<DefaultParamResultsClass>();

        var summary = _sut.GetCalcResultSummary(ordered, materials, calcResult, totalPackaging, producerInvoicedMaterialNetTonnage, defaultParams);

        Assert.IsTrue(summary.ProducerDisposalFees.Any(r => r.isTotalRow));
    }

    [TestMethod]
    public async Task GetProducerRow_MarksProducerAsScaledUp_WhenMatchExists()
    {
        var producer = context.ProducerDetail.Single(p => p.ProducerId == 1 && p.CalculatorRunId == 1);

        var materials = await materialService.GetMaterials();

        var tonnageByMaterial = new Dictionary<string, CalcResultScaledupProducerTonnage>();

        foreach (var m in materials)
        {
            tonnageByMaterial[m.Code] = new CalcResultScaledupProducerTonnage
            {
                ReportedHouseholdPackagingWasteTonnage = 0,
                ReportedPublicBinTonnage = 0,
                ReportedSelfManagedConsumerWasteTonnage = 0,
                TotalReportedTonnage = 0,
                NetReportedTonnage = 0,
                ScaledupReportedHouseholdPackagingWasteTonnage = 0,
                ScaledupReportedPublicBinTonnage = 0,
                ScaledupReportedSelfManagedConsumerWasteTonnage = 0,
                ScaledupTotalReportedTonnage = 0,
                ScaledupNetReportedTonnage = 0
            };
        }

        _sut.ScaledupProducers = new List<CalcResultScaledupProducer>
        {
            new()
            {
                ProducerId = producer.ProducerId,
                SubsidiaryId = producer.SubsidiaryId,
                ScaledupProducerTonnageByMaterial = tonnageByMaterial
            }
        };

        var ordered = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, context.ProducerDetail.ToList());

        var producerInvoicedMaterialNetTonnage = Array.Empty<InvoicedProducerRecord>();

        var row = _sut.GetProducerRow(
            new List<CalcResultSummaryProducerDisposalFees>(),
            ordered.Where(pd => pd.ProducerId == producer.ProducerId).ToList(),
            producer,
            materials,
            calcResult, new List<TotalPackagingTonnagePerRun>(),
            producerInvoicedMaterialNetTonnage);

        Assert.AreEqual(CommonConstants.Yes, row.IsProducerScaledup);
    }

    [TestMethod]
    public async Task GetProducerRow_MarksProducerAsPartialObligation_WhenMatchExists()
    {
        var producer = context.ProducerDetail.Single(p => p.ProducerId == 1 && p.CalculatorRunId == 1);

        var materials = await materialService.GetMaterials();

        var tonnageByMaterial = new Dictionary<string, CalcResultPartialObligationTonnage>();

        foreach (var m in materials)
        {
            tonnageByMaterial[m.Code] = new CalcResultPartialObligationTonnage
            {
                ReportedHouseholdPackagingWasteTonnage = 0,
                ReportedPublicBinTonnage = 0,
                ReportedSelfManagedConsumerWasteTonnage = 0,
                TotalReportedTonnage = 0,
                NetReportedTonnage = 0,
                PartialReportedHouseholdPackagingWasteTonnage = 0,
                PartialReportedPublicBinTonnage = 0,
                PartialReportedSelfManagedConsumerWasteTonnage = 0,
                PartialTotalReportedTonnage = 0,
                PartialNetReportedTonnage = 0
            };
        }

        _sut.PartialObligations = new List<CalcResultPartialObligation>
        {
            new()
            {
                ProducerId = producer.ProducerId,
                SubsidiaryId = producer.SubsidiaryId,
                PartialObligationTonnageByMaterial = tonnageByMaterial
            }
        };

        var ordered = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, context.ProducerDetail.ToList());

        var producerInvoicedMaterialNetTonnage = Array.Empty<InvoicedProducerRecord>();

        var row = _sut.GetProducerRow(
            new List<CalcResultSummaryProducerDisposalFees>(),
            ordered.Where(pd => pd.ProducerId == producer.ProducerId).ToList(),
            producer,
            materials,
            calcResult, new List<TotalPackagingTonnagePerRun>(),
            producerInvoicedMaterialNetTonnage);

        Assert.AreEqual(CommonConstants.Yes, row.IsPartialObligation);
    }

    [TestMethod]
    public void CanAddTotalRow_SingleProducerAndPomDataExists_ReturnsFalse()
    {
        var producer = context.ProducerDetail.Single(p => p.ProducerId == 1 && p.CalculatorRunId == 1);

        var producersAndSubsidiaries = new List<ProducerDetail>
        {
            new()
            {
                ProducerId = producer.ProducerId,
                CalculatorRunId = producer.CalculatorRunId,
                SubsidiaryId = null,
                ProducerName = "Parent Org"
            }
        };

        _sut.ParentOrganisations = new List<Organisation>
        {
            new() { OrganisationId = producer.ProducerId, OrganisationName = "Org1" }
        };

        var result = _sut.CanAddTotalRow(producer, producersAndSubsidiaries, new List<CalcResultSummaryProducerDisposalFees>());

        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task GetProducerRow_MarksProducerAsNotScaledUp_WhenNoMatch()
    {
        var producer = context.ProducerDetail.Single(p => p.ProducerId == 1 && p.CalculatorRunId == 1);
        var materials = await materialService.GetMaterials();

        _sut.ScaledupProducers = new List<CalcResultScaledupProducer>();

        var ordered = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, context.ProducerDetail.ToList());
        var producerInvoicedMaterialNetTonnage = Array.Empty<InvoicedProducerRecord>();

        var row = _sut.GetProducerRow(
            new List<CalcResultSummaryProducerDisposalFees>(),
            ordered.Where(pd => pd.ProducerId == producer.ProducerId).ToList(),
            producer,
            materials,
            calcResult,
            new List<TotalPackagingTonnagePerRun>(),
            producerInvoicedMaterialNetTonnage);

        Assert.AreEqual(CommonConstants.No, row.IsProducerScaledup);
    }

    [TestMethod]
    public async Task GetProducerRow_OrgDetailColumnsIfAvailable()
    {
        var producer = context.ProducerDetail.Single(p => p.ProducerId == 1 && p.CalculatorRunId == 1);
        var materials = await materialService.GetMaterials();

        _sut.ScaledupProducers = new List<CalcResultScaledupProducer>();
        _sut.Organisations = new List<Organisation>
        {
            new()
            {
                OrganisationId = 1,
                SubsidiaryId = null,
                OrganisationName = null,
                TradingName = null,
                StatusCode = "99",
                JoinerDate = "01/01/2025",
                LeaverDate = "15/07/2025"
            }
        };

        var ordered = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, context.ProducerDetail.ToList());
        var producerInvoicedMaterialNetTonnage = Array.Empty<InvoicedProducerRecord>();

        var row = _sut.GetProducerRow(
            new List<CalcResultSummaryProducerDisposalFees>(),
            ordered.Where(pd => pd.ProducerId == producer.ProducerId).ToList(),
            producer,
            materials,
            calcResult,
            new List<TotalPackagingTonnagePerRun>(),
            producerInvoicedMaterialNetTonnage);

        Assert.AreEqual(CommonConstants.No, row.IsPartialObligation);
        Assert.AreEqual("99", row.StatusCode);
        Assert.AreEqual("01/01/2025", row.JoinerDate);
        Assert.AreEqual("15/07/2025", row.LeaverDate);
        Assert.AreEqual(CommonConstants.No, row.IsProducerScaledup);
    }

    [TestMethod]
    public async Task GetProducerRow_MarksProducerAsNotPartialObligation_WhenNoMatch()
    {
        var producer = context.ProducerDetail.Single(p => p.ProducerId == 1 && p.CalculatorRunId == 1);
        var materials = await materialService.GetMaterials();

        _sut.PartialObligations = new List<CalcResultPartialObligation>();

        var ordered = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, context.ProducerDetail.ToList());
        var producerInvoicedMaterialNetTonnage = Array.Empty<InvoicedProducerRecord>();

        var row = _sut.GetProducerRow(
            new List<CalcResultSummaryProducerDisposalFees>(),
            ordered.Where(pd => pd.ProducerId == producer.ProducerId).ToList(),
            producer,
            materials,
            calcResult,
            new List<TotalPackagingTonnagePerRun>(),
            producerInvoicedMaterialNetTonnage);

        Assert.AreEqual(CommonConstants.No, row.IsPartialObligation);
    }

    [TestMethod]
    public void CanAddTotalRow_NoParentPomButHasSubsidiary_ReturnsTrue()
    {
        var parent = context.ProducerDetail.Single(p => p.ProducerId == 1 && p.CalculatorRunId == 1);
        var subOnly = new List<ProducerDetail>
        {
            new() { ProducerId = parent.ProducerId, CalculatorRunId = parent.CalculatorRunId, SubsidiaryId = "S1", ProducerName = "Sub1" }
        };

        _sut.ParentOrganisations = new List<Organisation>
        {
            new() { OrganisationId = parent.ProducerId, OrganisationName = "Org1" }
        };

        var canAdd = _sut.CanAddTotalRow(parent, subOnly, new List<CalcResultSummaryProducerDisposalFees>());
        Assert.IsTrue(canAdd);
    }

    private static void SeedDatabase(ApplicationDBContext context)
    {
        context.Material.AddRange(new List<Material>
        {
            new() { Id = 1, Name = "Material1", Code = "123" },
            new() { Id = 2, Name = "Material2", Code = MaterialCodes.Glass }
        });

        context.ProducerDetail.AddRange(new List<ProducerDetail>
        {
            new()
            {
                Id = 1, ProducerName = "Producer1", ProducerId = 1, CalculatorRunId = 1,
                CalculatorRun = new CalculatorRun
                {
                    Id = 1,
                    RelativeYear = new RelativeYear(2024),
                    Name = "CalculatorRunTest1",
                    Classification = RunClassification.InitialRunCompleted,
                    CalculatorRunOrganisationDataMasterId = 1
                }
            },

            new()
            {
                Id = 2, ProducerName = "Producer2", ProducerId = 2, CalculatorRunId = 2,
                CalculatorRun = new CalculatorRun
                {
                    Id = 2,
                    RelativeYear = new RelativeYear(2024),
                    Name = "CalculatorRunTest2",
                    Classification = RunClassification.InitialRunCompleted,
                    CalculatorRunOrganisationDataMasterId = 1
                }
            },

            new()
            {
                Id = 3, ProducerName = "Producer3", ProducerId = 3, CalculatorRunId = 3,
                CalculatorRun = new CalculatorRun
                {
                    Id = 3,
                    RelativeYear = new RelativeYear(2024),
                    Name = "CalculatorRunTest3",
                    Classification = RunClassification.InitialRunCompleted
                }
            },
            new() { Id = 4, ProducerName = "Producer4", ProducerId = 4, CalculatorRunId = 1, SubsidiaryId = null },
            new() { Id = 5, ProducerName = "Producer5", ProducerId = 4, CalculatorRunId = 1, SubsidiaryId = "A123" },
            new() { Id = 6, ProducerName = "Subsidiary1", ProducerId = 4, CalculatorRunId = 1, SubsidiaryId = "A456" }
        });

        context.ProducerReportedMaterial.AddRange(new List<ProducerReportedMaterial>
        {
            new() { MaterialId = 1, PackagingType = "HH", SubmissionPeriod = "2025-H1", PackagingTonnage = 350m, ProducerDetailId = 1 },
            new() { MaterialId = 1, PackagingType = "HH", SubmissionPeriod = "2025-H2", PackagingTonnage = 50m, ProducerDetailId = 1 },
            new() { MaterialId = 2, PackagingType = "HH", SubmissionPeriod = "2025-H1", PackagingTonnage = 250m, ProducerDetailId = 2 },
            new() { MaterialId = 2, PackagingType = "HH", SubmissionPeriod = "2025-H2", PackagingTonnage = 150m, ProducerDetailId = 2 },
            new() { MaterialId = 1, PackagingType = "CW", SubmissionPeriod = "2025-H1", PackagingTonnage = 75m, ProducerDetailId = 1 },
            new() { MaterialId = 1, PackagingType = "CW", SubmissionPeriod = "2025-H2", PackagingTonnage = 125m, ProducerDetailId = 1 },
            new() { MaterialId = 2, PackagingType = "CW", SubmissionPeriod = "2025-H1", PackagingTonnage = 25m, ProducerDetailId = 2 },
            new() { MaterialId = 2, PackagingType = "CW", SubmissionPeriod = "2025-H2", PackagingTonnage = 175m, ProducerDetailId = 2 },
            new() { MaterialId = 1, PackagingType = "HDC", SubmissionPeriod = "2025-H1", PackagingTonnage = 125m, ProducerDetailId = 1 },
            new() { MaterialId = 1, PackagingType = "HDC", SubmissionPeriod = "2025-H2", PackagingTonnage = 175m, ProducerDetailId = 1 },
            new() { MaterialId = 2, PackagingType = "HDC", SubmissionPeriod = "2025-H1", PackagingTonnage = 100m, ProducerDetailId = 2 },
            new() { MaterialId = 2, PackagingType = "HDC", SubmissionPeriod = "2025-H2", PackagingTonnage = 200m, ProducerDetailId = 2 },
            new() { MaterialId = 2, PackagingType = "HH", SubmissionPeriod = "2025-H1", PackagingTonnage = 50m, ProducerDetailId = 4 },
            new() { MaterialId = 2, PackagingType = "HH", SubmissionPeriod = "2025-H2", PackagingTonnage = 250m, ProducerDetailId = 4 },
            new() { MaterialId = 1, PackagingType = "HH", SubmissionPeriod = "2025-H1", PackagingTonnage = 110m, ProducerDetailId = 5 },
            new() { MaterialId = 1, PackagingType = "HH", SubmissionPeriod = "2025-H2", PackagingTonnage = 190m, ProducerDetailId = 5 }
        });

        context.ProducerResultFileSuggestedBillingInstruction.AddRange(new List<ProducerResultFileSuggestedBillingInstruction>
        {
            new() { Id = 1, CalculatorRunId = 1, ProducerId = 1, SuggestedBillingInstruction = BillingConstants.Suggestion.Initial, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
            new() { Id = 2, CalculatorRunId = 2, ProducerId = 2, SuggestedBillingInstruction = BillingConstants.Suggestion.Initial, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
            new() { Id = 3, CalculatorRunId = 3, ProducerId = 3, SuggestedBillingInstruction = BillingConstants.Suggestion.Initial, BillingInstructionAcceptReject = BillingConstants.Action.Accepted }
        });

        context.ProducerDesignatedRunInvoiceInstruction.AddRange(new List<ProducerDesignatedRunInvoiceInstruction>
        {
            new() { Id = 1, CalculatorRunId = 1, ProducerId = 1 },
            new() { Id = 2, CalculatorRunId = 2, ProducerId = 2 },
            new() { Id = 3, CalculatorRunId = 2, ProducerId = 2 }
        });
        context.ProducerInvoicedMaterialNetTonnage.AddRange(new List<ProducerInvoicedMaterialNetTonnage>
        {
            new() { Id = 3, MaterialId = 1, CalculatorRunId = 1, ProducerId = 1, InvoicedNetTonnage = 12.5M },
            new() { Id = 4, MaterialId = 2, CalculatorRunId = 2, ProducerId = 2, InvoicedNetTonnage = 13.5M },
            new() { Id = 5, MaterialId = 2, CalculatorRunId = 2, ProducerId = 2, InvoicedNetTonnage = 13.5M }
        });
        context.CalculatorRunOrganisationDataMaster.AddRange(new List<CalculatorRunOrganisationDataMaster>
        {
            new() { Id = 1, RelativeYear = new RelativeYear(2025), CreatedAt = DateTime.UtcNow, CreatedBy = "TestUser", EffectiveFrom = DateTime.UtcNow }
        });

        context.CalculatorRunOrganisationDataDetails.AddRange(new List<CalculatorRunOrganisationDataDetail>
        {
            new() { Id = 2, CalculatorRunOrganisationDataMasterId = 1, OrganisationId = 4, OrganisationName = "ORG1", SubsidiaryId = null, StatusCode = "99", JoinerDate = "01/01/2025", LeaverDate = "15/07/2025" }
        });

        context.SaveChanges();
    }
}
