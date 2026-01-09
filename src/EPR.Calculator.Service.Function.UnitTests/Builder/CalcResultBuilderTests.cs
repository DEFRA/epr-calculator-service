namespace EPR.Calculator.Service.Function.UnitTests
{
    using AutoFixture;
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
    using EPR.Calculator.Service.Function.Builder.RejectedProducers;
    using EPR.Calculator.Service.Function.Builder.ScaledupProducers;
    using EPR.Calculator.Service.Function.Builder.PartialObligations;
    using EPR.Calculator.Service.Function.Builder.Summary;
    using EPR.Calculator.Service.Function.Dtos;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

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
            this.Fixture = new Fixture();
            this.mockCalcResultDetailBuilder = new Mock<ICalcResultDetailBuilder>();
            this.mockLapcapBuilder = new Mock<ICalcResultLapcapDataBuilder>();
            this.mockSummaryBuilder = new Mock<ICalcResultSummaryBuilder>();
            this.mockCalcRunLaDisposalCostBuilder = new Mock<ICalcRunLaDisposalCostBuilder>();
            this.mockCommsCostReportBuilder = new Mock<ICalcResultCommsCostBuilder>();
            this.mockLateReportingBuilder = new Mock<ICalcResultLateReportingBuilder>();
            this.mockCalcResultParameterOtherCostBuilder = new Mock<ICalcResultParameterOtherCostBuilder>();
            this.mockOnePlusFourApportionmentBuilder = new Mock<ICalcResultOnePlusFourApportionmentBuilder>();
            this.mockCalcResultScaledupProducersBuilder = new Mock<ICalcResultScaledupProducersBuilder>();
            this.mockCalcResultPartialObligationBuilder = new Mock<ICalcResultPartialObligationBuilder>();
            this.mockCalcResultCancelledProducersBuilder = new Mock<ICalcResultCancelledProducersBuilder>();
            this.mockCalcResultRejectedProducersBuilder = new Mock<ICalcResultRejectedProducersBuilder>();
            this.mockCalcResultErrorReportBuilder = new Mock<ICalcResultErrorReportBuilder>();

            this.telemetryClient = new TelemetryClient(new TelemetryConfiguration());

            this.calcResultBuilder = new CalcResultBuilder(
                this.mockCalcResultDetailBuilder.Object,
                this.mockLapcapBuilder.Object,
                this.mockCalcResultParameterOtherCostBuilder.Object,
                this.mockOnePlusFourApportionmentBuilder.Object,
                this.mockCommsCostReportBuilder.Object,
                this.mockLateReportingBuilder.Object,
                this.mockCalcRunLaDisposalCostBuilder.Object,
                this.mockCalcResultScaledupProducersBuilder.Object,
                this.mockCalcResultPartialObligationBuilder.Object,
                this.mockSummaryBuilder.Object,
                this.mockCalcResultCancelledProducersBuilder.Object,
                this.mockCalcResultRejectedProducersBuilder.Object,
                this.mockCalcResultErrorReportBuilder.Object,
                this.telemetryClient);
        }

        private Fixture Fixture { get; init; }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new CalcResultBuilder(
                this.mockCalcResultDetailBuilder.Object,
                this.mockLapcapBuilder.Object,
                this.mockCalcResultParameterOtherCostBuilder.Object,
                this.mockOnePlusFourApportionmentBuilder.Object,
                this.mockCommsCostReportBuilder.Object,
                this.mockLateReportingBuilder.Object,
                this.mockCalcRunLaDisposalCostBuilder.Object,
                this.mockCalcResultScaledupProducersBuilder.Object,
                this.mockCalcResultPartialObligationBuilder.Object,
                this.mockSummaryBuilder.Object,
                this.mockCalcResultCancelledProducersBuilder.Object,
                this.mockCalcResultRejectedProducersBuilder.Object,
                this.mockCalcResultErrorReportBuilder.Object,
                this.telemetryClient);

            // Assert
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void Build_ShouldReturnCalcResult()
        {
            var resultsRequestDto = new CalcResultsRequestDto();
            var mockResultDetail = new Mock<CalcResultDetail>();
            var mockLapcapData = new Mock<CalcResultLapcapData>();
            var mockOtherParams = new Mock<CalcResultParameterOtherCost>();
            var mockOnePlusFourApp = new Mock<CalcResultOnePlusFourApportionment>();
            var mockCalcResultCommsCost = new Mock<CalcResultCommsCost>();
            var mockCalcResultLateReportingTonnage = this.Fixture.Create<CalcResultLateReportingTonnage>();
            var mockCalcResultLaDisposalCostData = new Mock<CalcResultLaDisposalCostData>();
            var mockCalcResultScaledUpProducersData = new Mock<CalcResultScaledupProducers>();
            var mockCalcResultPartialObligationsData = new Mock<CalcResultPartialObligations>();
            var mockCalcResultSummary = new Mock<CalcResultSummary>();

            this.mockCalcResultDetailBuilder.Setup(m => m.ConstructAsync(resultsRequestDto)).ReturnsAsync(mockResultDetail.Object);
            this.mockLapcapBuilder.Setup(m => m.ConstructAsync(resultsRequestDto)).ReturnsAsync(mockLapcapData.Object);
            this.mockCalcResultParameterOtherCostBuilder.Setup(m => m.ConstructAsync(resultsRequestDto))
                .ReturnsAsync(mockOtherParams.Object);
            this.mockOnePlusFourApportionmentBuilder.Setup(m => m.ConstructAsync(resultsRequestDto, It.IsAny<CalcResult>()))
                .Returns(mockOnePlusFourApp.Object);
            this.mockCommsCostReportBuilder
                .Setup(m => m.ConstructAsync(resultsRequestDto, It.IsAny<CalcResultOnePlusFourApportionment>(), It.IsAny<CalcResult>()))
                .ReturnsAsync(mockCalcResultCommsCost.Object);
            this.mockLateReportingBuilder.Setup(m => m.ConstructAsync(resultsRequestDto))
                .ReturnsAsync(mockCalcResultLateReportingTonnage);
            this.mockCalcRunLaDisposalCostBuilder.Setup(m => m.ConstructAsync(resultsRequestDto, It.IsAny<CalcResult>()))
                .ReturnsAsync(mockCalcResultLaDisposalCostData.Object);
            this.mockCalcResultScaledupProducersBuilder.Setup(m => m.ConstructAsync(resultsRequestDto))
                .ReturnsAsync(mockCalcResultScaledUpProducersData.Object);
            this.mockCalcResultPartialObligationBuilder.Setup(m => m.ConstructAsync(resultsRequestDto, mockCalcResultScaledUpProducersData.Object.ScaledupProducers ?? new List<CalcResultScaledupProducer>()))
                .ReturnsAsync(mockCalcResultPartialObligationsData.Object);
            this.mockSummaryBuilder.Setup(x => x.ConstructAsync(resultsRequestDto, It.IsAny<CalcResult>()))
                .ReturnsAsync(mockCalcResultSummary.Object);

            var results = this.calcResultBuilder.BuildAsync(resultsRequestDto);
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

            this.mockCalcRunLaDisposalCostBuilder.Verify(m => m.ConstructAsync(resultsRequestDto, It.IsAny<CalcResult>()), Times.Once);

        }
    }
}
