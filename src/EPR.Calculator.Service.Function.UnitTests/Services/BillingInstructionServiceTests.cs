using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.Telemetry;
using EPR.Calculator.Service.Function.UnitTests.Builder;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    [TestClass]
    public class BillingInstructionServiceTests
    {
        private BillingInstructionService _sut = null!;
        private Mock<ICalculatorTelemetryLogger> _telemetryLogger = null!;
        private IFixture _fixture = null!;

        [TestInitialize]
        public void SetUp()
        {
            _fixture = TestFixtures.New();
            _telemetryLogger = _fixture.Freeze<Mock<ICalculatorTelemetryLogger>>();

            _sut = _fixture.Create<BillingInstructionService>();
        }

        [TestMethod]
        public async Task CanCallCreateBillingInstructions()
        {
            // Arrange
            var calcResult = TestDataHelper.GetCalcResult();

            _telemetryLogger.Setup(mock => mock.LogInformation(It.IsAny<TrackMessage>())).Verifiable();
            _telemetryLogger.Setup(mock => mock.LogError(It.IsAny<ErrorMessage>())).Verifiable();

            // Act
            var result = await _sut.CreateBillingInstructions(calcResult);

            // Assert
            _telemetryLogger.Verify(mock => mock.LogInformation(It.IsAny<TrackMessage>()));

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task CanCallCreateBillingInstructionsWithNoInstructions()
        {
            // Arrange
            var calcResult = new CalcResult
            {
                ApplyModulation = true,
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
                    ByMaterial = []
                },
                CalcResultParameterOtherCost = new()
                {
                    SchemeSetupCost = new ByCountryCost
                    {
                        England = 0,
                        Wales = 0,
                        Scotland = 0,
                        NorthernIreland = 0
                    }
                },
                CalcResultLateReportingTonnageData = new()
                {
                    LateReportingTonnageByMaterial = []
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
            };


            _telemetryLogger.Setup(mock => mock.LogInformation(It.IsAny<TrackMessage>())).Verifiable();
            _telemetryLogger.Setup(mock => mock.LogError(It.IsAny<ErrorMessage>())).Verifiable();

            // Act
            var result = await _sut.CreateBillingInstructions(calcResult);

            // Assert
            _telemetryLogger.Verify(mock => mock.LogInformation(It.IsAny<TrackMessage>()));

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task CanCallCreateBillingInstructionsWithNoProducers()
        {
            // Arrange
            var calcResult = new CalcResult
            {
                ApplyModulation = true,
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
                    ByMaterial = []
                },
                CalcResultParameterOtherCost = new()
                {
                    SchemeSetupCost = new ByCountryCost
                    {
                        England = 0,
                        Wales = 0,
                        Scotland = 0,
                        NorthernIreland = 0
                    }
                },
                CalcResultLateReportingTonnageData = new()
                {
                    LateReportingTonnageByMaterial = []
                },
                CalcResultSummary  = new() {  ProducerDisposalFees = null! },
                CalcResultProjectedProducers = new CalcResultProjectedProducers(),
            };


            _telemetryLogger.Setup(mock => mock.LogInformation(It.IsAny<TrackMessage>())).Verifiable();
            _telemetryLogger.Setup(mock => mock.LogError(It.IsAny<ErrorMessage>())).Verifiable();

            // Act
            var result = await _sut.CreateBillingInstructions(calcResult);

            // Assert
            _telemetryLogger.Verify(mock => mock.LogError(It.IsAny<ErrorMessage>()));

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task CanCallCreateBillingInstructionsWithCancelledProducers()
        {
            // Arrange
            var calcResult = new CalcResult
            {
                ApplyModulation = true,
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
                    ByMaterial = []
                },
                CalcResultParameterOtherCost = new()
                {
                    SchemeSetupCost = new ByCountryCost
                    {
                        England = 0,
                        Wales = 0,
                        Scotland = 0,
                        NorthernIreland = 0
                    }
                },
                CalcResultLateReportingTonnageData = new()
                {
                    LateReportingTonnageByMaterial = []
                },
                CalcResultSummary = new()
                {
                    ProducerDisposalFees = _fixture.Create<List<CalcResultSummaryProducerDisposalFees>>()
                },
                CalcResultCancelledProducers = new CalcResultCancelledProducersResponse
                {
                    TitleHeader = CommonConstants.CancelledProducers,
                    CancelledProducers = new List<CalcResultCancelledProducersDto>
                    {
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
                    }
                },
                CalcResultProjectedProducers = new CalcResultProjectedProducers(),
            };


            _telemetryLogger.Setup(mock => mock.LogInformation(It.IsAny<TrackMessage>())).Verifiable();
            _telemetryLogger.Setup(mock => mock.LogError(It.IsAny<ErrorMessage>())).Verifiable();

            // Act
            var result = await _sut.CreateBillingInstructions(calcResult);

            Assert.IsTrue(result);
        }
    }
}
