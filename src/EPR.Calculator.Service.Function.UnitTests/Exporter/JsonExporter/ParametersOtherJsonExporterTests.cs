using AutoFixture;
using EPR.Calculator.Service.Function.Exporter.JsonExporter;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using Moq;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.JsonExporter
{
    [TestClass]
    public class ParametersOtherJsonExporterTests
    {
        private ParametersOtherJsonExporter _testClass;
        private Mock<IParametersOtherMapper> _testMapper;

        [TestInitialize]
        public void Setup()
        {
            _testMapper = new Mock<IParametersOtherMapper>();
            _testClass = new ParametersOtherJsonExporter(_testMapper.Object);
        }

        [TestMethod]
        public void CanCallExport()
        {
            // Arrange
            var fixture = new Fixture();
            var otherCost = fixture.Create<CalcResultParameterOtherCost>();

            _testMapper
                .Setup(mock => mock.Map(It.IsAny<CalcResultParameterOtherCost>()))
                .Returns(fixture.Create<CalcResultParametersOtherJson>());

            // Act
            var result = _testClass.Export(otherCost);

            // Assert
            _testMapper.Verify(mock => mock.Map(It.IsAny<CalcResultParameterOtherCost>()));
            Assert.IsFalse(string.IsNullOrEmpty(result));
        }
    }
}