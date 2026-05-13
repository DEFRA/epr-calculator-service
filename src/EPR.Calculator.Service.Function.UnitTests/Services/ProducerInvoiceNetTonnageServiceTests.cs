using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Mappers;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.Telemetry;
using EPR.Calculator.Service.Function.UnitTests.Builder;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    [TestClass]
    public class ProducerInvoiceNetTonnageServiceTests
    {
        private IFixture _fixture = null!;
        private ProducerInvoiceNetTonnageService _sut = null!;
        private Mock<ICalculatorTelemetryLogger> _telemetryLogger = null!;
        private Mock<IMaterialService> _materialService = null!;
        private Mock<IProducerInvoiceTonnageMapper> _producerInvoiceMapper = null!;

        [TestInitialize]
        public void Init()
        {
            _fixture = TestFixtures.New();
            _telemetryLogger = _fixture.Freeze<Mock<ICalculatorTelemetryLogger>>();
            _materialService = _fixture.Freeze<Mock<IMaterialService>>();
            _producerInvoiceMapper = _fixture.Freeze<Mock<IProducerInvoiceTonnageMapper>>();

            _sut = _fixture.Create<ProducerInvoiceNetTonnageService>();
        }

        [TestMethod]
        public async Task CanCallCreateProducerInvoiceNetTonnage1()
        {
            // Arrange
            var calcResult = TestDataHelper.GetCalcResult();

            _telemetryLogger.Setup(mock => mock.LogInformation(It.IsAny<TrackMessage>())).Verifiable();
            _telemetryLogger.Setup(mock => mock.LogError(It.IsAny<ErrorMessage>())).Verifiable();

            _materialService.Setup(m => m.GetMaterials()).ReturnsAsync(TestDataHelper.GetMaterials());
            _producerInvoiceMapper.Setup(m => m.Map(It.IsAny<ProducerInvoiceTonnage>())).Returns(_fixture.Create<ProducerInvoicedMaterialNetTonnage>());

            // Act

            var result = await _sut.CreateProducerInvoiceNetTonnage(calcResult);

            // Assert
            _telemetryLogger.Verify(mock => mock.LogInformation(It.IsAny<TrackMessage>()));

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task CanCallCreateProducerInvoiceTonnageWithNoProducers()
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
                    Name = string.Empty,
                    CalcResultLapcapDataDetails = new List<CalcResultLapcapDataDetail>(),
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
                CalcResultProjectedProducers = new CalcResultProjectedProducers(),
            };

            _materialService.Setup(mock => mock.GetMaterials()).ReturnsAsync(_fixture.Create<List<MaterialDetail>>());
            _telemetryLogger.Setup(mock => mock.LogInformation(It.IsAny<TrackMessage>())).Verifiable();
            _telemetryLogger.Setup(mock => mock.LogError(It.IsAny<ErrorMessage>())).Verifiable();

            // Act
            var result = await _sut.CreateProducerInvoiceNetTonnage(calcResult);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task CannotCallCreateProducerInvoiceTonnageWithNullCalcResult()
        {
            // Arrange
            var calcResult = _fixture.Create<CalcResult>();

            _telemetryLogger.Setup(mock => mock.LogInformation(It.IsAny<TrackMessage>())).Verifiable();
            _materialService.Setup(mock => mock.GetMaterials()).Throws<Exception>();

            // Act
            var result = await _sut.CreateProducerInvoiceNetTonnage(calcResult);

            // Assert
            _telemetryLogger.Verify(mock => mock.LogError(It.IsAny<ErrorMessage>()));

            Assert.IsFalse(result);
        }
    }
}
