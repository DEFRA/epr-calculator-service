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
        private ParametersOtherJsonExporter TestClass { get; init; }
        private Mock<IParametersOtherMapper> TestMapper{ get; init; }

        public ParametersOtherJsonExporterTests()
        {
            this.TestMapper = new Mock<IParametersOtherMapper>();
            this.TestClass = new ParametersOtherJsonExporter(this.TestMapper.Object);
        }

        [TestMethod]
        public void CanCallExport()
        {
            // Arrange
            var fixture = new Fixture();
            var otherCost = fixture.Create<CalcResultParameterOtherCost>();

            TestMapper
                .Setup(mock => mock.Map(It.IsAny<CalcResultParameterOtherCost>()))
                .Returns(fixture.Create<CalcResultParametersOtherJson>());

            // Act
            var result = TestClass.Export(otherCost);

            // Assert
            TestMapper.Verify(mock => mock.Map(It.IsAny<CalcResultParameterOtherCost>()));
            Assert.IsFalse(string.IsNullOrEmpty(result));
        }
    }
}