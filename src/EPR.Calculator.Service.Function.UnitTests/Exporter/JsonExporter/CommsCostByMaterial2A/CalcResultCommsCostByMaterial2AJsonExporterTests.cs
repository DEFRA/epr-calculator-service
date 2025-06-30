using AutoFixture;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.CommsCostByMaterial2A;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using EPR.Calculator.Service.Function.UnitTests.Builder;
using Moq;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.JsonExporter.CommsCostByMaterial2a
{
    [TestClass]
    public class CalcResultCommsCostByMaterial2AJsonExporterTests
    {
        private CalcResultCommsCostByMaterial2AJsonExporter _testClass;
        private Mock<ICalcResultCommsCostByMaterial2AJsonMapper> _testMapper;

        [TestInitialize]
        public void Setup()
        {
            _testMapper = new Mock<ICalcResultCommsCostByMaterial2AJsonMapper>();
            _testClass = new CalcResultCommsCostByMaterial2AJsonExporter(_testMapper.Object);
        }

        [TestMethod]
        public void CanCallExport()
        {
            // Arrange
            var fixture = new Fixture();
            var commsCostByMaterial = fixture.Create<Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial>>();
            var materials = TestDataHelper.GetMaterials();

            _testMapper.Setup(mock => mock.Map(
                    It.IsAny<Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial>>(),
                    It.IsAny<List<MaterialDetail>>())
                ).Returns(fixture.Create<CalcResultCommsCostByMaterial2AJson>());

            // Act
            var result = _testClass.Export(commsCostByMaterial, materials);

            // Assert
            _testMapper.Verify(mock => mock.Map(
                It.IsAny<Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial>>(),
                It.IsAny<List<MaterialDetail>>())
            );

            Assert.AreNotEqual(string.Empty, result);
        }
    }
}
