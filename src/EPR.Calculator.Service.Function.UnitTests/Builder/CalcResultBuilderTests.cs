using AutoFixture;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Builder.Modulation;
using EPR.Calculator.API.Data.DataModels;
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
using EPR.Calculator.Service.Function.Builder.ProjectedProducers;
using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Services;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Moq;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.UnitTests
{
    [TestClass]
    public class CalcResultBuilderTests
    {
        private readonly Mock<IParameterService> mockParameterService;
        private readonly Mock<IMaterialService> mockMaterialService;
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
        private readonly Mock<ICalcResultProjectedProducersBuilder> mockCalcResultProjectedProducersBuilder;
        private readonly Mock<ICalcResultCancelledProducersBuilder> mockCalcResultCancelledProducersBuilder;
        private readonly Mock<ICalcResultRejectedProducersBuilder> mockCalcResultRejectedProducersBuilder;
        private readonly Mock<ICalcResultErrorReportBuilder> mockCalcResultErrorReportBuilder;
        private readonly Mock<IProjectedProducersService> mockProjectedProducerService;
        private readonly Mock<ISelfManagedConsumerWasteService> mockSelfManagedConsumerWasteService;
        private readonly Mock<ICalcResultModulationBuilder> mockModulationBuilder;
        private readonly Mock<IReportedProducerService> mockReportedProducerService;
        private TelemetryClient telemetryClient;

        public CalcResultBuilderTests()
        {
            Fixture = new Fixture();
            mockParameterService = new Mock<IParameterService>();
            mockMaterialService = new Mock<IMaterialService>();
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
            mockCalcResultProjectedProducersBuilder = new Mock<ICalcResultProjectedProducersBuilder>();
            mockCalcResultCancelledProducersBuilder = new Mock<ICalcResultCancelledProducersBuilder>();
            mockCalcResultRejectedProducersBuilder = new Mock<ICalcResultRejectedProducersBuilder>();
            mockCalcResultErrorReportBuilder = new Mock<ICalcResultErrorReportBuilder>();
            mockProjectedProducerService = new Mock<IProjectedProducersService>();
            mockSelfManagedConsumerWasteService = new Mock<ISelfManagedConsumerWasteService>();
            mockModulationBuilder = new Mock<ICalcResultModulationBuilder>();
            mockReportedProducerService = new Mock<IReportedProducerService>();

            telemetryClient = new TelemetryClient(new TelemetryConfiguration());

            calcResultBuilder = new CalcResultBuilder(
                mockParameterService.Object,
                mockMaterialService.Object,
                mockCalcResultDetailBuilder.Object,
                mockLapcapBuilder.Object,
                mockCalcResultParameterOtherCostBuilder.Object,
                mockOnePlusFourApportionmentBuilder.Object,
                mockCommsCostReportBuilder.Object,
                mockLateReportingBuilder.Object,
                mockCalcRunLaDisposalCostBuilder.Object,
                mockCalcResultScaledupProducersBuilder.Object,
                mockCalcResultPartialObligationBuilder.Object,
                mockCalcResultProjectedProducersBuilder.Object,
                mockSummaryBuilder.Object,
                mockCalcResultCancelledProducersBuilder.Object,
                mockCalcResultRejectedProducersBuilder.Object,
                mockCalcResultErrorReportBuilder.Object,
                mockProjectedProducerService.Object,
                mockSelfManagedConsumerWasteService.Object,
                mockModulationBuilder.Object,
                mockReportedProducerService.Object,
                telemetryClient);
        }

        private Fixture Fixture { get; init; }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new CalcResultBuilder(
                mockParameterService.Object,
                mockMaterialService.Object,
                mockCalcResultDetailBuilder.Object,
                mockLapcapBuilder.Object,
                mockCalcResultParameterOtherCostBuilder.Object,
                mockOnePlusFourApportionmentBuilder.Object,
                mockCommsCostReportBuilder.Object,
                mockLateReportingBuilder.Object,
                mockCalcRunLaDisposalCostBuilder.Object,
                mockCalcResultScaledupProducersBuilder.Object,
                mockCalcResultPartialObligationBuilder.Object,
                mockCalcResultProjectedProducersBuilder.Object,
                mockSummaryBuilder.Object,
                mockCalcResultCancelledProducersBuilder.Object,
                mockCalcResultRejectedProducersBuilder.Object,
                mockCalcResultErrorReportBuilder.Object,
                mockProjectedProducerService.Object,
                mockSelfManagedConsumerWasteService.Object,
                mockModulationBuilder.Object,
                mockReportedProducerService.Object,
                telemetryClient);

            // Assert
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public async Task Build_ShouldReturnCalcResult()
        {
            var resultsRequestDto = new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2025)};
            var mockProducers1 = new List<ProducerDetail>
            {
                new ProducerDetail { ProducerId = 1, SubsidiaryId = null }
            };
            var mockProducers2 = new List<ProducerDetail>
            {
                new ProducerDetail { ProducerId = 2, SubsidiaryId = null }
            };
            var mockResultDetail = new Mock<CalcResultDetail>();
            var mockLapcapData = new Mock<CalcResultLapcapData>();
            var mockOtherParams = new Mock<CalcResultParameterOtherCost>();
            var mockOnePlusFourApp = new Mock<CalcResultOnePlusFourApportionment>();
            var mockCalcResultCommsCost = new Mock<CalcResultCommsCost>();
            var mockCalcResultLateReportingTonnage = Fixture.Create<CalcResultLateReportingTonnage>();
            var mockCalcResultLaDisposalCostData = new Mock<CalcResultLaDisposalCostData>();
            var mockCalcResultScaledUpProducersData = new Mock<CalcResultScaledupProducers>();
            var mockCalcResultPartialObligationsData = new Mock<CalcResultPartialObligations>();
            var mockCalcResultProjectedProducersData = new Mock<CalcResultProjectedProducers>();
            var mockCalcResultSummary = new Mock<CalcResultSummary>();

            mockCalcResultDetailBuilder.Setup(m => m.ConstructAsync(resultsRequestDto)).ReturnsAsync(mockResultDetail.Object);
            mockLapcapBuilder.Setup(m => m.ConstructAsync(resultsRequestDto)).ReturnsAsync(mockLapcapData.Object);
            mockCalcResultParameterOtherCostBuilder.Setup(m => m.ConstructAsync(resultsRequestDto))
                .ReturnsAsync(mockOtherParams.Object);
            mockOnePlusFourApportionmentBuilder.Setup(m => m.ConstructAsync(resultsRequestDto, It.IsAny<CalcResult>()))
                .Returns(mockOnePlusFourApp.Object);
            mockCommsCostReportBuilder
                .Setup(m => m.ConstructAsync(resultsRequestDto, It.IsAny<CalcResultOnePlusFourApportionment>(), It.IsAny<CalcResultLateReportingTonnage>()))
                .ReturnsAsync(mockCalcResultCommsCost.Object);
            mockLateReportingBuilder.Setup(m => m.ConstructAsync(resultsRequestDto))
                .ReturnsAsync(mockCalcResultLateReportingTonnage);
            mockCalcRunLaDisposalCostBuilder.Setup(m => m.ConstructAsync(resultsRequestDto, It.IsAny<List<MaterialDetail>>(), It.IsAny<CalcResultLapcapData>(), It.IsAny<CalcResultLateReportingTonnage>(), It.IsAny<SelfManagedConsumerWaste>(), It.IsAny<bool>()))
                .ReturnsAsync(mockCalcResultLaDisposalCostData.Object);
            mockReportedProducerService.Setup(m => m.GetProducers(resultsRequestDto.RunId))
                .ReturnsAsync(mockProducers1);
            mockCalcResultScaledupProducersBuilder.Setup(m => m.ConstructAsync(resultsRequestDto, mockProducers1))
                .ReturnsAsync((mockProducers2, mockCalcResultScaledUpProducersData.Object));
            mockCalcResultPartialObligationBuilder.Setup(m => m.ConstructAsync(resultsRequestDto, mockProducers2))
                .ReturnsAsync((mockProducers2, mockCalcResultPartialObligationsData.Object));
            mockSummaryBuilder.Setup(x => x.ConstructAsync(It.IsAny<int>(), It.IsAny<RelativeYear>(), It.IsAny<bool>(), It.IsAny<CalcResult>(), It.IsAny<SelfManagedConsumerWaste>()))
                .ReturnsAsync(mockCalcResultSummary.Object);

            mockMaterialService.Setup(x => x.GetMaterials())
                .ReturnsAsync(new List<MaterialDetail>());
            mockSelfManagedConsumerWasteService.Setup(x => x.Calculate(
                    It.IsAny<CalcResultsRequestDto>(),
                    It.IsAny<IEnumerable<MaterialDetail>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new SelfManagedConsumerWaste
                {
                    ProducerTotals = [],
                    OverallTotalPerMaterials = []
                });

            var result = await calcResultBuilder.BuildAsync(resultsRequestDto);

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
            Assert.AreNotEqual(mockCalcResultProjectedProducersData.Object, result.CalcResultProjectedProducers);
            Assert.AreEqual(mockCalcResultSummary.Object, result.CalcResultSummary);

            mockCalcRunLaDisposalCostBuilder.Verify(m => m.ConstructAsync(resultsRequestDto, It.IsAny<List<MaterialDetail>>(), It.IsAny<CalcResultLapcapData>(), It.IsAny<CalcResultLateReportingTonnage>(), It.IsAny<SelfManagedConsumerWaste>(), It.IsAny<bool>()), Times.Once);
            mockCalcResultScaledupProducersBuilder.Verify(m => m.ConstructAsync(resultsRequestDto, It.IsAny<List<ProducerDetail>>()), Times.Once);
            mockCalcResultProjectedProducersBuilder.Verify(m => m.ConstructAsync(resultsRequestDto, It.IsAny<List<ProducerDetail>>()), Times.Never);
            mockCalcResultPartialObligationBuilder.Verify(m => m.ConstructAsync(resultsRequestDto, It.IsAny<List<ProducerDetail>>()), Times.Once);
        }

        [TestMethod]
        public async Task Build_ShouldReturnCalcResult_WithProjectedProducers()
        {
            var resultsRequestDto = new CalcResultsRequestDto() { RunId = 1, RelativeYear = new RelativeYear(2026), IsBillingFile = false };
            var mockProducers1 = new List<ProducerDetail>
            {
                new ProducerDetail { ProducerId = 1, SubsidiaryId = null }
            };
            var mockProducers2 = new List<ProducerDetail>
            {
                new ProducerDetail { ProducerId = 2, SubsidiaryId = null }
            };

            var mockCalcResultScaledUpProducersData = new Mock<CalcResultScaledupProducers>();
            var mockCalcResultProjectedProducersData = new Mock<CalcResultProjectedProducers>();
            var mockCalcResultPartialObligationsData = new Mock<CalcResultPartialObligations>();

            mockReportedProducerService.Setup(m => m.GetProducers(resultsRequestDto.RunId))
                .ReturnsAsync(mockProducers1);
            mockCalcResultProjectedProducersBuilder.Setup(m => m.ConstructAsync(resultsRequestDto, mockProducers1))
                .ReturnsAsync((mockProducers2, mockCalcResultProjectedProducersData.Object));
            mockCalcResultPartialObligationBuilder.Setup(m => m.ConstructAsync(resultsRequestDto, mockProducers2))
                .ReturnsAsync((mockProducers2, mockCalcResultPartialObligationsData.Object));

            var result = await calcResultBuilder.BuildAsync(resultsRequestDto);

            Assert.IsNotNull(result);
            Assert.AreNotEqual(mockCalcResultScaledUpProducersData.Object, result.CalcResultScaledupProducers);
            Assert.AreSame(mockCalcResultProjectedProducersData.Object, result.CalcResultProjectedProducers);

            mockCalcResultScaledupProducersBuilder.Verify(m => m.ConstructAsync(resultsRequestDto, It.IsAny<List<ProducerDetail>>()), Times.Never);
            mockCalcResultProjectedProducersBuilder.Verify(m => m.ConstructAsync(resultsRequestDto, It.IsAny<List<ProducerDetail>>()), Times.Once);
            mockProjectedProducerService.Verify(m => m.StoreProjectedProducers(1, It.IsAny<List<ProducerDetail>>()), Times.Once);
        }

        [TestMethod]
        public async Task Build_ShouldReturnCalcResult_WithProjectedProducers_Billing()
        {
            var resultsRequestDto = new CalcResultsRequestDto() { RunId = 1, RelativeYear = new RelativeYear(2026), IsBillingFile = true };
            var mockProducers1 = new List<ProducerDetail>
            {
                new ProducerDetail { ProducerId = 1, SubsidiaryId = null }
            };
            var mockProducers2 = new List<ProducerDetail>
            {
                new ProducerDetail { ProducerId = 2, SubsidiaryId = null }
            };
            var mockCalcResultScaledUpProducersData = new Mock<CalcResultScaledupProducers>();
            var mockCalcResultProjectedProducersData = new Mock<CalcResultProjectedProducers>();

            mockReportedProducerService.Setup(m => m.GetProducers(resultsRequestDto.RunId))
                .ReturnsAsync(mockProducers1);

            mockCalcResultProjectedProducersBuilder.Setup(m => m.ConstructAsync(resultsRequestDto, mockProducers1))
                .ReturnsAsync((mockProducers2, mockCalcResultProjectedProducersData.Object!));

            var result = await calcResultBuilder.BuildAsync(resultsRequestDto);

            Assert.IsNotNull(result);
            Assert.AreSame(mockCalcResultProjectedProducersData.Object, result.CalcResultProjectedProducers);
            Assert.AreNotEqual(mockCalcResultScaledUpProducersData.Object, result.CalcResultScaledupProducers);

            mockCalcResultScaledupProducersBuilder.Verify(m => m.ConstructAsync(resultsRequestDto,  It.IsAny<List<ProducerDetail>>()), Times.Never);
            mockCalcResultProjectedProducersBuilder.Verify(m => m.ConstructAsync(resultsRequestDto,  It.IsAny<List<ProducerDetail>>()), Times.Once);
            mockProjectedProducerService.Verify(m => m.StoreProjectedProducers(1, It.IsAny<List<ProducerDetail>>()), Times.Never);
        }
    }
}
