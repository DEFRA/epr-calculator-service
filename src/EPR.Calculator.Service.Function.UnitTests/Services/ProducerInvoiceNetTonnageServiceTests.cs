using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Features.Calculator.Contexts;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Utils;
using Microsoft.Extensions.Logging;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    [TestClass]
    public class ProducerInvoiceNetTonnageServiceTests
    {
        private ApplicationDBContext _dbContext = null!;
        private ProducerInvoiceNetTonnageService _testClass = null!;
        private Mock<ILogger<ProducerInvoiceNetTonnageService>> _logger = null!;
        private Mock<IMaterialService> _materialService = null!;

        [TestInitialize]
        public void Initalize()
        {
            _dbContext = TestFixtures.New().Create<ApplicationDBContext>();
            _logger = new Mock<ILogger<ProducerInvoiceNetTonnageService>>();
            _materialService = new Mock<IMaterialService>();
            _testClass = new ProducerInvoiceNetTonnageService(_dbContext, new TestBulkOps(), _materialService.Object, _logger.Object);
        }

        [TestMethod]
        public async Task CanCallCreateProducerInvoiceNetTonnage1()
        {
            // Arrange
            var runContext = TestFixtures.Legacy.Create<CalculatorRunContext>();
            var calcResult = TestDataHelper.GetCalcResult();

            _materialService.Setup(m => m.GetMaterials(It.IsAny<CancellationToken>())).ReturnsAsync(TestDataHelper.Materials);

            // Act/Assert
            await Should.NotThrowAsync(async () => await _testClass.CreateProducerInvoiceNetTonnage(runContext, calcResult));
        }

        [TestMethod]
        public async Task CanCallCreateProducerInvoiceTonnageWithNoProducers()
        {
            // Arrange
            var runContext = TestFixtures.Legacy.Create<CalculatorRunContext>();
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
                CalcResultProjectedProducers = new CalcResultProjectedProducers(),
                CalcResultModulation = null,
            };

            _materialService.Setup(mock => mock.GetMaterials(It.IsAny<CancellationToken>())).ReturnsAsync(TestFixtures.Legacy.Create<ImmutableArray<MaterialDto>>());

            // Act/Assert
            await Should.ThrowAsync<Exception>(async () => await _testClass.CreateProducerInvoiceNetTonnage(runContext, calcResult));
        }

        [TestMethod]
        public async Task CannotCallCreateProducerInvoiceTonnageWithNullCalcResult()
        {
            // Arrange
            var runContext = TestFixtures.Legacy.Create<CalculatorRunContext>();
            var calcResult = TestFixtures.Legacy.Create<CalcResult>();
            _materialService.Setup(mock => mock.GetMaterials(It.IsAny<CancellationToken>())).Throws<Exception>();

            // Act/Assert
            await Should.ThrowAsync<Exception>(async () => await _testClass.CreateProducerInvoiceNetTonnage(runContext, calcResult));
        }
    }
}