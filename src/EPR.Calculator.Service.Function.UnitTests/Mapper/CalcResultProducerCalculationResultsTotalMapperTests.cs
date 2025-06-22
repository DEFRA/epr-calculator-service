using AutoFixture;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    [TestClass]
    public class CalcResultProducerCalculationResultsTotalMapperTests
    {
        private CalcResultProducerCalculationResultsTotalMapper _mapper;

        [TestInitialize]
        public void SetUp()
        {
            _mapper = new CalcResultProducerCalculationResultsTotalMapper();
        }

        [TestMethod]
        public void Map_ReturnsNull()
        {
            // Arrange
            var fixture = new Fixture();
            var summary = fixture.Create<CalcResultSummary>();

            // Act
            var result = _mapper.Map(summary);

            // Assert
            Assert.IsNull(result);
        }
    }
}