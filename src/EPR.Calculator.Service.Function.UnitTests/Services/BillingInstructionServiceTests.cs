namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    using System;
    using System.Threading.Tasks;
    using AutoFixture;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.Service.Common.Logging;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Interface;
    using EPR.Calculator.Service.Function.Models;
    using EPR.Calculator.Service.Function.Services;
    using EPR.Calculator.Service.Function.UnitTests.Builder;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class BillingInstructionServiceTests
    {
        private BillingInstructionService _testClass = null!;
        private Mock<IDbLoadingChunkerService<ProducerResultFileSuggestedBillingInstruction>> _billingInstructionChunker = null!;
        private Mock<ICalculatorTelemetryLogger> _telemetryLogger = null!;

        [TestInitialize]
        public void SetUp()
        {
            _billingInstructionChunker = new Mock<IDbLoadingChunkerService<ProducerResultFileSuggestedBillingInstruction>>();
            _telemetryLogger = new Mock<ICalculatorTelemetryLogger>();
            _testClass = new BillingInstructionService(_billingInstructionChunker.Object, _telemetryLogger.Object);
        }

        [TestMethod]
        public async Task CanCallCreateBillingInstructions()
        {
            // Arrange
            var fixture = new Fixture();
            var calcResult = TestDataHelper.GetCalcResult();

            _telemetryLogger.Setup(mock => mock.LogInformation(It.IsAny<TrackMessage>())).Verifiable();
            _telemetryLogger.Setup(mock => mock.LogError(It.IsAny<ErrorMessage>())).Verifiable();

            // Act
            var result = await _testClass.CreateBillingInstructions(calcResult);

            // Assert
            _telemetryLogger.Verify(mock => mock.LogInformation(It.IsAny<TrackMessage>()));

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task CanCallCreateBillingInstructionsWithNoInstructions()
        {
            // Arrange
            var fixture = new Fixture();
            var calcResult = new CalcResult
            {
                CalcResultScaledupProducers = new CalcResultScaledupProducers(),
                CalcResultDetail = new CalcResultDetail
                {
                    RunId = 4,
                    RunDate = DateTime.Now,
                    RunName = "RunName",
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
                CalcResultSummary = new CalcResultSummary()
                {
                    ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>()
                    {
                        new ()
                        {
                            ProducerCommsFeesByMaterial = new Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial>() { },
                            ProducerDisposalFeesByMaterial = new Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial>() { },
                            ProducerId = "1",
                            ProducerName = "Test",
                            TotalProducerDisposalFeeWithBadDebtProvision = 100,
                            TotalProducerCommsFeeWithBadDebtProvision = 100,
                            SubsidiaryId = "1",
                            Level= CommonConstants.LevelTwo.ToString(),
                            BillingInstructionSection = new CalcResultSummaryBillingInstruction
                            {
                                CurrentYearInvoiceTotalToDate = "1000",
                                TonnageChangeSinceLastInvoice = "Tonnage Changed",
                                LiabilityDifference = "-200",
                                MaterialThresholdBreached = "-ve",
                                TonnageThresholdBreached = "-ve",
                                PercentageLiabilityDifference = "10.05",
                                MaterialPercentageThresholdBreached = "-ve",
                                TonnagePercentageThresholdBreached = "-ve",
                                SuggestedBillingInstruction = "INITIAL",
                                SuggestedInvoiceAmount = "500"
                            }
                        }
                    }
                }
            };


            _telemetryLogger.Setup(mock => mock.LogInformation(It.IsAny<TrackMessage>())).Verifiable();
            _telemetryLogger.Setup(mock => mock.LogError(It.IsAny<ErrorMessage>())).Verifiable();

            // Act
            var result = await _testClass.CreateBillingInstructions(calcResult);

            // Assert
            _telemetryLogger.Verify(mock => mock.LogInformation(It.IsAny<TrackMessage>()));

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task CanCallCreateBillingInstructionsWithNoProducers()
        {
            // Arrange
            var fixture = new Fixture();
            var calcResult = new CalcResult
            {
                CalcResultScaledupProducers = new CalcResultScaledupProducers(),
                CalcResultDetail = new CalcResultDetail
                {
                    RunId = 4,
                    RunDate = DateTime.Now,
                    RunName = "RunName",
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
                CalcResultSummary  = new() {  ProducerDisposalFees = null }
            };


            _telemetryLogger.Setup(mock => mock.LogInformation(It.IsAny<TrackMessage>())).Verifiable();
            _telemetryLogger.Setup(mock => mock.LogError(It.IsAny<ErrorMessage>())).Verifiable();

            // Act
            var result = await _testClass.CreateBillingInstructions(calcResult);

            // Assert
            _telemetryLogger.Verify(mock => mock.LogError(It.IsAny<ErrorMessage>()));

            Assert.IsFalse(result);
        }
       
    }
}