using AutoFixture;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.CommsCostByMaterial2a;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using Moq;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.JsonExporter.CommsCostByMaterial2a
{
    [TestClass]
    public class CalcResultCommsCostByMaterial2aJsonExporterTests
    {
        private CalcResultCommsCostByMaterial2aJsonExporter _testClass;
        private Mock<ICalcResultCommsCostByMaterial2aJsonMapper> _testMapper;

        [TestInitialize]
        public void Setup()
        {
            _testMapper = new Mock<ICalcResultCommsCostByMaterial2aJsonMapper>();
            _testClass = new CalcResultCommsCostByMaterial2aJsonExporter(_testMapper.Object);
        }

        [TestMethod]
        public void CanCallExport()
        {
            // Arrange
            var fixture = new Fixture();
            var commsCostByMaterial = fixture.Create<Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial>>();

            _testMapper.Setup(mock => mock.Map(It.IsAny<Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial>>())).Returns(fixture.Create<CalcResultCommsCostByMaterial2aJson>());

            // Act
            var result = _testClass.Export(commsCostByMaterial);

            // Assert
            _testMapper.Verify(mock => mock.Map(It.IsAny<Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial>>()));

            Assert.AreNotEqual(string.Empty, result);
        }
    }
}
