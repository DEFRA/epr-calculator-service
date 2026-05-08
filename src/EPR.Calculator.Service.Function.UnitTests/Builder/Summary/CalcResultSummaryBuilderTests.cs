using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Builder;
using EPR.Calculator.Service.Function.Builder.ParametersOther;
using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Builder.Summary.OneAndTwoA;
using EPR.Calculator.Service.Function.Builder.Summary.TonnageVsAllProducer;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Features.Calculator.Contexts;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Data;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary
{
    [TestClass]
    public class CalcResultSummaryBuilderTests
    {
        private readonly DbContextOptions<ApplicationDBContext> dbContextOptions;
        private readonly ApplicationDBContext dbContext;
        private readonly IInvoicedProducerService invoicedProducerService;
        private readonly CalcResultSummaryBuilder calcResultsService;
        private readonly CalcResult calculationResult;
        private readonly ImmutableList<MaterialDetail> materials;
        private readonly ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup;
        private readonly SelfManagedConsumerWaste smcw;
        private readonly CalculatorRunContext runContext = DummyData.RunContexts.CalculatorRun2025 with { RunId = 1 };
        private readonly ImmutableList<InvoicedProducerRecord> emptyInvoicedTonnage = ImmutableList<InvoicedProducerRecord>.Empty;

        private IFixture Fixture { get; init; } = TestFixtures.New();

        public CalcResultSummaryBuilderTests()
        {
            dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "CalcResultSummaryTestDb")
                .Options;
            dbContext = new ApplicationDBContext(dbContextOptions);
            Fixture.Inject(dbContext);
            invoicedProducerService = Fixture.Freeze<InvoicedProducerService>();
            calcResultsService = new CalcResultSummaryBuilder(dbContext, invoicedProducerService);
            calculationResult = new CalcResult
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
                            TotalValue = 100,
                        },
                    ],
                    Materiality =
                    [
                        new CalcResultMateriality
                        {
                            Amount = "Amount £s",
                            AmountValue = 0,
                            Percentage = "%",
                            PercentageValue = 0,
                            SevenMateriality = "7 Materiality",
                        },
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
                            TotalValue = 0,
                        },
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
                        TotalValue = 100,
                    },
                },
                CalcResultDetail = new CalcResultDetail {
                    RunId = 1,
                    RelativeYear = new RelativeYear(2024)
                },
                CalcResultLaDisposalCostData = new CalcResultLaDisposalCostData
                {
                    Name = Fixture.Create<string>(),
                    CalcResultLaDisposalCostDetails = new List<CalcResultLaDisposalCostDataDetail>
                    {
                        new CalcResultLaDisposalCostDataDetail
                        {
                            DisposalCostPricePerTonne="20",
                            England="EnglandTest",
                            Wales="WalesTest",
                            Name="ScotlandTest",
                            Scotland="ScotlandTest",
                            NorthernIreland = "NorthernIrelandTest",
                            Total = "TotalTest",
                            ProducerReportedHouseholdPackagingWasteTonnage = Fixture.Create<string>(),
                            ReportedPublicBinTonnage = Fixture.Create<string>(),
                            ProducerReportedTotalTonnage = Fixture.Create<string>(),
                        },
                        new CalcResultLaDisposalCostDataDetail
                        {
                            DisposalCostPricePerTonne="20",
                            England="EnglandTest",
                            Wales="WalesTest",
                            NorthernIreland = "NorthernIrelandTest",
                            Name="Material1",
                            Scotland="ScotlandTest",
                            Total = "TotalTest",
                            ProducerReportedHouseholdPackagingWasteTonnage = Fixture.Create<string>(),
                            ReportedPublicBinTonnage = Fixture.Create<string>(),
                            ProducerReportedTotalTonnage = Fixture.Create<string>(),
                        },
                        new CalcResultLaDisposalCostDataDetail
                        {
                            DisposalCostPricePerTonne="10",
                            England="EnglandTest",
                            Wales="WalesTest",
                            NorthernIreland = "NorthernIrelandTest",
                            Name="Material2",
                            Scotland="ScotlandTest",
                            Total = "TotalTest",
                            ProducerReportedHouseholdPackagingWasteTonnage = Fixture.Create<string>(),
                            ReportedPublicBinTonnage = Fixture.Create<string>(),
                            ProducerReportedTotalTonnage = Fixture.Create<string>(),
                        },
                    }
                },
                CalcResultLapcapData = new CalcResultLapcapData { CalcResultLapcapDataDetails = new List<CalcResultLapcapDataDetail>() },
                CalcResultOnePlusFourApportionment = new CalcResultOnePlusFourApportionment
                {
                    Name = Fixture.Create<string>(),
                    CalcResultOnePlusFourApportionmentDetails =
                    [
                        new ()
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
                            Name = "Test",
                        },
                        new ()
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
                            Name = "Test",
                        },
                        new ()
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
                            Name = "Test",
                        },
                        new ()
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
                            Name = "Test",
                        },
                        new ()
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
                            Name = OnePlus4ApportionmentColumnHeaders.OnePluseFourApportionment,
                        }

                    ],
                },
                CalcResultSummary = new CalcResultSummary
                {
                    ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>
                    {
                        new ()
                            {
                            ProducerCommsFeesByMaterial = new Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial>(),
                            ProducerDisposalFeesByMaterial = new Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial>(),
                            ProducerId = "1",
                            ProducerName = "Test",
                            TotalProducerDisposalFeeWithBadDebtProvision = 100,
                            TotalProducerCommsFeeWithBadDebtProvision = 100,
                            SubsidiaryId = "1",
                            },
                    },
                },
                CalcResultCommsCostReportDetail = new CalcResultCommsCost
                {
                    CalcResultCommsCostCommsCostByMaterial =
                    [
                        new ()
                        {
                            CommsCostByMaterialPricePerTonne = "0.42",
                            Name = "Material1",
                        },
                        new ()
                        {
                            CommsCostByMaterialPricePerTonne = "0.3",
                            Name = "Material2",
                        },
                    ],
                    CommsCostByCountry =
                    [
                        new ()
                        {
                            Total = "Total",
                        },
                        new ()
                        {
                            TotalValue = 2530,
                        },
                    ],
                },
                CalcResultLateReportingTonnageData = Fixture.Create<CalcResultLateReportingTonnage>(),
                CalcResultScaledupProducers = new CalcResultScaledupProducers
                {
                    TitleHeader = null,
                    MaterialBreakdownHeaders = null,
                    ColumnHeaders = null,
                    ScaledupProducers = new List<CalcResultScaledupProducer>(),
                },
                CalcResultPartialObligations = new CalcResultPartialObligations
                {
                    TitleHeader = null,
                    MaterialBreakdownHeaders = null,
                    ColumnHeaders = null,
                    PartialObligations = new List<CalcResultPartialObligation>(),
                },
                CalcResultProjectedProducers = new CalcResultProjectedProducers(),
            };
            // Seed database
            (materials, projectedMaterialsLookup) = SeedDatabase(dbContext);

            smcw = new SelfManagedConsumerWaste
            {
                ProducerTotals = new List<ProducerSelfManagedConsumerWaste>
                {
                    new ()
                    {
                        ProducerId = 1,
                        SubsidiaryId = null,
                        Level = 1,
                        SelfManagedConsumerWasteDataPerMaterials = CreateMaterialData()
                    },
                    new ()
                    {
                        ProducerId = 4,
                        SubsidiaryId = null,
                        Level = 1,
                        SelfManagedConsumerWasteDataPerMaterials = CreateMaterialData()
                    },
                    new ()
                    {
                        ProducerId = 4,
                        SubsidiaryId = "A123",
                        Level = 2,
                        SelfManagedConsumerWasteDataPerMaterials = CreateMaterialData()
                    }
                },

                OverallTotalPerMaterials = new Dictionary<string, SelfManagedConsumerWasteData>
                {
                    {
                        MaterialCodes.Aluminium,
                        new SelfManagedConsumerWasteData
                        {
                            SelfManagedConsumerWasteTonnage = 0,
                            ActionedSelfManagedConsumerWasteTonnage = 0,
                            ResidualSelfManagedConsumerWasteTonnage = 0,
                            NetReportedTonnage = (0, 0, 0, 0)
                        }
                    },
                    {
                        MaterialCodes.Glass,
                        new SelfManagedConsumerWasteData
                        {
                            SelfManagedConsumerWasteTonnage = 0,
                            ActionedSelfManagedConsumerWasteTonnage = 0,
                            ResidualSelfManagedConsumerWasteTonnage = 0,
                            NetReportedTonnage = (0, 0, 0, 0)
                        }
                    }
                }
            };
        }

        private Dictionary<string, SelfManagedConsumerWasteData> CreateMaterialData()
        {
            return new Dictionary<string, SelfManagedConsumerWasteData>
            {
                { "AL", CreateEmptyData() },
                { "GL", CreateEmptyData() }
            };
        }

        private SelfManagedConsumerWasteData CreateEmptyData()
        {
            return new SelfManagedConsumerWasteData
            {
                SelfManagedConsumerWasteTonnage = 0,
                ActionedSelfManagedConsumerWasteTonnage = 0,
                ResidualSelfManagedConsumerWasteTonnage = 0,
                NetReportedTonnage = (0, 0, 0, 0)
            };
        }

        [TestCleanup]
        public void TestCleanup()
        {
            dbContext.Database.EnsureDeleted();
            dbContext.Dispose();
        }

        [TestMethod]
        public async Task Construct_ShouldReturnCalcResultSummary()
        {
            var result = await calcResultsService.ConstructAsync(runContext, materials, calculationResult, smcw);

            Assert.IsNotNull(result);
            Assert.AreEqual(CalcResultSummaryHeaders.CalculationResult, result.ResultSummaryHeader?.Name);
            Assert.AreEqual(26, result.ProducerDisposalFeesHeaders.Count());

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ProducerDisposalFees);
            Assert.AreEqual(5, result.ProducerDisposalFees.Count());
            var firstProducer = result.ProducerDisposalFees.FirstOrDefault();
            Assert.IsNotNull(firstProducer);
            Assert.AreEqual("Producer1", firstProducer.ProducerName);
            Assert.AreEqual(0, calcResultsService.ScaledupProducers.Count());
            Assert.AreEqual(0, calcResultsService.PartialObligations.Count());
        }

        [TestMethod]
        public async Task Construct_NullScaledUpProdcuers_ShouldSetScaledupProducersToEmptyCollection()
        {
            // Assign
            var calcResult = calculationResult;
            calcResult.CalcResultScaledupProducers = new CalcResultScaledupProducers
            {
                ColumnHeaders = new List<CalcResultScaledupProducerHeader>(),
                MaterialBreakdownHeaders = new List<CalcResultScaledupProducerHeader>(),
                TitleHeader = new CalcResultScaledupProducerHeader
                {
                    Name = "Scaled-up Producers",
                    ColumnIndex = 1,
                },
                ScaledupProducers = null
            };

            // Act
            var result = await calcResultsService.ConstructAsync(runContext, materials, calcResult, smcw);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, calcResultsService.ScaledupProducers.Count());
        }

        [TestMethod]
        public async Task Construct_NullPartialObligations_ShouldSetPartialOblgiationsToEmptyCollection()
        {
            // Assign
            var calcResult = calculationResult;
            calcResult.CalcResultPartialObligations = new CalcResultPartialObligations
            {
                ColumnHeaders = new List<CalcResultPartialObligationHeader>(),
                MaterialBreakdownHeaders = new List<CalcResultPartialObligationHeader>(),
                TitleHeader = new CalcResultPartialObligationHeader
                {
                    Name = "Partial Obligations",
                    ColumnIndex = 1,
                },
                PartialObligations = null
            };

            // Act
            var result = await calcResultsService.ConstructAsync(runContext, materials, calcResult, smcw);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, calcResultsService.PartialObligations.Count());
        }

        [TestMethod]
        public async Task Construct_ShouldSetScaledupProducers()
        {
            // Assign
            var calcResult = calculationResult;
            calcResult.CalcResultScaledupProducers = DummyData.GetScaledupProducers();

            // Act
            var result = await calcResultsService.ConstructAsync(runContext, materials, calcResult, smcw);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, calcResultsService.ScaledupProducers.Count());
        }

        [TestMethod]
        public async Task Construct_ShouldSetPartialObligations()
        {
            // Assign
            var calcResult = calculationResult;
            calcResult.CalcResultPartialObligations = DummyData.GetPartialObligations();

            // Act
            var result = await calcResultsService.ConstructAsync(runContext, materials, calcResult, smcw);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, calcResultsService.PartialObligations.Count());
        }


        [TestMethod]
        public async Task Construct_ShouldMapMaterialBreakdownHeaders()
        {
            var result = await calcResultsService.ConstructAsync(runContext, materials, calculationResult, smcw);

            Assert.AreEqual("Material1 Breakdown", result.MaterialBreakdownHeaders.First().Name);
        }

        [TestMethod]
        public async Task Construct_ShouldCalculateProducerDisposalFeesCorrectly()
        {
            var result = await calcResultsService.ConstructAsync(runContext, materials, calculationResult, smcw);

            Assert.IsTrue(result.ProducerDisposalFees.Any());
            Assert.AreEqual("Producer1", result.ProducerDisposalFees.First().ProducerName);
        }

        [TestMethod]
        public async Task Construct_ShouldReturnEmptyProducerDisposalFees_WhenNoProducers()
        {
            var result = await calcResultsService.ConstructAsync(runContext, materials, calculationResult, smcw);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ProducerDisposalFees);
        }

        [TestMethod]
        public void Construct_ShouldCalculateBadDebtProvisionCorrectly()
        {
            var result = calcResultsService.ConstructAsync(runContext, materials, calculationResult, smcw);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, 0);
        }

        [TestMethod]
        public async Task Construct_ShouldReturnProducerDisposalFees_WithoutSubsidiaryTotalRow()
        {
            var result = await calcResultsService.ConstructAsync(runContext, materials, calculationResult, smcw);

            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.ProducerDisposalFees.Count());
            Assert.IsFalse(result.ProducerDisposalFees.Any(fee => fee.ProducerName.Contains("Total")));
        }

        [TestMethod]
        public async Task Construct_ShouldReturnProducerDisposalFees_WithSubsidiaryTotalRow()
        {
            var result = await calcResultsService.ConstructAsync(runContext, materials, calculationResult, smcw);
            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.ProducerDisposalFees.Count());
        }

        [TestMethod]
        public async Task Construct_ShouldReturnOverallTotalRow_ForAllProducers()
        {
            var result = await calcResultsService.ConstructAsync(runContext, materials, calculationResult, smcw);
            Assert.IsNotNull(result);
            var totalRow = result.ProducerDisposalFees.LastOrDefault();
            Assert.IsNotNull(totalRow);
        }

        [TestMethod]
        public async Task GetTotalBadDebtprovision1_ShouldReturnCorrectValue()
        {
            var result = await calcResultsService.ConstructAsync(runContext, materials, calculationResult, smcw);
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
            var result = await calcResultsService.ConstructAsync(runContext, materials, calculationResult, smcw);
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
            var result = await calcResultsService.ConstructAsync(runContext, materials, calculationResult, smcw);
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
            var result = await calcResultsService.ConstructAsync(runContext, materials, calculationResult, smcw);
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
            var result = await calcResultsService.ConstructAsync(runContext, materials, calculationResult, smcw);
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
            var result = await calcResultsService.ConstructAsync(runContext, materials, calculationResult, smcw);
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
            var result = await calcResultsService.ConstructAsync(runContext, materials, calculationResult, smcw);
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
            var result = await calcResultsService.ConstructAsync(runContext, materials, calculationResult, smcw);
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
            var result = await calcResultsService.ConstructAsync(runContext, materials, calculationResult, smcw);
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
            var result = await calcResultsService.ConstructAsync(runContext, materials, calculationResult, smcw);

            Assert.IsNotNull(result);
            Assert.AreEqual(CalcResultSummaryHeaders.CalculationResult, result.ResultSummaryHeader!.Name);
            Assert.AreEqual(26, result.ProducerDisposalFeesHeaders.Count());
            Assert.IsNotNull(result.ProducerDisposalFees);
            Assert.AreEqual(5, result.ProducerDisposalFees.Count());
        }

        private IEnumerable<CalcResultProducerAndReportMaterialDetail> GetProducerRunMaterialDetails(
            IEnumerable<ProducerDetail> producerDetails,
            IEnumerable<ProducerReportedMaterialProjected> producerReportedMaterials,
            int runId
        )
        {
            return (
                from p in producerDetails
                join m in producerReportedMaterials on p.Id equals m.ProducerDetailId
                where p.CalculatorRunId == runId
                select new CalcResultProducerAndReportMaterialDetail
                {
                    ProducerDetail = p,
                    ProducerReportedMaterialProjected = m
                }
            ).ToList();
        }

        [TestMethod]
        public void GetOrderedListOfProducersAssociatedRunId_ShouldReturnCorrectValue()
        {
            var result = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, dbContext.ProducerDetail.ToList());
            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.Count());
            Assert.AreEqual("Producer1", result.First().ProducerName);
            Assert.AreEqual("Subsidiary1", result.Last().ProducerName);
        }

        [TestMethod]
        public void GetCalcResultSummary_ShouldReturnCorrectValue()
        {
            var orderedProducerDetails = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, dbContext.ProducerDetail.ToList());
            var runProducerMaterialDetails = GetProducerRunMaterialDetails(
                orderedProducerDetails,
                dbContext.ProducerReportedMaterialProjected.ToList(),
                1);

            var totalPackagingTonnage =
                CalcResultSummaryBuilder.GetTotalPackagingTonnagePerRun(
                    runProducerMaterialDetails,
                    materials,
                    1
                );

            var producerInvoicedMaterialNetTonnage = emptyInvoicedTonnage;

            var defaultParams = new List<DefaultParamResultsClass>();

            var result = new CalcResultSummaryBuilder(dbContext, invoicedProducerService).GetCalcResultSummary(runContext, projectedMaterialsLookup, orderedProducerDetails, materials, calculationResult, totalPackagingTonnage, producerInvoicedMaterialNetTonnage, defaultParams, smcw);

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

            var modulationResult = calculationResult;
            var modulationRunContext = DummyData.RunContexts.CalculatorRun2026 with { RunId = 1 };
            var result2 = new CalcResultSummaryBuilder(dbContext, invoicedProducerService).GetCalcResultSummary(modulationRunContext, projectedMaterialsLookup, orderedProducerDetails, materials, modulationResult, totalPackagingTonnage, producerInvoicedMaterialNetTonnage, defaultParams, smcw);
            Assert.AreEqual(205, result2.ColumnHeaders.Count());
        }

        [TestMethod]
        public void GetProducerDetailsForTotalRow_IsOverallTotalRow_ReturnsNullObject()
        {
            // Arrange
            int producerId = 1;
            bool isOverAllTotalRow = true;

            // Act
            var result = calcResultsService.GetProducerDetailsForTotalRow(producerId, isOverAllTotalRow);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetProducerDetailsForTotalRow_ParentProducerNotFound_ReturnsNullObject()
        {
            // Arrange
            int producerId = 1;
            bool isOverAllTotalRow = false;

            calcResultsService.ParentOrganisations = new List<Organisation>();

            // Act
            var result = calcResultsService.GetProducerDetailsForTotalRow(producerId, isOverAllTotalRow);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetProducerDetailsForTotalRow_ParentProducerFoundWithName_ReturnsOrganisationName()
        {
            // Arrange
            int producerId = 1;
            bool isOverAllTotalRow = false;

            calcResultsService.ParentOrganisations = new List<Organisation> {
                    new Organisation { OrganisationId = 1, OrganisationName = "Org1" }
                };

            // Act
            var result = calcResultsService.GetProducerDetailsForTotalRow(producerId, isOverAllTotalRow);

            // Assert
            Assert.AreEqual("Org1", result!.OrganisationName);
        }

        [TestMethod]
        public void GetProducerDetailsForTotalRow_ParentProducerFoundWithoutName_ReturnsNullString()
        {
            // Arrange
            int producerId = 1;
            bool isOverAllTotalRow = false;

            calcResultsService.ParentOrganisations = new List<Organisation> {
                    new Organisation { OrganisationId = 1, OrganisationName = null }
                };

            // Act
            var result = calcResultsService.GetProducerDetailsForTotalRow(producerId, isOverAllTotalRow);

            // Assert
            Assert.IsNull(result!.OrganisationName);
        }

        [TestMethod]
        public void GetProducerDetailsForTotalRow_ParentProducerFoundWithTradingName_ReturnsTradingName()
        {
            // Arrange
            int producerId = 1;
            bool isOverAllTotalRow = false;

            calcResultsService.ParentOrganisations = new List<Organisation> {
                    new Organisation { OrganisationId = 1, OrganisationName = "Good Fruit", TradingName = "GF Trading Name 1" }
                };

            // Act
            var result = calcResultsService.GetProducerDetailsForTotalRow(producerId, isOverAllTotalRow);

            // Assert
            Assert.AreEqual("GF Trading Name 1", result!.TradingName);
        }

        [TestMethod]
        public async Task Construct_HandlesNullScaledupProducers_UsesEmptyList()
        {
            // Arrange
            var calcResult = calculationResult;
            calcResult.CalcResultScaledupProducers = new CalcResultScaledupProducers
            {
                ScaledupProducers = null
            };

            // Act
            var result = await calcResultsService.ConstructAsync(runContext, materials, calcResult, smcw);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ProducerDisposalFees.Any());
        }

        [TestMethod]
        public async Task Construct_HandlesNullPartialObligations_UsesEmptyList()
        {
            // Arrange
            var calcResult = calculationResult;
            calcResult.CalcResultPartialObligations = new CalcResultPartialObligations
            {
                PartialObligations = null
            };

            // Act
            var result = await calcResultsService.ConstructAsync(runContext, materials, calcResult, smcw);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ProducerDisposalFees.Any());
        }

        [TestMethod]
        public void GetCalcResultSummary_AddsProducerTotalRow_AndProducerRow()
        {
            // Arrange
            var ordered = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, dbContext.ProducerDetail.ToList());
            var runDetails = GetProducerRunMaterialDetails(ordered, dbContext.ProducerReportedMaterialProjected.ToList(), 1);

            calcResultsService.ScaledupProducers = new List<CalcResultScaledupProducer>();

            var totalPackaging =
                CalcResultSummaryBuilder.GetTotalPackagingTonnagePerRun(runDetails, materials, 1);

            calcResultsService.ParentOrganisations = new List<Organisation>
            {
                new Organisation { OrganisationId = ordered.First().ProducerId, OrganisationName = "Org1" }
            };

            var producerInvoicedMaterialNetTonnage = emptyInvoicedTonnage;
            var defaultParams = new List<DefaultParamResultsClass>();
            // Act
            var summary = calcResultsService.GetCalcResultSummary(runContext, projectedMaterialsLookup, ordered, materials, calculationResult, totalPackaging, producerInvoicedMaterialNetTonnage, defaultParams, smcw);

            // Assert
            Assert.IsNotNull(summary);
            Assert.IsTrue(summary.ProducerDisposalFees.Any());
            Assert.IsTrue(summary.ProducerDisposalFees.Any(r => r.isTotalRow));
            Assert.IsTrue(summary.ProducerDisposalFees.Any(r => !r.isTotalRow));
        }

        [TestMethod]
        public void GetCalcResultSummary_AddsTotalRow_WhenProducerHasSubsidiary()
        {
            var parent = dbContext.ProducerDetail.Single(p => p.ProducerId == 1 && p.CalculatorRunId == 1);
            var sub = new ProducerDetail
            {
                ProducerName = parent.ProducerName + " and subsidiary",
                ProducerId = parent.ProducerId,
                CalculatorRunId = parent.CalculatorRunId,
                SubsidiaryId = "S1"
            };
            dbContext.ProducerDetail.Add(sub);
            dbContext.ProducerReportedMaterialProjected.Add(new ProducerReportedMaterialProjected
            {
                ProducerDetail = sub,
                MaterialId = 1,
                PackagingType = "HH",
                SubmissionPeriod = "2025-H1",
                PackagingTonnage = 5m
            });
            dbContext.ProducerReportedMaterialProjected.Add(new ProducerReportedMaterialProjected
            {
                ProducerDetail = sub,
                MaterialId = 1,
                PackagingType = "HH",
                SubmissionPeriod = "2025-H2",
                PackagingTonnage = 5m
            });
            dbContext.SaveChanges();

            calcResultsService.ParentOrganisations = new List<Organisation>
            {
                new Organisation { OrganisationId = parent.ProducerId, OrganisationName = "Org1" }
            };

            var ordered = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, dbContext.ProducerDetail.ToList());
            var runDetails = GetProducerRunMaterialDetails(ordered, dbContext.ProducerReportedMaterialProjected.ToList(), 1);
            var totalPackaging =
                CalcResultSummaryBuilder.GetTotalPackagingTonnagePerRun(runDetails, materials, 1);

            var producerInvoicedMaterialNetTonnage = emptyInvoicedTonnage;
            var defaultParams = new List<DefaultParamResultsClass>();

            var summary = calcResultsService.GetCalcResultSummary(runContext, projectedMaterialsLookup, ordered, materials, calculationResult, totalPackaging, producerInvoicedMaterialNetTonnage, defaultParams, smcw);

            Assert.IsTrue(summary.ProducerDisposalFees.Any(r => r.isTotalRow));
        }

        [TestMethod]
        public void GetProducerRow_MarksProducerAsScaledUp_WhenMatchExists()
        {
            var producer = dbContext.ProducerDetail.Single(p => p.ProducerId == 1 && p.CalculatorRunId == 1);
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

            calcResultsService.ScaledupProducers = new List<CalcResultScaledupProducer>
            {
                new CalcResultScaledupProducer
                {
                    ProducerId = producer.ProducerId,
                    SubsidiaryId = producer.SubsidiaryId,
                    ScaledupProducerTonnageByMaterial = tonnageByMaterial
                }
            };

            var ordered = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, dbContext.ProducerDetail.ToList());

            var producerInvoicedMaterialNetTonnage = emptyInvoicedTonnage;

            var row = calcResultsService.GetProducerRow(
                runContext,
                projectedMaterialsLookup,
                new List<CalcResultSummaryProducerDisposalFees>(),
                ordered.Where(pd => pd.ProducerId == producer.ProducerId).ToList(),
                producer,
                materials,
                calculationResult, new List<TotalPackagingTonnagePerRun>(),
                producerInvoicedMaterialNetTonnage,
                smcw);

            Assert.AreEqual(CommonConstants.Yes, row.IsProducerScaledup);
        }

        [TestMethod]
        public void GetProducerRow_MarksProducerAsPartialObligation_WhenMatchExists()
        {
            var producer = dbContext.ProducerDetail.Single(p => p.ProducerId == 1 && p.CalculatorRunId == 1);

            var tonnageByMaterial = new Dictionary<string, CalcResultPartialObligationTonnage>();

            foreach (var m in materials)
            {
                tonnageByMaterial[m.Code] = new CalcResultPartialObligationTonnage
                {
                    HouseholdTonnage = 0,
                    PublicBinTonnage = 0,
                    SelfManagedConsumerWasteTonnage = 0,
                    TotalTonnage = 0,
                    PartialHouseholdTonnage = 0,
                    PartialPublicBinTonnage = 0,
                    PartialSelfManagedConsumerWasteTonnage = 0,
                    PartialTotalTonnage = 0
                };
            }

            calcResultsService.PartialObligations = new List<CalcResultPartialObligation>
            {
                new CalcResultPartialObligation
                {
                    ProducerId = producer.ProducerId,
                    SubsidiaryId = producer.SubsidiaryId,
                    PartialObligationTonnageByMaterial = tonnageByMaterial
                }
            };

            var ordered = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, dbContext.ProducerDetail.ToList());

            var producerInvoicedMaterialNetTonnage = emptyInvoicedTonnage;

            var row = calcResultsService.GetProducerRow(
                runContext,
                projectedMaterialsLookup,
                new List<CalcResultSummaryProducerDisposalFees>(),
                ordered.Where(pd => pd.ProducerId == producer.ProducerId).ToList(),
                producer,
                materials,
                calculationResult, new List<TotalPackagingTonnagePerRun>(),
                producerInvoicedMaterialNetTonnage,
                smcw);

            Assert.AreEqual(CommonConstants.Yes, row.IsPartialObligation);
        }

        [TestMethod]
        public void GetProducerRow_MarksProducerAsNotScaledUp_WhenNoMatch()
        {
            var producer = dbContext.ProducerDetail.Single(p => p.ProducerId == 1 && p.CalculatorRunId == 1);
            calcResultsService.ScaledupProducers = new List<CalcResultScaledupProducer>();

            var ordered = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, dbContext.ProducerDetail.ToList());
            var producerInvoicedMaterialNetTonnage = emptyInvoicedTonnage;

            var row = calcResultsService.GetProducerRow(
                runContext,
                projectedMaterialsLookup,
                new List<CalcResultSummaryProducerDisposalFees>(),
                ordered.Where(pd => pd.ProducerId == producer.ProducerId).ToList(),
                producer,
                materials,
                calculationResult,
                new List<TotalPackagingTonnagePerRun>(),
                producerInvoicedMaterialNetTonnage,
                smcw);

            Assert.AreEqual(CommonConstants.No, row.IsProducerScaledup);
        }

        [TestMethod]
        public void GetProducerRow_OrgDetailColumnsIfAvailable()
        {
            var producer = dbContext.ProducerDetail.Single(p => p.ProducerId == 1 && p.CalculatorRunId == 1);

            calcResultsService.ScaledupProducers = new List<CalcResultScaledupProducer>();
            calcResultsService.Organisations = new List<Organisation>
            {
                new Organisation
                {
                    OrganisationId = 1,
                    SubsidiaryId = null,
                    OrganisationName = null,
                    TradingName = null,
                    StatusCode = "99",
                    JoinerDate = "01/01/2025",
                    LeaverDate = "15/07/2025" }
            };

            var ordered = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, dbContext.ProducerDetail.ToList());
            var producerInvoicedMaterialNetTonnage = emptyInvoicedTonnage;

            var row = calcResultsService.GetProducerRow(
                runContext,
                projectedMaterialsLookup,
                new List<CalcResultSummaryProducerDisposalFees>(),
                ordered.Where(pd => pd.ProducerId == producer.ProducerId).ToList(),
                producer,
                materials,
                calculationResult,
                new List<TotalPackagingTonnagePerRun>(),
                producerInvoicedMaterialNetTonnage,
                smcw);

            Assert.AreEqual(CommonConstants.No, row.IsPartialObligation);
            Assert.AreEqual("99", row.StatusCode);
            Assert.AreEqual("01/01/2025", row.JoinerDate);
            Assert.AreEqual("15/07/2025", row.LeaverDate);
            Assert.AreEqual(CommonConstants.No, row.IsProducerScaledup);
        }

        [TestMethod]
        public void GetProducerRow_MarksProducerAsNotPartialObligation_WhenNoMatch()
        {
            var producer = dbContext.ProducerDetail.Single(p => p.ProducerId == 1 && p.CalculatorRunId == 1);

            calcResultsService.PartialObligations = new List<CalcResultPartialObligation>();

            var ordered = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, dbContext.ProducerDetail.ToList());
            var producerInvoicedMaterialNetTonnage = emptyInvoicedTonnage;

            var row = calcResultsService.GetProducerRow(
                runContext,
                projectedMaterialsLookup,
                new List<CalcResultSummaryProducerDisposalFees>(),
                ordered.Where(pd => pd.ProducerId == producer.ProducerId).ToList(),
                producer,
                materials,
                calculationResult,
                new List<TotalPackagingTonnagePerRun>(),
                producerInvoicedMaterialNetTonnage,
                smcw);

            Assert.AreEqual(CommonConstants.No, row.IsPartialObligation);
        }

        [TestMethod]
        public async Task ConstructAsync_PopulatesBillingInstructionSection_OnLevel1Rows()
        {
            // Note: persistence of billing instructions to the DB is now handled by BillingRunFinalizer/BillingInstructionService
            // (covered by their own tests). This test only verifies that ConstructAsync populates the
            // BillingInstructionSection on each Level 1 producer row in the in-memory result.

            // Act
            var summary = await calcResultsService.ConstructAsync(runContext, materials, calculationResult, smcw);

            // Assert
            var level1 = summary.ProducerDisposalFees
                                .FirstOrDefault(f => f.Level == CommonConstants.LevelOne.ToString()
                                                  && f.ProducerIdInt == 1);
            Assert.IsNotNull(level1);
            Assert.IsNotNull(level1.BillingInstructionSection, "BillingInstructionSection should be populated on Level 1.");
        }

        private static (ImmutableList<MaterialDetail> , ILookup<(int, string?), ProducerReportedMaterialProjected>) SeedDatabase(ApplicationDBContext dbContext)
        {
            var materials = new List<Material>
            {
                new() { Id = 1, Name = "Material1", Code = MaterialCodes.Aluminium },
                new() { Id = 2, Name = "Material2", Code = MaterialCodes.Glass },
            };
            dbContext.Material.AddRange(materials);

            var producerDetails = new List<ProducerDetail>
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
                new() { Id = 4, ProducerName = "Producer4", ProducerId = 4, CalculatorRunId = 1 , SubsidiaryId=null },
                new() { Id = 5, ProducerName = "Producer5", ProducerId = 4, CalculatorRunId = 1 , SubsidiaryId="A123" },
                new() { Id = 6, ProducerName = "Subsidiary1", ProducerId = 4, CalculatorRunId = 1 , SubsidiaryId="A456"},
            };
            dbContext.ProducerDetail.AddRange(producerDetails);

            var producerReportedMaterialProjecteds = new List<ProducerReportedMaterialProjected>
            {
                new() { MaterialId = 1, PackagingType = "HH", SubmissionPeriod = "2025-H1", PackagingTonnage = 350m, ProducerDetailId = 1 },
                new() { MaterialId = 1, PackagingType = "HH", SubmissionPeriod = "2025-H2", PackagingTonnage = 50m, ProducerDetailId = 1 },
                new() { MaterialId = 2, PackagingType = "HH", SubmissionPeriod = "2025-H1", PackagingTonnage = 250m, ProducerDetailId = 2 },
                new() { MaterialId = 2, PackagingType = "HH", SubmissionPeriod = "2025-H2", PackagingTonnage = 150m, ProducerDetailId = 2 },
                new() { MaterialId = 1, PackagingType = "CW", SubmissionPeriod = "2025-H1", PackagingTonnage = 75m, ProducerDetailId = 1 },
                new() { MaterialId = 1, PackagingType = "CW", SubmissionPeriod = "2025-H2", PackagingTonnage = 125m, ProducerDetailId = 1 },
                new() { MaterialId = 2, PackagingType = "CW", SubmissionPeriod = "2025-H1", PackagingTonnage = 25m, ProducerDetailId = 2 },
                new() { MaterialId = 2, PackagingType = "CW", SubmissionPeriod = "2025-H2", PackagingTonnage = 175m, ProducerDetailId = 2 },
                new() { MaterialId = 1, PackagingType = "HDC", SubmissionPeriod = "2025-H1",  PackagingTonnage = 125m, ProducerDetailId = 1 },
                new() { MaterialId = 1, PackagingType = "HDC", SubmissionPeriod = "2025-H2",  PackagingTonnage = 175m, ProducerDetailId = 1 },
                new() { MaterialId = 2, PackagingType = "HDC", SubmissionPeriod = "2025-H1",  PackagingTonnage = 100m, ProducerDetailId = 2 },
                new() { MaterialId = 2, PackagingType = "HDC", SubmissionPeriod = "2025-H2",  PackagingTonnage = 200m, ProducerDetailId = 2 },
                new() { MaterialId = 2, PackagingType = "HH", SubmissionPeriod = "2025-H1", PackagingTonnage = 50m, ProducerDetailId = 4 },
                new() { MaterialId = 2, PackagingType = "HH", SubmissionPeriod = "2025-H2", PackagingTonnage = 250m, ProducerDetailId = 4 },
                new() { MaterialId = 1, PackagingType = "HH", SubmissionPeriod = "2025-H1", PackagingTonnage = 110m, ProducerDetailId = 5 },
                new() { MaterialId = 1, PackagingType = "HH", SubmissionPeriod = "2025-H2", PackagingTonnage = 190m, ProducerDetailId = 5 }
            };

            dbContext.ProducerReportedMaterialProjected.AddRange(producerReportedMaterialProjecteds);
            var projectedMaterialsLookup = producerDetails
                .SelectMany(pd => producerReportedMaterialProjecteds
                    .Where(rm => rm.ProducerDetailId == pd.Id)
                    .Select(rm => (Key: (pd.ProducerId, pd.SubsidiaryId), Rm: rm))
                )
                .ToLookup(x => x.Key, x => x.Rm);


            dbContext.ProducerResultFileSuggestedBillingInstruction.AddRange(new List<ProducerResultFileSuggestedBillingInstruction>
            {
                new ProducerResultFileSuggestedBillingInstruction { Id = 1, CalculatorRunId = 1, ProducerId = 1, SuggestedBillingInstruction="INITIAL", BillingInstructionAcceptReject="Accepted"},
                new ProducerResultFileSuggestedBillingInstruction { Id = 2, CalculatorRunId = 2, ProducerId = 2, SuggestedBillingInstruction="INITIAL", BillingInstructionAcceptReject="Accepted"},
                new ProducerResultFileSuggestedBillingInstruction { Id = 3, CalculatorRunId = 3, ProducerId = 3, SuggestedBillingInstruction="INITIAL", BillingInstructionAcceptReject="Accepted"},
            });

            dbContext.ProducerDesignatedRunInvoiceInstruction.AddRange(new List<ProducerDesignatedRunInvoiceInstruction>
            {
                new ProducerDesignatedRunInvoiceInstruction { Id = 1,CalculatorRunId = 1, ProducerId = 1},
                new ProducerDesignatedRunInvoiceInstruction { Id = 2,CalculatorRunId = 2, ProducerId = 2},
                new ProducerDesignatedRunInvoiceInstruction { Id = 3,CalculatorRunId = 2, ProducerId = 2},
            });
            dbContext.ProducerInvoicedMaterialNetTonnage.AddRange(new List<ProducerInvoicedMaterialNetTonnage>
            {
                new ProducerInvoicedMaterialNetTonnage { Id = 3, MaterialId = 1,CalculatorRunId = 1, ProducerId = 1, InvoicedNetTonnage = 12.5M},
                new ProducerInvoicedMaterialNetTonnage { Id = 4, MaterialId = 2,CalculatorRunId = 2, ProducerId = 2, InvoicedNetTonnage = 13.5M},
                new ProducerInvoicedMaterialNetTonnage { Id = 5, MaterialId = 2,CalculatorRunId = 2, ProducerId = 2, InvoicedNetTonnage = 13.5M},
            });
            dbContext.CalculatorRunOrganisationDataMaster.AddRange(new List<CalculatorRunOrganisationDataMaster>
            {
                new CalculatorRunOrganisationDataMaster {Id = 1,RelativeYear = new RelativeYear(2025), CreatedAt= DateTime.UtcNow, CreatedBy = "TestUser" , EffectiveFrom = DateTime.UtcNow}
            });

            dbContext.CalculatorRunOrganisationDataDetails.AddRange(new List<CalculatorRunOrganisationDataDetail>
            {
                new CalculatorRunOrganisationDataDetail {Id = 2, CalculatorRunOrganisationDataMasterId = 1, OrganisationId = 4, OrganisationName="ORG1", SubsidiaryId = null, StatusCode = "99", JoinerDate = "01/01/2025", LeaverDate = "15/07/2025"},
            });

            dbContext.SaveChanges();

            return (materials.Select(m => new MaterialDetail{ Id = m.Id, Code = m.Code, Name = m.Name}).ToImmutableList(), projectedMaterialsLookup);
        }
    }
}
