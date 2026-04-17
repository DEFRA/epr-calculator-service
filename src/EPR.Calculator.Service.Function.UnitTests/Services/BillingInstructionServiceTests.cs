using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Features.Billing.Contexts;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Utils;
using Microsoft.Extensions.Logging;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    [TestClass]
    public class BillingInstructionServiceTests
    {
        private BillingInstructionService _testClass = null!;
        private ApplicationDBContext _dbContext = null!;
        private Mock<ILogger<BillingInstructionService>> _logger = null!;

        [TestInitialize]
        public void SetUp()
        {
            _dbContext = TestFixtures.New().Create<ApplicationDBContext>();
            _logger = new Mock<ILogger<BillingInstructionService>>();
            _testClass = new BillingInstructionService(_dbContext, new TestBulkOps(), _logger.Object);
        }

        [TestMethod]
        public async Task CanCallCreateBillingInstructions()
        {
            // Arrange
            var runContext = TestFixtures.Default.Create<BillingRunContext>();
            var calcResult = TestData.GetCalcResult();

            // Act/Assert
            await Should.NotThrowAsync(() => _testClass.CreateBillingInstructions(runContext, calcResult));
        }

        [TestMethod]
        public async Task CanCallCreateBillingInstructionsWithNoInstructions()
        {
            // Arrange
            var runContext = TestFixtures.Default.Create<BillingRunContext>();
            var calcResult = new CalcResult
            {
                CalcResultScaledupProducers = new CalcResultScaledupProducers(),
                CalcResultPartialObligations = new CalcResultPartialObligations(),
                CalcResultDetail = new CalcResultDetail
                {
                    RunId = 4,
                    RunDate = DateTime.UtcNow,
                    RunName = "RunName",
                    RelativeYear = new RelativeYear(2024),
                },
                CalcResultLapcapData = new CalcResultLapcapData
                {
                    Name = string.Empty,
                    CalcResultLapcapDataDetails = new List<CalcResultLapcapDataDetails>(),
                },
                CalcResultParameterOtherCost = new()
                {
                    BadDebtProvision = new KeyValuePair<string, string>(),
                    Name = string.Empty,
                    Details = new List<CalcResultParameterOtherCostDetail>(),
                    Materiality = new List<CalcResultMateriality>(),
                    SaOperatingCost = new List<CalcResultParameterOtherCostDetail>(),
                    SchemeSetupCost = new CalcResultParameterOtherCostDetail(),
                },
                CalcResultLateReportingTonnageData = new()
                {
                    Name = string.Empty,
                    CalcResultLateReportingTonnageDetails = new List<CalcResultLateReportingTonnageDetail>(),
                    MaterialHeading = string.Empty,
                    TonnageHeading = string.Empty,
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
                            Level= CommonConstants.LevelTwo.ToString(),
                            BillingInstructionSection = new CalcResultSummaryBillingInstruction
                            {
                                CurrentYearInvoiceTotalToDate = 1000m,
                                TonnageChangeSinceLastInvoice = "Tonnage Changed",
                                LiabilityDifference = -200,
                                MaterialThresholdBreached = "-ve",
                                TonnageThresholdBreached = "-ve",
                                PercentageLiabilityDifference = 10.05m,
                                MaterialPercentageThresholdBreached = "-ve",
                                TonnagePercentageThresholdBreached = "-ve",
                                SuggestedBillingInstruction = "INITIAL",
                                SuggestedInvoiceAmount = 500m
                            }
                        }
                    }
                },
                CalcResultProjectedProducers = new CalcResultProjectedProducers(),
                CalcResultModulation = null,
            };

            // Act/Assert
            await Should.ThrowAsync<Exception>(() => _testClass.CreateBillingInstructions(runContext, calcResult));
        }

        [TestMethod]
        public async Task CanCallCreateBillingInstructionsWithNoProducers()
        {
            // Arrange
            var runContext = TestFixtures.Default.Create<BillingRunContext>();
            var calcResult = new CalcResult
            {
                CalcResultScaledupProducers = new CalcResultScaledupProducers(),
                CalcResultPartialObligations = new CalcResultPartialObligations(),
                CalcResultDetail = new CalcResultDetail
                {
                    RunId = 4,
                    RunDate = DateTime.UtcNow,
                    RunName = "RunName",
                    RelativeYear = new RelativeYear(2024),
                },
                CalcResultLapcapData = new CalcResultLapcapData
                {
                    Name = string.Empty,
                    CalcResultLapcapDataDetails = new List<CalcResultLapcapDataDetails>(),
                },
                CalcResultParameterOtherCost = new()
                {
                    BadDebtProvision = new KeyValuePair<string, string>(),
                    Name = string.Empty,
                    Details = new List<CalcResultParameterOtherCostDetail>(),
                    Materiality = new List<CalcResultMateriality>(),
                    SaOperatingCost = new List<CalcResultParameterOtherCostDetail>(),
                    SchemeSetupCost = new CalcResultParameterOtherCostDetail(),
                },
                CalcResultLateReportingTonnageData = new()
                {
                    Name = string.Empty,
                    CalcResultLateReportingTonnageDetails = new List<CalcResultLateReportingTonnageDetail>(),
                    MaterialHeading = string.Empty,
                    TonnageHeading = string.Empty,
                },
                CalcResultSummary  = new() {  ProducerDisposalFees = null! },
                CalcResultProjectedProducers = new CalcResultProjectedProducers(),
                CalcResultModulation = null,
            };

            // Act/Assert
            await Should.ThrowAsync<Exception>(() => _testClass.CreateBillingInstructions(runContext, calcResult));
        }

        [TestMethod]
        public async Task CanCallCreateBillingInstructionsWithCancelledProducers()
        {
            // Arrange
            var runContext = TestFixtures.Default.Create<BillingRunContext>();
            var calcResult = new CalcResult
            {
                CalcResultScaledupProducers = new CalcResultScaledupProducers(),
                CalcResultPartialObligations = new CalcResultPartialObligations(),
                CalcResultDetail = new CalcResultDetail
                {
                    RunId = 4,
                    RunDate = DateTime.UtcNow,
                    RunName = "RunName",
                    RelativeYear = new RelativeYear(2024),
                },
                CalcResultLapcapData = new CalcResultLapcapData
                {
                    Name = string.Empty,
                    CalcResultLapcapDataDetails = new List<CalcResultLapcapDataDetails>(),
                },
                CalcResultParameterOtherCost = new()
                {
                    BadDebtProvision = new KeyValuePair<string, string>(),
                    Name = string.Empty,
                    Details = new List<CalcResultParameterOtherCostDetail>(),
                    Materiality = new List<CalcResultMateriality>(),
                    SaOperatingCost = new List<CalcResultParameterOtherCostDetail>(),
                    SchemeSetupCost = new CalcResultParameterOtherCostDetail(),
                },
                CalcResultLateReportingTonnageData = new()
                {
                    Name = string.Empty,
                    CalcResultLateReportingTonnageDetails = new List<CalcResultLateReportingTonnageDetail>(),
                    MaterialHeading = string.Empty,
                    TonnageHeading = string.Empty,
                },
                CalcResultSummary = new()
                {
                    ProducerDisposalFees = TestFixtures.Default.Create<List<CalcResultSummaryProducerDisposalFees>>()
                },
                CalcResultCancelledProducers = new CalcResultCancelledProducersResponse
                {
                    TitleHeader = CommonConstants.CancelledProducers,
                    CancelledProducers = ImmutableArray.Create(
                        new CalcResultCancelledProducersDto
                        {
                            LastTonnage = null,
                            ProducerId = 1,
                            TradingNameValue = "Test",
                            LatestInvoice = new LatestInvoice
                            {
                                BillingInstructionIdValue = "1_1",
                                RunNameValue = "RunName",
                                RunNumberValue = "4"
                            },
                        }
                    )
                },
                CalcResultProjectedProducers = new CalcResultProjectedProducers(),
                CalcResultModulation = null,
            };

            // Act/Assert
            await Should.NotThrowAsync(() => _testClass.CreateBillingInstructions(runContext, calcResult));
        }
    }
}