using AutoFixture;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.Builder;
using Microsoft.Extensions.Logging;
using Moq;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    [TestClass]
    public class ProducerInvoiceNetTonnageServiceTests
    {

        public ProducerInvoiceNetTonnageServiceTests()
        {
            producerInvoiceMaterialChunker = new Mock<IDbLoadingChunkerService<ProducerInvoicedMaterialNetTonnage>>();
            logger = new Mock<ILogger<ProducerInvoiceNetTonnageService>>();
            materialService = new Mock<IMaterialService>();
            producerInvoiceMapper = new Mock<IProducerInvoiceTonnageMapper>();
            testClass = new ProducerInvoiceNetTonnageService(producerInvoiceMaterialChunker.Object, logger.Object, materialService.Object, producerInvoiceMapper.Object);
        }

        private ProducerInvoiceNetTonnageService testClass { get; init; }
        private Mock<IDbLoadingChunkerService<ProducerInvoicedMaterialNetTonnage>> producerInvoiceMaterialChunker { get; init; }
        private Mock<ILogger<ProducerInvoiceNetTonnageService>> logger { get; init; }
        private Mock<IMaterialService> materialService { get; init; }
        private Mock<IProducerInvoiceTonnageMapper> producerInvoiceMapper { get; init; }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new ProducerInvoiceNetTonnageService(producerInvoiceMaterialChunker.Object, logger.Object, materialService.Object, producerInvoiceMapper.Object);

            // Assert
            Assert.IsNotNull(instance);
        }


        [TestMethod]
        public async Task CanCallCreateProducerInvoiceNetTonnage1()
        {
            // Arrange
            var fixture = new Fixture();
            var calcResult = TestDataHelper.GetCalcResult();

            materialService.Setup(m => m.GetMaterials()).ReturnsAsync(TestDataHelper.GetMaterials());
            producerInvoiceMapper.Setup(m => m.Map(It.IsAny<ProducerInvoiceTonnage>())).Returns(fixture.Create<ProducerInvoicedMaterialNetTonnage>());

            // Act

            var result = await testClass.CreateProducerInvoiceNetTonnage(calcResult);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task CanCallCreateProducerInvoiceTonnageWithNoProducers()
        {
            // Arrange
            var fixture = new Fixture();
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
                CalcResultModulation = null,
            };

            materialService.Setup(mock => mock.GetMaterials()).ReturnsAsync(fixture.Create<List<MaterialDetail>>());

            // Act
            var result = await testClass.CreateProducerInvoiceNetTonnage(calcResult);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task CannotCallCreateProducerInvoiceTonnageWithNullCalcResult()
        {
            // Arrange
            var fixture = new Fixture();
            var calcResult = fixture.Create<CalcResult>();
            materialService.Setup(mock => mock.GetMaterials()).Throws<Exception>();

            // Act
            var result = await testClass.CreateProducerInvoiceNetTonnage(calcResult);

            // Assert
            Assert.IsFalse(result);
        }
    }
}