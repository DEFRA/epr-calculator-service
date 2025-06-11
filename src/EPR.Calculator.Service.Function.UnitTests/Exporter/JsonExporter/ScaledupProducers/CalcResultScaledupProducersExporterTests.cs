using AutoFixture;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.ScaledupProducers;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using Moq;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.JsonExporter.ScaledupProducers
{
    [TestClass]
    public class CalcResultScaledupProducersExporterTests
    {
        private CalcResultScaledupProducersJsonExporter _testClass;
        private Mock<ICalcResultScaledupProducersJsonMapper> _testMapper;

        [TestInitialize]
        public void Setup()
        {
            _testMapper = new Mock<ICalcResultScaledupProducersJsonMapper>();
            _testClass = new CalcResultScaledupProducersJsonExporter(_testMapper.Object);
        }

        [TestMethod]
        public void CanCallExport()
        {
            // Arrange
            var fixture = new Fixture();
            var calcResultScaledupProducers = fixture.Create<CalcResultScaledupProducers>();
            var acceptedProducerIds = fixture.Create<IEnumerable<int>>();

            _testMapper.Setup(mock => mock.Map(It.IsAny<CalcResultScaledupProducers>(), It.IsAny<IEnumerable<int>>())).Returns(fixture.Create<CalcResultScaledupProducersJson>());

            // Act
            var result = _testClass.Export(calcResultScaledupProducers, acceptedProducerIds);

            // Assert
            _testMapper.Verify(mock => mock.Map(It.IsAny<CalcResultScaledupProducers>(), It.IsAny<IEnumerable<int>>()));

            Assert.AreNotEqual(string.Empty, result);
        }
    }
}
