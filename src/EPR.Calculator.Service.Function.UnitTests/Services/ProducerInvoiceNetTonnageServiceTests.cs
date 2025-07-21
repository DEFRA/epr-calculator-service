namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.Service.Common.Logging;
    using EPR.Calculator.Service.Common.UnitTests.Utils;
    using EPR.Calculator.Service.Function.Interface;
    using EPR.Calculator.Service.Function.Mapper;
    using EPR.Calculator.Service.Function.Models;
    using EPR.Calculator.Service.Function.Services;
    using EPR.Calculator.Service.Function.UnitTests.Builder;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ProducerInvoiceNetTonnageServiceTests
    {
     
        public ProducerInvoiceNetTonnageServiceTests()
        {
            producerInvoiceMaterialChunker = new Mock<IDbLoadingChunkerService<ProducerInvoicedMaterialNetTonnage>>();
            telemetryLogger = new Mock<ICalculatorTelemetryLogger>();
            materialService = new Mock<IMaterialService>();
            producerInvoiceMapper = new Mock<IProducerInvoiceTonnageMapper>();
            testClass = new ProducerInvoiceNetTonnageService(producerInvoiceMaterialChunker.Object, telemetryLogger.Object, materialService.Object, producerInvoiceMapper.Object);
        }

        private ProducerInvoiceNetTonnageService testClass { get; init; }
        private Mock<IDbLoadingChunkerService<ProducerInvoicedMaterialNetTonnage>> producerInvoiceMaterialChunker { get; init; }
        private Mock<ICalculatorTelemetryLogger> telemetryLogger { get; init; }
        private Mock<IMaterialService> materialService { get; init; }
        private Mock<IProducerInvoiceTonnageMapper> producerInvoiceMapper { get; init; }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new ProducerInvoiceNetTonnageService(producerInvoiceMaterialChunker.Object, telemetryLogger.Object, materialService.Object, producerInvoiceMapper.Object);

            // Assert
            Assert.IsNotNull(instance);
        }


        [TestMethod]
        public async Task CanCallCreateProducerInvoiceNetTonnage1()
        {
            // Arrange
            var fixture = new Fixture();
            var calcResult = TestDataHelper.GetCalcResult();

            telemetryLogger.Setup(mock => mock.LogInformation(It.IsAny<TrackMessage>())).Verifiable();
            telemetryLogger.Setup(mock => mock.LogError(It.IsAny<ErrorMessage>())).Verifiable();

            materialService.Setup(m => m.GetMaterials()).ReturnsAsync(TestDataHelper.GetMaterials());
            producerInvoiceMapper.Setup(m => m.Map(It.IsAny<ProducerInvoiceTonnage>())).Returns(fixture.Create<ProducerInvoicedMaterialNetTonnage>());

            // Act
            
            var result = await testClass.CreateProducerInvoiceNetTonnage(calcResult);

            // Assert
            telemetryLogger.Verify(mock => mock.LogInformation(It.IsAny<TrackMessage>()));

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
                }
            };

            materialService.Setup(mock => mock.GetMaterials()).ReturnsAsync(fixture.Create<List<MaterialDetail>>());
            telemetryLogger.Setup(mock => mock.LogInformation(It.IsAny<TrackMessage>())).Verifiable();
            telemetryLogger.Setup(mock => mock.LogError(It.IsAny<ErrorMessage>())).Verifiable();

            // Act
            var result = await testClass.CreateProducerInvoiceNetTonnage(calcResult);

            Assert.IsFalse(result);
        }

    }
}