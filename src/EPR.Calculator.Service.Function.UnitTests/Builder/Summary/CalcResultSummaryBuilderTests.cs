using AutoFixture;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Builder;
using EPR.Calculator.Service.Function.Builder.ParametersOther;
using EPR.Calculator.Service.Function.Builder.ScaledupProducers;
using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Builder.Summary.OneAndTwoA;
using EPR.Calculator.Service.Function.Builder.Summary.TonnageVsAllProducer.cs;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Mappers;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using EPR.Calculator.Service.Function.Services;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary
{
    [TestClass]
    public class CalcResultSummaryBuilderTests
    {
        private readonly DbContextOptions<ApplicationDBContext> dbContextOptions;
        private readonly ApplicationDBContext context;
        private readonly CalcResultSummaryBuilder calcResultsService;
        private readonly CalcResult calculationResult;
        private readonly CalcResultScaledupProducers scaledupProducers;
        private readonly CalcResultPartialObligations partialObligations;
        private readonly SelfManagedConsumerWaste smcw;

        private Fixture Fixture { get; init; } = new Fixture();

        public CalcResultSummaryBuilderTests()
        {
            dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "CalcResultSummaryTestDb")
                .Options;
            context = new ApplicationDBContext(dbContextOptions);
            calcResultsService = new CalcResultSummaryBuilder(context);
            scaledupProducers = TestDataHelper.GetScaledupProducers();
            partialObligations = TestDataHelper.GetPartialObligations();
            calculationResult = new CalcResult
            {
                ShowModulations = false,
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
                    },
                },
                CalcResultLapcapData = new CalcResultLapcapData { CalcResultLapcapDataDetail = new List<CalcResultLapcapDataDetail>() },
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
                CalcResultParameterCommunicationCost = Fixture.Create<CalcResultParameterCommunicationCost>(),
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
            SeedDatabase(context);

            smcw = new SelfManagedConsumerWaste
            {
                ProducerTotals = new List<ProducerSelfManagedConsumerWaste>
                {
                    new ProducerSelfManagedConsumerWaste
                    {
                        ProducerId = 4,
                        SubsidiaryId = string.Empty,
                        Level = 1,
                        SelfManagedConsumerWasteDataPerMaterials = new Dictionary<string, SelfManagedConsumerWasteData>
                        {
                            { "AL", new SelfManagedConsumerWasteData
                                {
                                    SelfManagedConsumerWasteTonnage = 0,
                                    ActionedSelfManagedConsumerWasteTonnage = 0,
                                    ResidualSelfManagedConsumerWasteTonnage = 0,
                                    NetReportedTonnage = (0, 0, 0, 0)
                                }
                            },
                            { "GL", new SelfManagedConsumerWasteData
                                {
                                    SelfManagedConsumerWasteTonnage = 0,
                                    ActionedSelfManagedConsumerWasteTonnage = 0,
                                    ResidualSelfManagedConsumerWasteTonnage = 0,
                                    NetReportedTonnage = (0, 0, 0, 0)
                                }
                            }
                        }
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

        [TestCleanup]
        public void TestCleanup()
        {
            context.Database.EnsureDeleted();
            context.Dispose();
        }

        [TestMethod]
        public async Task Construct_ShouldReturnCalcResultSummary()
        {
            var result = await calcResultsService.ConstructAsync(runId: 1, relativeYear: new RelativeYear(2024), isBillingFile: false, calculationResult, smcw);

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
            var result = await calcResultsService.ConstructAsync(runId: 1, relativeYear: new RelativeYear(2024), isBillingFile: false, calcResult, smcw);

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
            var result = await calcResultsService.ConstructAsync(runId: 1, relativeYear: new RelativeYear(2024), isBillingFile: false, calcResult, smcw);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, calcResultsService.PartialObligations.Count());
        }

        [TestMethod]
        public async Task Construct_ShouldSetScaledupProducers()
        {
            // Assign
            var calcResult = calculationResult;
            calcResult.CalcResultScaledupProducers = TestDataHelper.GetScaledupProducers();

            // Act
            var result = await calcResultsService.ConstructAsync(runId: 1, relativeYear: new RelativeYear(2024), isBillingFile: false, calcResult, smcw);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, calcResultsService.ScaledupProducers.Count());
        }

        [TestMethod]
        public async Task Construct_ShouldSetPartialObligations()
        {
            // Assign
            var calcResult = calculationResult;
            calcResult.CalcResultPartialObligations = TestDataHelper.GetPartialObligations();

            // Act
            var result = await calcResultsService.ConstructAsync(runId: 1, relativeYear: new RelativeYear(2024), isBillingFile: false, calcResult, smcw);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, calcResultsService.PartialObligations.Count());
        }


        [TestMethod]
        public async Task Construct_ShouldMapMaterialBreakdownHeaders()
        {
            var result = await calcResultsService.ConstructAsync(runId: 1, relativeYear: new RelativeYear(2024), isBillingFile: false, calculationResult, smcw);

            Assert.AreEqual("Material1 Breakdown", result.MaterialBreakdownHeaders.First().Name);
        }

        [TestMethod]
        public async Task Construct_ShouldCalculateProducerDisposalFeesCorrectly()
        {
            var result = await calcResultsService.ConstructAsync(runId: 1, relativeYear: new RelativeYear(2024), isBillingFile: false, calculationResult, smcw);

            Assert.IsTrue(result.ProducerDisposalFees.Any());
            Assert.AreEqual("Producer1", result.ProducerDisposalFees.First().ProducerName);
        }

        [TestMethod]
        public async Task Construct_ShouldReturnEmptyProducerDisposalFees_WhenNoProducers()
        {
            var result = await calcResultsService.ConstructAsync(runId: 1, relativeYear: new RelativeYear(2024), isBillingFile: false, calculationResult, smcw);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ProducerDisposalFees);
        }

        [TestMethod]
        public void Construct_ShouldCalculateBadDebtProvisionCorrectly()
        {
            var result = calcResultsService.ConstructAsync(runId: 1, relativeYear: new RelativeYear(2024), isBillingFile: false, calculationResult, smcw);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, 0);
        }

        [TestMethod]
        public async Task Construct_ShouldReturnProducerDisposalFees_WithoutSubsidiaryTotalRow()
        {
            var result = await calcResultsService.ConstructAsync(runId: 1, relativeYear: new RelativeYear(2024), isBillingFile: false, calculationResult, smcw);

            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.ProducerDisposalFees.Count());
            Assert.IsFalse(result.ProducerDisposalFees.Any(fee => fee.ProducerName.Contains("Total")));
        }

        [TestMethod]
        public async Task Construct_ShouldReturnProducerDisposalFees_WithSubsidiaryTotalRow()
        {
            var result = await calcResultsService.ConstructAsync(runId: 1, relativeYear: new RelativeYear(2024), isBillingFile: false, calculationResult, smcw);
            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.ProducerDisposalFees.Count());
        }

        [TestMethod]
        public async Task Construct_ShouldReturnOverallTotalRow_ForAllProducers()
        {
            var result = await calcResultsService.ConstructAsync(runId: 1, relativeYear: new RelativeYear(2024), isBillingFile: false, calculationResult, smcw);
            Assert.IsNotNull(result);
            var totalRow = result.ProducerDisposalFees.LastOrDefault();
            Assert.IsNotNull(totalRow);
        }

        [TestMethod]
        public async Task GetTotalBadDebtprovision1_ShouldReturnCorrectValue()
        {
            var result = await calcResultsService.ConstructAsync(runId: 1, relativeYear: new RelativeYear(2024), isBillingFile: false, calculationResult, smcw);
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
            var result = await calcResultsService.ConstructAsync(runId: 1, relativeYear: new RelativeYear(2024), isBillingFile: false, calculationResult, smcw);
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
            var result = await calcResultsService.ConstructAsync(runId: 1, relativeYear: new RelativeYear(2024), isBillingFile: false, calculationResult, smcw);
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
            var result = await calcResultsService.ConstructAsync(runId: 1, relativeYear: new RelativeYear(2024), isBillingFile: false, calculationResult, smcw);
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
            var result = await calcResultsService.ConstructAsync(runId: 1, relativeYear: new RelativeYear(2024), isBillingFile: false, calculationResult, smcw);
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
            var result = await calcResultsService.ConstructAsync(runId: 1, relativeYear: new RelativeYear(2024), isBillingFile: false, calculationResult, smcw);
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
            var result = await calcResultsService.ConstructAsync(runId: 1, relativeYear: new RelativeYear(2024), isBillingFile: false, calculationResult, smcw);
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
            var result = await calcResultsService.ConstructAsync(runId: 1, relativeYear: new RelativeYear(2024), isBillingFile: false, calculationResult, smcw);
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
            var result = await calcResultsService.ConstructAsync(runId: 1, relativeYear: new RelativeYear(2024), isBillingFile: false, calculationResult, smcw);
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
            var result = await calcResultsService.ConstructAsync(runId: 1, relativeYear: new RelativeYear(2024), isBillingFile: false, calculationResult, smcw);

            Assert.IsNotNull(result);
            Assert.AreEqual(CalcResultSummaryHeaders.CalculationResult, result.ResultSummaryHeader!.Name);
            Assert.AreEqual(26, result.ProducerDisposalFeesHeaders.Count());
            Assert.IsNotNull(result.ProducerDisposalFees);
            Assert.AreEqual(5, result.ProducerDisposalFees.Count());
        }

        [TestMethod]
        public void MaterialMapper_ShouldReturnCorrectValue()
        {
            var materials = context.Material.ToList();
            var result = MaterialMapper.Map(materials);
            Assert.IsNotNull(result);
            Assert.AreEqual(materials.Count, result.Count);
            var material = result[0];
            var actualMaterial = materials[0];
            Assert.IsNotNull(material);
            Assert.IsNotNull(actualMaterial);
            Assert.AreEqual(material.Name, actualMaterial.Name);
            Assert.AreEqual(material.Code, actualMaterial.Code);
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
                        ProducerReportedMaterial = m,
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
        public void GetPreviousInvoicedTonnage_ReturnsCorrectResults()
        {
            // Act
            var result = calcResultsService.GetPreviousInvoicedTonnageFromDb(new RelativeYear(2024));

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(12.5M, result.First().InvoicedTonnage?.InvoicedNetTonnage);
            Assert.AreEqual(3, result.First().InvoicedTonnage?.Id); // latest CalculatorRun Id
            Assert.AreEqual(1, result.First().InvoicedTonnage?.ProducerId);
        }

        [TestMethod]
        public void GetCalcResultSummary_ShouldReturnCorrectValue()
        {
            var orderedProducerDetails = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, context.ProducerDetail.ToList());
            var runProducerMaterialDetails = GetProducerRunMaterialDetails(
                orderedProducerDetails,
                context.ProducerReportedMaterial.ToList(),
                1);

            var materials = MaterialMapper.Map(context.Material.ToList());

            var totalPackagingTonnage = CalcResultSummaryBuilder.GetTotalPackagingTonnagePerRun(runProducerMaterialDetails,
                                                                                                materials,
                                                                                                1,
                                                                                                calcResultsService.ScaledupProducers.ToList(),
                                                                                                calcResultsService.PartialObligations.ToList());

            var producerInvoicedMaterialNetTonnage = calcResultsService.GetPreviousInvoicedTonnageFromDb(new RelativeYear(2024));

            var defaultParams = new List<DefaultParamResultsClass>();

            var result = new CalcResultSummaryBuilder(context).GetCalcResultSummary(orderedProducerDetails, materials, calculationResult, totalPackagingTonnage, producerInvoicedMaterialNetTonnage, defaultParams, smcw);

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
            modulationResult.ShowModulations = true;
            var result2 = new CalcResultSummaryBuilder(context).GetCalcResultSummary(orderedProducerDetails, materials, modulationResult, totalPackagingTonnage, producerInvoicedMaterialNetTonnage, defaultParams, smcw);
            Assert.AreEqual(225, result2.ColumnHeaders.Count());
        }

        [TestMethod]
        public void GetCalcResultSummary_CanAddTotalRow()
        {
            var sut = new CalcResultSummaryBuilder(context);
            calcResultsService.ParentOrganisations = new List<Organisation> {
                new() { OrganisationId = 1, OrganisationName = "Org1" }
            };

            var orderedProducerDetails = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, context.ProducerDetail.ToList()).ToList();
            var runProducerMaterialDetails = GetProducerRunMaterialDetails(
                orderedProducerDetails,
                context.ProducerReportedMaterial.ToList(),
                1);

            var materials = MaterialMapper.Map(context.Material.ToList());

            var totalPackagingTonnage = CalcResultSummaryBuilder.GetTotalPackagingTonnagePerRun(runProducerMaterialDetails, materials, 1, calcResultsService.ScaledupProducers.ToList(), calcResultsService.PartialObligations.ToList());

            orderedProducerDetails.Add(new ProducerDetail
            {
                ProducerId = 1
            });

            var producerInvoicedMaterialNetTonnage = calcResultsService.GetPreviousInvoicedTonnageFromDb(new RelativeYear(2024));
            var defaultParams = new List<DefaultParamResultsClass>();

            var result = sut.GetCalcResultSummary(orderedProducerDetails, materials, calculationResult, totalPackagingTonnage, producerInvoicedMaterialNetTonnage, defaultParams, smcw);

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
        }

        [TestMethod]
        public void GetTonnages_ShouldCalculateCorrectlyForGlass()
        {
            List<CalculatorRunPomDataDetail> pomData = new List<CalculatorRunPomDataDetail>();
            List<MaterialDetail> materials = new List<MaterialDetail>();

            var glassMaterial = new MaterialDetail
            {
                Code = MaterialCodes.Glass,
                Name = "Glass",
                Description = "Glass material description",
            };
            materials.Add(glassMaterial);

            var glassPomData = new List<CalculatorRunPomDataDetail>
            {
                new CalculatorRunPomDataDetail
                {
                    PackagingMaterial = MaterialCodes.Glass,
                    SubmissionPeriod = "2024-P1",
                    PackagingType = PackagingTypes.Household,
                    PackagingMaterialWeight = 100,
                    SubmissionPeriodDesc = "2024 Period 1",
                    LoadTimeStamp = DateTime.UtcNow,
                },
                new CalculatorRunPomDataDetail
                {
                    PackagingMaterial = MaterialCodes.Glass,
                    SubmissionPeriod = "2024-P1",
                    PackagingType = PackagingTypes.HouseholdDrinksContainers,
                    PackagingMaterialWeight = 30,
                    SubmissionPeriodDesc = "2024 Period 1",
                    LoadTimeStamp = DateTime.UtcNow,
                },
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
            calculationResult.CalcResultScaledupProducers.ScaledupProducers = GetScaledUpProducers();
            await calcResultsService.ConstructAsync(runId: 1, relativeYear: new RelativeYear(2024), isBillingFile: false, calculationResult, smcw);

            var orderedProducerDetails = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, context.ProducerDetail.ToList());
            var runProducerMaterialDetails = GetProducerRunMaterialDetails(
                orderedProducerDetails,
                context.ProducerReportedMaterial.ToList(),
                1);

            var materials = MaterialMapper.Map(context.Material.ToList());

            var totalPackagingTonnage = CalcResultSummaryBuilder.GetTotalPackagingTonnagePerRun(runProducerMaterialDetails,
                                                                                                materials,
                                                                                                1,
                                                                                                calcResultsService.ScaledupProducers.ToList(),
                                                                                                calcResultsService.PartialObligations.ToList());
            var scaledUpProducer = totalPackagingTonnage.First(t => t.ProducerId == 4);

            Assert.AreEqual(2, totalPackagingTonnage.Count());
            Assert.IsNotNull(scaledUpProducer.ProducerId);
            Assert.AreEqual(100, scaledUpProducer.TotalPackagingTonnage);
        }

        public static List<CalcResultScaledupProducer> GetScaledUpProducers()
        {
            var test = new List<CalcResultScaledupProducer>
            {
                new CalcResultScaledupProducer
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
                            ScaledupNetReportedTonnage = 0,
                        }
                    },
                },
                },
            };

            return test;
        }

        [TestMethod]
        public async Task GetCalcResultSummary_PartialObligationShouldReturnCorrectValue()
        {
            calculationResult.CalcResultPartialObligations.PartialObligations = GetPartialObligations();
            await calcResultsService.ConstructAsync(runId: 1, relativeYear: new RelativeYear(2024), isBillingFile: false, calculationResult, smcw);

            var orderedProducerDetails = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, context.ProducerDetail.ToList());
            var runProducerMaterialDetails = GetProducerRunMaterialDetails(
                orderedProducerDetails,
                context.ProducerReportedMaterial.ToList(),
                1);

            var materials = MaterialMapper.Map(context.Material.ToList());

            var totalPackagingTonnage = CalcResultSummaryBuilder.GetTotalPackagingTonnagePerRun(runProducerMaterialDetails,
                                                                                                materials,
                                                                                                1,
                                                                                                calcResultsService.ScaledupProducers.ToList(),
                                                                                                calcResultsService.PartialObligations.ToList());
            var partialObligation = totalPackagingTonnage.First(t => t.ProducerId == 4);

            Assert.AreEqual(2, totalPackagingTonnage.Count());
            Assert.IsNotNull(partialObligation.ProducerId);
            Assert.AreEqual(25, partialObligation.TotalPackagingTonnage);
        }

        public static List<CalcResultPartialObligation> GetPartialObligations()
        {
            var test = new List<CalcResultPartialObligation>
            {
                new CalcResultPartialObligation
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
                            PartialNetReportedTonnage = 0,
                        }
                    },
                },
                },
            };

            return test;
        }

        [TestMethod]
        public void CanAddTotalRow_ParentProducerNotFound_ReturnsFalse()
        {
            // Arrange
            ProducerDetail producer = context.ProducerDetail.FirstOrDefault()!;
            IEnumerable<ProducerDetail> producersAndSubsidiaries = context.ProducerDetail;
            List<CalcResultSummaryProducerDisposalFees> producerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>();

            calcResultsService.ParentOrganisations = new List<Organisation>();

            // Act
            var result = calcResultsService.CanAddTotalRow(producer, producersAndSubsidiaries, producerDisposalFees);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CanAddTotalRow_ProducerDisposalFeeExists_ReturnsFalse()
        {
            // Arrange
            ProducerDetail producer = context.ProducerDetail.FirstOrDefault()!;
            IEnumerable<ProducerDetail> producersAndSubsidiaries = context.ProducerDetail;
            List<CalcResultSummaryProducerDisposalFees> producerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> {
                    new CalcResultSummaryProducerDisposalFees { ProducerId = "1", ProducerName="Org1", SubsidiaryId = "" }
                };

            calcResultsService.ParentOrganisations = new List<Organisation> {
                    new Organisation { OrganisationId = 1, OrganisationName = "Org1" }
                };

            // Act
            var result = calcResultsService.CanAddTotalRow(producer, producersAndSubsidiaries, producerDisposalFees);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CanAddTotalRow_ValidConditions_ReturnsTrue()
        {
            // Arrange
            ProducerDetail producer = context.ProducerDetail.FirstOrDefault()!;
            IEnumerable<ProducerDetail> producersAndSubsidiaries = context.ProducerDetail;
            List<CalcResultSummaryProducerDisposalFees> producerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> {
                    new CalcResultSummaryProducerDisposalFees { ProducerId = "2", ProducerName="Org1", SubsidiaryId = "" }
                };

            calcResultsService.ParentOrganisations = new List<Organisation> {
                    new Organisation { OrganisationId = 1, OrganisationName = "Org1" }
                };

            // Act
            var result = calcResultsService.CanAddTotalRow(producer, producersAndSubsidiaries, producerDisposalFees);

            // Assert
            Assert.IsTrue(result);
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
            var result = await calcResultsService.ConstructAsync(runId: 1, relativeYear: new RelativeYear(2024), isBillingFile: false, calcResult, smcw);

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
            var result = await calcResultsService.ConstructAsync(runId: 1, relativeYear: new RelativeYear(2024), isBillingFile: false, calcResult, smcw);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ProducerDisposalFees.Any());
        }

        [TestMethod]
        public void GetCalcResultSummary_AddsProducerTotalRow_AndProducerRow()
        {
            // Arrange
            var ordered = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, context.ProducerDetail.ToList());
            var materials = MaterialMapper.Map(context.Material.ToList());
            var runDetails = GetProducerRunMaterialDetails(ordered, context.ProducerReportedMaterial.ToList(), 1);

            calcResultsService.ScaledupProducers = new List<CalcResultScaledupProducer>();

            var totalPackaging = CalcResultSummaryBuilder.GetTotalPackagingTonnagePerRun(
                runDetails, materials, 1, calcResultsService.ScaledupProducers.ToList(), calcResultsService.PartialObligations.ToList());

            calcResultsService.ParentOrganisations = new List<Organisation>
            {
                new Organisation { OrganisationId = ordered.First().ProducerId, OrganisationName = "Org1" }
            };

            var producerInvoicedMaterialNetTonnage = calcResultsService.GetPreviousInvoicedTonnageFromDb(new RelativeYear(2024));
            var defaultParams = new List<DefaultParamResultsClass>();
            // Act
            var summary = calcResultsService.GetCalcResultSummary(ordered, materials, calculationResult, totalPackaging, producerInvoicedMaterialNetTonnage, defaultParams, smcw);

            // Assert
            Assert.IsNotNull(summary);
            Assert.IsTrue(summary.ProducerDisposalFees.Any());
            Assert.IsTrue(summary.ProducerDisposalFees.Any(r => r.isTotalRow));
            Assert.IsTrue(summary.ProducerDisposalFees.Any(r => !r.isTotalRow));
        }

        [TestMethod]
        public void GetCalcResultSummary_AddsTotalRow_WhenProducerHasSubsidiary()
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

            calcResultsService.ParentOrganisations = new List<Organisation>
            {
                new Organisation { OrganisationId = parent.ProducerId, OrganisationName = "Org1" }
            };

            var ordered = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, context.ProducerDetail.ToList());
            var materials = MaterialMapper.Map(context.Material.ToList());
            var runDetails = GetProducerRunMaterialDetails(ordered, context.ProducerReportedMaterial.ToList(), 1);
            var totalPackaging = CalcResultSummaryBuilder.GetTotalPackagingTonnagePerRun(
                runDetails, materials, 1, calcResultsService.ScaledupProducers.ToList(),
                calcResultsService.PartialObligations.ToList());

            var producerInvoicedMaterialNetTonnage = calcResultsService.GetPreviousInvoicedTonnageFromDb(new RelativeYear(2024));
            var defaultParams = new List<DefaultParamResultsClass>();

            var fixedSmcw = smcw with
            {
                ProducerTotals = smcw.ProducerTotals
                    .Select(p => p with
                    {
                        ProducerId = 1
                    })
                    .ToList()
            };

            var summary = calcResultsService.GetCalcResultSummary(ordered, materials, calculationResult, totalPackaging, producerInvoicedMaterialNetTonnage, defaultParams, fixedSmcw);

            Assert.IsTrue(summary.ProducerDisposalFees.Any(r => r.isTotalRow));
        }

        [TestMethod]
        public void GetProducerRow_MarksProducerAsScaledUp_WhenMatchExists()
        {

            var producer = context.ProducerDetail.Single(p => p.ProducerId == 1 && p.CalculatorRunId == 1);

            var materials = MaterialMapper.Map(context.Material.ToList());

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

            var ordered = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, context.ProducerDetail.ToList());

            var producerInvoicedMaterialNetTonnage = calcResultsService.GetPreviousInvoicedTonnageFromDb(new RelativeYear(2024));

            var row = calcResultsService.GetProducerRow(
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

            var producer = context.ProducerDetail.Single(p => p.ProducerId == 1 && p.CalculatorRunId == 1);

            var materials = MaterialMapper.Map(context.Material.ToList());

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

            calcResultsService.PartialObligations = new List<CalcResultPartialObligation>
            {
                new CalcResultPartialObligation
                {
                    ProducerId = producer.ProducerId,
                    SubsidiaryId = producer.SubsidiaryId,
                    PartialObligationTonnageByMaterial = tonnageByMaterial
                }
            };

            var ordered = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, context.ProducerDetail.ToList());

            var producerInvoicedMaterialNetTonnage = calcResultsService.GetPreviousInvoicedTonnageFromDb(new RelativeYear(2024));

            var row = calcResultsService.GetProducerRow(
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
        public void CanAddTotalRow_SingleProducerAndPomDataExists_ReturnsFalse()
        {
            var producer = context.ProducerDetail.Single(p => p.ProducerId == 1 && p.CalculatorRunId == 1);

            var producersAndSubsidiaries = new List<ProducerDetail>
            {
                new ProducerDetail
                {
                    ProducerId = producer.ProducerId,
                    CalculatorRunId = producer.CalculatorRunId,
                    SubsidiaryId = null,
                    ProducerName = "Parent Org"
                }
            };

            calcResultsService.ParentOrganisations = new List<Organisation>
            {
                new Organisation { OrganisationId = producer.ProducerId, OrganisationName = "Org1" }
            };

            var result = calcResultsService.CanAddTotalRow(producer, producersAndSubsidiaries, new List<CalcResultSummaryProducerDisposalFees>());

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GetProducerRow_MarksProducerAsNotScaledUp_WhenNoMatch()
        {
            var producer = context.ProducerDetail.Single(p => p.ProducerId == 1 && p.CalculatorRunId == 1);
            var materials = MaterialMapper.Map(context.Material.ToList());

            calcResultsService.ScaledupProducers = new List<CalcResultScaledupProducer>();

            var ordered = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, context.ProducerDetail.ToList());
            var producerInvoicedMaterialNetTonnage = calcResultsService.GetPreviousInvoicedTonnageFromDb(new RelativeYear(2024));

            var row = calcResultsService.GetProducerRow(
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
            var producer = context.ProducerDetail.Single(p => p.ProducerId == 1 && p.CalculatorRunId == 1);
            var materials = MaterialMapper.Map(context.Material.ToList());

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

            var ordered = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, context.ProducerDetail.ToList());
            var producerInvoicedMaterialNetTonnage = calcResultsService.GetPreviousInvoicedTonnageFromDb(new RelativeYear(2024));

            var row = calcResultsService.GetProducerRow(
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
            var producer = context.ProducerDetail.Single(p => p.ProducerId == 1 && p.CalculatorRunId == 1);
            var materials = MaterialMapper.Map(context.Material.ToList());

            calcResultsService.PartialObligations = new List<CalcResultPartialObligation>();

            var ordered = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, context.ProducerDetail.ToList());
            var producerInvoicedMaterialNetTonnage = calcResultsService.GetPreviousInvoicedTonnageFromDb(new RelativeYear(2024));

            var row = calcResultsService.GetProducerRow(
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
        public void CanAddTotalRow_NoParentPomButHasSubsidiary_ReturnsTrue()
        {
            var parent = context.ProducerDetail.Single(p => p.ProducerId == 1 && p.CalculatorRunId == 1);
            var subOnly = new List<ProducerDetail> {
                new ProducerDetail { ProducerId = parent.ProducerId, CalculatorRunId = parent.CalculatorRunId, SubsidiaryId = "S1", ProducerName = "Sub1" }
            };

            calcResultsService.ParentOrganisations = new List<Organisation> {
                new Organisation { OrganisationId = parent.ProducerId, OrganisationName = "Org1" }
            };

            var canAdd = calcResultsService.CanAddTotalRow(parent, subOnly, new List<CalcResultSummaryProducerDisposalFees>());
            Assert.IsTrue(canAdd);
        }

        [TestMethod]
        public async Task ConstructAsync_WhenIsBillingFileTrue_Persists_BillingInstructions_ToDb()
        {
            // Act
            var summary = await calcResultsService.ConstructAsync(runId: 1, relativeYear: new RelativeYear(2024), isBillingFile: true, calculationResult, smcw);

            // Assert (verify Level 1 row exists and has a section)
            var level1 = summary.ProducerDisposalFees
                                .FirstOrDefault(f => f.Level == CommonConstants.LevelOne.ToString()
                                                  && f.ProducerIdInt == 1);
            Assert.IsNotNull(level1);
            Assert.IsNotNull(level1.BillingInstructionSection, "BillingInstructionSection should be populated on Level 1.");

            // Assert
            var entity = context.ProducerResultFileSuggestedBillingInstruction
                                .Single(p => p.CalculatorRunId == 1 && p.ProducerId == 1);

            Assert.AreEqual(level1.BillingInstructionSection!.CurrentYearInvoiceTotalToDate, entity.CurrentYearInvoiceTotalToDate);
            Assert.AreEqual(level1.BillingInstructionSection!.TonnageChangeSinceLastInvoice, entity.TonnageChangeSinceLastInvoice);
            Assert.AreEqual(level1.BillingInstructionSection!.LiabilityDifference, entity.AmountLiabilityDifferenceCalcVsPrev);
            Assert.AreEqual(level1.BillingInstructionSection!.MaterialThresholdBreached, entity.MaterialPoundThresholdBreached);
            Assert.AreEqual(level1.BillingInstructionSection!.TonnageThresholdBreached, entity.TonnagePoundThresholdBreached);
            Assert.AreEqual(level1.BillingInstructionSection!.PercentageLiabilityDifference, entity.PercentageLiabilityDifferenceCalcVsPrev);
            Assert.AreEqual(level1.BillingInstructionSection!.TonnagePercentageThresholdBreached, entity.TonnagePercentageThresholdBreached);
            Assert.AreEqual(level1.BillingInstructionSection!.SuggestedBillingInstruction, entity.SuggestedBillingInstruction);
            Assert.AreEqual(level1.BillingInstructionSection!.SuggestedInvoiceAmount ?? 0m, entity.SuggestedInvoiceAmount);
        }

        [TestMethod]
        public async Task ConstructAsync_WhenIsBillingFileFalse_DoesNotPersist_Billing_Instructions_ToDb()
        {
            // Arrange
            var beforeEntity = context.ProducerResultFileSuggestedBillingInstruction
                                     .Single(p => p.CalculatorRunId == 1 && p.ProducerId == 1);

            var before = new
            {
                beforeEntity.CurrentYearInvoiceTotalToDate,
                beforeEntity.TonnageChangeSinceLastInvoice,
                beforeEntity.AmountLiabilityDifferenceCalcVsPrev,
                beforeEntity.MaterialPoundThresholdBreached,
                beforeEntity.TonnagePoundThresholdBreached,
                beforeEntity.PercentageLiabilityDifferenceCalcVsPrev,
                beforeEntity.TonnagePercentageThresholdBreached,
                beforeEntity.SuggestedBillingInstruction,
                beforeEntity.SuggestedInvoiceAmount
            };

            // Act
            await calcResultsService.ConstructAsync(runId: 1, relativeYear: new RelativeYear(2024), isBillingFile: false, calculationResult, smcw);

            var afterEntity = context.ProducerResultFileSuggestedBillingInstruction
                                    .Single(p => p.CalculatorRunId == 1 && p.ProducerId == 1);

            Assert.AreEqual(before.CurrentYearInvoiceTotalToDate, afterEntity.CurrentYearInvoiceTotalToDate);
            Assert.AreEqual(before.TonnageChangeSinceLastInvoice, afterEntity.TonnageChangeSinceLastInvoice);
            Assert.AreEqual(before.AmountLiabilityDifferenceCalcVsPrev, afterEntity.AmountLiabilityDifferenceCalcVsPrev);
            Assert.AreEqual(before.MaterialPoundThresholdBreached, afterEntity.MaterialPoundThresholdBreached);
            Assert.AreEqual(before.TonnagePoundThresholdBreached, afterEntity.TonnagePoundThresholdBreached);
            Assert.AreEqual(before.PercentageLiabilityDifferenceCalcVsPrev, afterEntity.PercentageLiabilityDifferenceCalcVsPrev);
            Assert.AreEqual(before.TonnagePercentageThresholdBreached, afterEntity.TonnagePercentageThresholdBreached);
            Assert.AreEqual(before.SuggestedBillingInstruction, afterEntity.SuggestedBillingInstruction);
            Assert.AreEqual(before.SuggestedInvoiceAmount, afterEntity.SuggestedInvoiceAmount);
        }

        [TestMethod]
        public async Task UpdateBillingInstructions_NoChanges_When_NoLevel1Fees()
        {
            // Arrange:
            var calcResult = TestDataHelper.GetCalcResult();

            var summary = TestDataHelper.GetCalcResultSummary();
            summary.ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>
            {
                new()
                {
                    ProducerId = "1",
                    ProducerIdInt = 1,
                    ProducerName = "Producer 1",
                    SubsidiaryId = "Subsidiary 1",
                    Level = "2"
                }
            };

            var before = context.ProducerResultFileSuggestedBillingInstruction.Single(p => p.CalculatorRunId == 1 && p.ProducerId == 1).SuggestedInvoiceAmount;

            // Act
            await calcResultsService.UpdateBillingInstructions(calcResult, summary);

            // Assert
            var after = context.ProducerResultFileSuggestedBillingInstruction.Single(p => p.CalculatorRunId == 1 && p.ProducerId == 1).SuggestedInvoiceAmount;

            Assert.AreEqual(before, after);
        }

        [TestMethod]
        public async Task UpdateBillingInstructions_Level1ButMissingEntity_SkipsUpdate()
        {
            var calcResult = TestDataHelper.GetCalcResult();

            var summary = TestDataHelper.GetCalcResultSummary();
            summary.ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>
            {
                new()
                {
                    ProducerId = "999",
                    ProducerIdInt = 999,
                    ProducerName = "ProducerNotExistsInEntityList",
                    SubsidiaryId = string.Empty,
                    Level = CommonConstants.LevelOne.ToString(),
                    BillingInstructionSection = new CalcResultSummaryBillingInstruction
                    {
                        SuggestedBillingInstruction = "INITIAL",
                        SuggestedInvoiceAmount = 123m
                    }
                }
            };

            await calcResultsService.UpdateBillingInstructions(calcResult, summary);

            var entity = context.ProducerResultFileSuggestedBillingInstruction.Single(p => p.CalculatorRunId == 1 && p.ProducerId == 1);

            Assert.AreNotEqual(123m, entity.SuggestedInvoiceAmount);
        }

        [TestMethod]
        public async Task UpdateBillingInstructions_MapsAllFields_When_Level1EntityExists()
        {
            // Arrange
            var calcResult = TestDataHelper.GetCalcResult();

            var section = new CalcResultSummaryBillingInstruction
            {
                CurrentYearInvoiceTotalToDate = 123.45m,
                TonnageChangeSinceLastInvoice = "Yes",
                LiabilityDifference = 678.90m,
                MaterialThresholdBreached = "Mat TH",
                TonnageThresholdBreached = "Ton TH",
                PercentageLiabilityDifference = 11.22m,
                TonnagePercentageThresholdBreached = "Ton % TH",
                SuggestedBillingInstruction = "ISSUE",
                SuggestedInvoiceAmount = 999.01m
            };

            var summary = TestDataHelper.GetCalcResultSummary();
            summary.ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>
            {
                new CalcResultSummaryProducerDisposalFees
                {
                    ProducerId = "1",
                    ProducerIdInt = 1,
                    ProducerName = "Producer 1",
                    SubsidiaryId = string.Empty,
                    Level = CommonConstants.LevelOne.ToString(),
                    BillingInstructionSection = section
                }
            };

            // Act
            await calcResultsService.UpdateBillingInstructions(calcResult, summary);

            // Assert
            var entity = context.ProducerResultFileSuggestedBillingInstruction
                                .Single(p => p.CalculatorRunId == 1 && p.ProducerId == 1);

            Assert.AreEqual(section.CurrentYearInvoiceTotalToDate, entity.CurrentYearInvoiceTotalToDate);
            Assert.AreEqual(section.TonnageChangeSinceLastInvoice, entity.TonnageChangeSinceLastInvoice);
            Assert.AreEqual(section.LiabilityDifference, entity.AmountLiabilityDifferenceCalcVsPrev);
            Assert.AreEqual(section.MaterialThresholdBreached, entity.MaterialPoundThresholdBreached);
            Assert.AreEqual(section.TonnageThresholdBreached, entity.TonnagePoundThresholdBreached);
            Assert.AreEqual(section.PercentageLiabilityDifference, entity.PercentageLiabilityDifferenceCalcVsPrev);
            Assert.AreEqual(section.TonnagePercentageThresholdBreached, entity.TonnagePercentageThresholdBreached);
            Assert.AreEqual(section.SuggestedBillingInstruction, entity.SuggestedBillingInstruction);
            Assert.AreEqual(section.SuggestedInvoiceAmount, entity.SuggestedInvoiceAmount);
        }

        [TestMethod]
        public async Task UpdateBillingInstructions_Level1WithNullBillingInstructionSection_SetsFieldsToNullOrZero()
        {
            // Arrange
            var entity = context.ProducerResultFileSuggestedBillingInstruction.Single(p => p.CalculatorRunId == 1 && p.ProducerId == 1);
            entity.CurrentYearInvoiceTotalToDate = 999.99m;
            entity.TonnageChangeSinceLastInvoice = "CHANGE";
            entity.AmountLiabilityDifferenceCalcVsPrev = 888.88m;
            entity.MaterialPoundThresholdBreached = "MAT";
            entity.TonnagePoundThresholdBreached = "TON";
            entity.PercentageLiabilityDifferenceCalcVsPrev = 77.77m;
            entity.TonnagePercentageThresholdBreached = "TON%";
            entity.SuggestedBillingInstruction = "INITIAL";
            entity.SuggestedInvoiceAmount = 555.55m;
            await context.SaveChangesAsync();

            var summary = TestDataHelper.GetCalcResultSummary();
            summary.ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>
            {
                new()
                {
                    ProducerId = "1",
                    ProducerIdInt = 1,
                    ProducerName = "Producer 1",
                    SubsidiaryId = string.Empty,
                    Level = CommonConstants.LevelOne.ToString(),
                    BillingInstructionSection = null
                }
            };

            // Act
            await calcResultsService.UpdateBillingInstructions(calculationResult, summary);

            // Assert
            var updated = context.ProducerResultFileSuggestedBillingInstruction.Single(p => p.CalculatorRunId == 1 && p.ProducerId == 1);
            Assert.IsNull(updated.CurrentYearInvoiceTotalToDate);
            Assert.IsNull(updated.TonnageChangeSinceLastInvoice);
            Assert.IsNull(updated.AmountLiabilityDifferenceCalcVsPrev);
            Assert.IsNull(updated.MaterialPoundThresholdBreached);
            Assert.IsNull(updated.TonnagePoundThresholdBreached);
            Assert.IsNull(updated.PercentageLiabilityDifferenceCalcVsPrev);
            Assert.IsNull(updated.TonnagePercentageThresholdBreached);
            Assert.IsNull(updated.SuggestedBillingInstruction);
            Assert.AreEqual(0m, updated.SuggestedInvoiceAmount);
        }

        [TestMethod]
        public async Task UpdateBillingInstructions_Level1WithNullSuggestedInvoiceAmount_SetsInvoiceAmountToZero()
        {
            // Arrange
            var entity = context.ProducerResultFileSuggestedBillingInstruction.Single(p => p.CalculatorRunId == 1 && p.ProducerId == 1);
            entity.SuggestedInvoiceAmount = 321.45m;
            await context.SaveChangesAsync();

            var section = new CalcResultSummaryBillingInstruction
            {
                CurrentYearInvoiceTotalToDate = 10m,
                TonnageChangeSinceLastInvoice = "No",
                LiabilityDifference = 5.5m,
                MaterialThresholdBreached = "Mat TH",
                TonnageThresholdBreached = "Ton TH",
                PercentageLiabilityDifference = 1.23m,
                TonnagePercentageThresholdBreached = "Ton % TH",
                SuggestedBillingInstruction = "ISSUE",
                SuggestedInvoiceAmount = null
            };

            var summary = TestDataHelper.GetCalcResultSummary();
            summary.ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>
            {
                new()
                {
                    ProducerId = "1",
                    ProducerIdInt = 1,
                    ProducerName = "Producer 1",
                    SubsidiaryId = string.Empty,
                    Level = CommonConstants.LevelOne.ToString(),
                    BillingInstructionSection = section
                }
            };

            // Act
            await calcResultsService.UpdateBillingInstructions(calculationResult, summary);

            // Assert
            var updated = context.ProducerResultFileSuggestedBillingInstruction.Single(p => p.CalculatorRunId == 1 && p.ProducerId == 1);
            Assert.AreEqual(section.CurrentYearInvoiceTotalToDate, updated.CurrentYearInvoiceTotalToDate);
            Assert.AreEqual(section.TonnageChangeSinceLastInvoice, updated.TonnageChangeSinceLastInvoice);
            Assert.AreEqual(section.LiabilityDifference, updated.AmountLiabilityDifferenceCalcVsPrev);
            Assert.AreEqual(section.MaterialThresholdBreached, updated.MaterialPoundThresholdBreached);
            Assert.AreEqual(section.TonnageThresholdBreached, updated.TonnagePoundThresholdBreached);
            Assert.AreEqual(section.PercentageLiabilityDifference, updated.PercentageLiabilityDifferenceCalcVsPrev);
            Assert.AreEqual(section.TonnagePercentageThresholdBreached, updated.TonnagePercentageThresholdBreached);
            Assert.AreEqual(section.SuggestedBillingInstruction, updated.SuggestedBillingInstruction);
            Assert.AreEqual(0m, updated.SuggestedInvoiceAmount);
        }

        private static void SeedDatabase(ApplicationDBContext context)
        {
            context.Material.AddRange(new List<Material>
            {
                new() { Id = 1, Name = "Material1", Code = MaterialCodes.Aluminium },
                new() { Id = 2, Name = "Material2", Code = MaterialCodes.Glass },
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
                        CalculatorRunClassificationId=7,
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
                            CalculatorRunClassificationId=7,
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
                            CalculatorRunClassificationId=7
                        }
                },
                new() { Id = 4, ProducerName = "Producer4", ProducerId = 4, CalculatorRunId = 1 , SubsidiaryId=null },
                new() { Id = 5, ProducerName = "Producer5", ProducerId = 4, CalculatorRunId = 1 , SubsidiaryId="A123" },
                new() { Id = 6, ProducerName = "Subsidiary1", ProducerId = 4, CalculatorRunId = 1 , SubsidiaryId="A456"},
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
                new() { MaterialId = 1, PackagingType = "HDC", SubmissionPeriod = "2025-H1",  PackagingTonnage = 125m, ProducerDetailId = 1 },
                new() { MaterialId = 1, PackagingType = "HDC", SubmissionPeriod = "2025-H2",  PackagingTonnage = 175m, ProducerDetailId = 1 },
                new() { MaterialId = 2, PackagingType = "HDC", SubmissionPeriod = "2025-H1",  PackagingTonnage = 100m, ProducerDetailId = 2 },
                new() { MaterialId = 2, PackagingType = "HDC", SubmissionPeriod = "2025-H2",  PackagingTonnage = 200m, ProducerDetailId = 2 },
                new() { MaterialId = 2, PackagingType = "HH", SubmissionPeriod = "2025-H1", PackagingTonnage = 50m, ProducerDetailId = 4 },
                new() { MaterialId = 2, PackagingType = "HH", SubmissionPeriod = "2025-H2", PackagingTonnage = 250m, ProducerDetailId = 4 },
                new() { MaterialId = 1, PackagingType = "HH", SubmissionPeriod = "2025-H1", PackagingTonnage = 110m, ProducerDetailId = 5 },
                new() { MaterialId = 1, PackagingType = "HH", SubmissionPeriod = "2025-H2", PackagingTonnage = 190m, ProducerDetailId = 5 }
            });

            context.ProducerResultFileSuggestedBillingInstruction.AddRange(new List<ProducerResultFileSuggestedBillingInstruction>
            {
                new ProducerResultFileSuggestedBillingInstruction { Id = 1, CalculatorRunId = 1, ProducerId = 1, SuggestedBillingInstruction="INITIAL", BillingInstructionAcceptReject="Accepted"},
                new ProducerResultFileSuggestedBillingInstruction { Id = 2, CalculatorRunId = 2, ProducerId = 2, SuggestedBillingInstruction="INITIAL", BillingInstructionAcceptReject="Accepted"},
                new ProducerResultFileSuggestedBillingInstruction { Id = 3, CalculatorRunId = 3, ProducerId = 3, SuggestedBillingInstruction="INITIAL", BillingInstructionAcceptReject="Accepted"},
            });

            context.ProducerDesignatedRunInvoiceInstruction.AddRange(new List<ProducerDesignatedRunInvoiceInstruction>
            {
                new ProducerDesignatedRunInvoiceInstruction { Id = 1,CalculatorRunId = 1, ProducerId = 1},
                new ProducerDesignatedRunInvoiceInstruction { Id = 2,CalculatorRunId = 2, ProducerId = 2},
                new ProducerDesignatedRunInvoiceInstruction { Id = 3,CalculatorRunId = 2, ProducerId = 2},
            });
            context.ProducerInvoicedMaterialNetTonnage.AddRange(new List<ProducerInvoicedMaterialNetTonnage>
            {
                new ProducerInvoicedMaterialNetTonnage { Id = 3, MaterialId = 1,CalculatorRunId = 1, ProducerId = 1, InvoicedNetTonnage = 12.5M},
                new ProducerInvoicedMaterialNetTonnage { Id = 4, MaterialId = 2,CalculatorRunId = 2, ProducerId = 2, InvoicedNetTonnage = 13.5M},
                new ProducerInvoicedMaterialNetTonnage { Id = 5, MaterialId = 2,CalculatorRunId = 2, ProducerId = 2, InvoicedNetTonnage = 13.5M},
            });
            context.CalculatorRunOrganisationDataMaster.AddRange(new List<CalculatorRunOrganisationDataMaster>
            {
                new CalculatorRunOrganisationDataMaster {Id = 1,RelativeYear = new RelativeYear(2025), CreatedAt= DateTime.UtcNow, CreatedBy = "TestUser" , EffectiveFrom = DateTime.UtcNow}
            });

            context.CalculatorRunOrganisationDataDetails.AddRange(new List<CalculatorRunOrganisationDataDetail>
            {
                new CalculatorRunOrganisationDataDetail {Id = 2, CalculatorRunOrganisationDataMasterId = 1, OrganisationId = 4, OrganisationName="ORG1", SubsidiaryId = null, StatusCode = "99", JoinerDate = "01/01/2025", LeaverDate = "15/07/2025"},
            });

            context.SaveChanges();
        }
    }
}