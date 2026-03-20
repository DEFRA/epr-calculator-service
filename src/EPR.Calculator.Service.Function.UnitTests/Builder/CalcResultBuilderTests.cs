using AutoFixture;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Builder;
using EPR.Calculator.Service.Function.Builder.CancelledProducers;
using EPR.Calculator.Service.Function.Builder.CommsCost;
using EPR.Calculator.Service.Function.Builder.Detail;
using EPR.Calculator.Service.Function.Builder.ErrorReport;
using EPR.Calculator.Service.Function.Builder.LaDisposalCost;
using EPR.Calculator.Service.Function.Builder.Lapcap;
using EPR.Calculator.Service.Function.Builder.LateReportingTonnages;
using EPR.Calculator.Service.Function.Builder.OnePlusFourApportionment;
using EPR.Calculator.Service.Function.Builder.ParametersOther;
using EPR.Calculator.Service.Function.Builder.PartialObligations;
using EPR.Calculator.Service.Function.Builder.RejectedProducers;
using EPR.Calculator.Service.Function.Builder.ScaledupProducers;
using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Moq;

namespace EPR.Calculator.Service.Function.UnitTests
{
    [TestClass]
    public class CalcResultBuilderTests
    {
        private readonly Mock<ICalcResultDetailBuilder> mockCalcResultDetailBuilder;
        private readonly Mock<ICalcResultLapcapDataBuilder> mockLapcapBuilder;
        private readonly Mock<ICalcResultLateReportingBuilder> mockLateReportingBuilder;
        private readonly Mock<ICalcRunLaDisposalCostBuilder> mockCalcRunLaDisposalCostBuilder;
        private readonly Mock<ICalcResultCommsCostBuilder> mockCommsCostReportBuilder;
        private readonly Mock<ICalcResultSummaryBuilder> mockSummaryBuilder;
        private readonly CalcResultBuilder calcResultBuilder;

        private readonly Mock<ICalcResultParameterOtherCostBuilder> mockCalcResultParameterOtherCostBuilder;
        private readonly Mock<ICalcResultOnePlusFourApportionmentBuilder> mockOnePlusFourApportionmentBuilder;
        private readonly Mock<ICalcResultScaledupProducersBuilder> mockCalcResultScaledupProducersBuilder;
        private readonly Mock<ICalcResultPartialObligationBuilder> mockCalcResultPartialObligationBuilder;
        private readonly Mock<ICalcResultCancelledProducersBuilder> mockCalcResultCancelledProducersBuilder;
        private readonly Mock<ICalcResultRejectedProducersBuilder> mockCalcResultRejectedProducersBuilder;
        private readonly Mock<ICalcResultErrorReportBuilder> mockCalcResultErrorReportBuilder;
        private TelemetryClient telemetryClient;

        public CalcResultBuilderTests()
        {
            Fixture = new Fixture();
            mockCalcResultDetailBuilder = new Mock<ICalcResultDetailBuilder>();
            mockLapcapBuilder = new Mock<ICalcResultLapcapDataBuilder>();
            mockSummaryBuilder = new Mock<ICalcResultSummaryBuilder>();
            mockCalcRunLaDisposalCostBuilder = new Mock<ICalcRunLaDisposalCostBuilder>();
            mockCommsCostReportBuilder = new Mock<ICalcResultCommsCostBuilder>();
            mockLateReportingBuilder = new Mock<ICalcResultLateReportingBuilder>();
            mockCalcResultParameterOtherCostBuilder = new Mock<ICalcResultParameterOtherCostBuilder>();
            mockOnePlusFourApportionmentBuilder = new Mock<ICalcResultOnePlusFourApportionmentBuilder>();
            mockCalcResultScaledupProducersBuilder = new Mock<ICalcResultScaledupProducersBuilder>();
            mockCalcResultPartialObligationBuilder = new Mock<ICalcResultPartialObligationBuilder>();
            mockCalcResultCancelledProducersBuilder = new Mock<ICalcResultCancelledProducersBuilder>();
            mockCalcResultRejectedProducersBuilder = new Mock<ICalcResultRejectedProducersBuilder>();
            mockCalcResultErrorReportBuilder = new Mock<ICalcResultErrorReportBuilder>();

            telemetryClient = new TelemetryClient(new TelemetryConfiguration());

            calcResultBuilder = new CalcResultBuilder(
                mockCalcResultDetailBuilder.Object,
                mockLapcapBuilder.Object,
                mockCalcResultParameterOtherCostBuilder.Object,
                mockOnePlusFourApportionmentBuilder.Object,
                mockCommsCostReportBuilder.Object,
                mockLateReportingBuilder.Object,
                mockCalcRunLaDisposalCostBuilder.Object,
                mockCalcResultScaledupProducersBuilder.Object,
                mockCalcResultPartialObligationBuilder.Object,
                mockSummaryBuilder.Object,
                mockCalcResultCancelledProducersBuilder.Object,
                mockCalcResultRejectedProducersBuilder.Object,
                mockCalcResultErrorReportBuilder.Object,
                telemetryClient);
        }

