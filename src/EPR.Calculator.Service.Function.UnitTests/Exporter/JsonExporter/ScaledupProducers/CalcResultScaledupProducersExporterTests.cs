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
            var materials = fixture.Create<List<MaterialDetail>>();

            _testMapper.Setup(mock => mock.Map(It.IsAny<CalcResultScaledupProducers>(),
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<List<MaterialDetail>>())).Returns(fixture.Create<CalcResultScaledupProducersJson>());

            // Act
            var result = _testClass.Export(calcResultScaledupProducers, acceptedProducerIds, materials);

            // Assert
            _testMapper.Verify(mock => mock.Map(It.IsAny<CalcResultScaledupProducers>(), It.IsAny<IEnumerable<int>>(), It.IsAny<List<MaterialDetail>>()));

            Assert.AreNotEqual(null, result);
        }
    }
}
