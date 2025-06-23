using AutoFixture;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    [TestClass]
    public class CalcResultProducerCalculationResultsTotalMapperTests
    {
        private CalcResultProducerCalculationResultsTotalMapper Mapper;

        public CalcResultProducerCalculationResultsTotalMapperTests()
        {
            this.Mapper = new CalcResultProducerCalculationResultsTotalMapper();
        }

        [TestMethod]
        public void Map_ReturnsNull()
        {
            // Arrange
            var fixture = new Fixture();
            var summary = fixture.Create<CalcResultSummary>();

            // Act
            var result = this.Mapper.Map(summary);

            // Assert
            Assert.IsNull(result);
        }
    }
}