        private Fixture Fixture { get; init; }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new CalcResultBuilder(
                mockCalcResultDetailBuilder.Object,
                mockLapcapBuilder.Object,
                mockCalcResultParameterOtherCostBuilder.Object,
                mockOnePlusFourApportionmentBuilder.Object,
                mockCommsCostReportBuilder.Object,
                mockLateReportingBuilder.Object,
                mockCalcRunLaDisposalCostBuilder.Object,
                mockCalcResultScaledupProducersBuilder.Object,
                mockCalcResultPartialObligationBuilder.Object,
                mockSummaryBuilder.Object,
                mockCalcResultCancelledProducersBuilder.Object,
                mockCalcResultRejectedProducersBuilder.Object,
                mockCalcResultErrorReportBuilder.Object,
                telemetryClient);

            // Assert
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void Build_ShouldReturnCalcResult()
        {
            var resultsRequestDto = new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2025)};
            var mockResultDetail = new Mock<CalcResultDetail>();
            var mockLapcapData = new Mock<CalcResultLapcapData>();
            var mockOtherParams = new Mock<CalcResultParameterOtherCost>();
            var mockOnePlusFourApp = new Mock<CalcResultOnePlusFourApportionment>();
            var mockCalcResultCommsCost = new Mock<CalcResultCommsCost>();
            var mockCalcResultLateReportingTonnage = Fixture.Create<CalcResultLateReportingTonnage>();
            var mockCalcResultLaDisposalCostData = new Mock<CalcResultLaDisposalCostData>();
            var mockCalcResultScaledUpProducersData = new Mock<CalcResultScaledupProducers>();
            var mockCalcResultPartialObligationsData = new Mock<CalcResultPartialObligations>();
            var mockCalcResultSummary = new Mock<CalcResultSummary>();

            mockCalcResultDetailBuilder.Setup(m => m.ConstructAsync(resultsRequestDto)).ReturnsAsync(mockResultDetail.Object);
            mockLapcapBuilder.Setup(m => m.ConstructAsync(resultsRequestDto)).ReturnsAsync(mockLapcapData.Object);
            mockCalcResultParameterOtherCostBuilder.Setup(m => m.ConstructAsync(resultsRequestDto))
                .ReturnsAsync(mockOtherParams.Object);
            mockOnePlusFourApportionmentBuilder.Setup(m => m.ConstructAsync(resultsRequestDto, It.IsAny<CalcResult>()))
                .Returns(mockOnePlusFourApp.Object);
            mockCommsCostReportBuilder
                .Setup(m => m.ConstructAsync(resultsRequestDto, It.IsAny<CalcResultOnePlusFourApportionment>(), It.IsAny<CalcResult>()))
                .ReturnsAsync(mockCalcResultCommsCost.Object);
            mockLateReportingBuilder.Setup(m => m.ConstructAsync(resultsRequestDto))
                .ReturnsAsync(mockCalcResultLateReportingTonnage);
            mockCalcRunLaDisposalCostBuilder.Setup(m => m.ConstructAsync(resultsRequestDto, It.IsAny<CalcResult>()))
                .ReturnsAsync(mockCalcResultLaDisposalCostData.Object);
            mockCalcResultScaledupProducersBuilder.Setup(m => m.ConstructAsync(resultsRequestDto))
                .ReturnsAsync(mockCalcResultScaledUpProducersData.Object);
            mockCalcResultPartialObligationBuilder.Setup(m => m.ConstructAsync(resultsRequestDto, mockCalcResultScaledUpProducersData.Object.ScaledupProducers ?? new List<CalcResultScaledupProducer>()))
                .ReturnsAsync(mockCalcResultPartialObligationsData.Object);
            mockSummaryBuilder.Setup(x => x.ConstructAsync(It.IsAny<int>(), It.IsAny<RelativeYear>(), It.IsAny<bool>(), It.IsAny<CalcResult>()))
                .ReturnsAsync(mockCalcResultSummary.Object);

            var results = calcResultBuilder.BuildAsync(resultsRequestDto);
            results.Wait();
            var result = results.Result;

            Assert.IsNotNull(result);
            Assert.AreEqual(mockResultDetail.Object, result.CalcResultDetail);
            Assert.AreEqual(mockLapcapData.Object, result.CalcResultLapcapData);
            Assert.AreEqual(mockOtherParams.Object, result.CalcResultParameterOtherCost);
            Assert.AreEqual(mockOnePlusFourApp.Object, result.CalcResultOnePlusFourApportionment);
            Assert.AreEqual(mockCalcResultCommsCost.Object, result.CalcResultCommsCostReportDetail);
            Assert.AreEqual(mockCalcResultLateReportingTonnage, result.CalcResultLateReportingTonnageData);
            Assert.AreEqual(mockCalcResultLaDisposalCostData.Object, result.CalcResultLaDisposalCostData);
            Assert.AreEqual(mockCalcResultScaledUpProducersData.Object, result.CalcResultScaledupProducers);
            Assert.AreEqual(mockCalcResultPartialObligationsData.Object, result.CalcResultPartialObligations);
            Assert.AreEqual(mockCalcResultSummary.Object, result.CalcResultSummary);

            mockCalcRunLaDisposalCostBuilder.Verify(m => m.ConstructAsync(resultsRequestDto, It.IsAny<CalcResult>()), Times.Once);

        }
    }
}
