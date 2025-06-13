using AutoFixture;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    [TestClass]
    public class CalcResultCommsCostByMaterial2aJsonMapperTests
    {
        private CalcResultCommsCostByMaterial2aJsonMapper _testClass;

        [TestInitialize]
        public void Setup()
        {
            _testClass = new CalcResultCommsCostByMaterial2aJsonMapper();
        }

        [TestMethod]
        public void CanCallMap()
        {
            // Arrange
            var fixture = new Fixture();
            var calcResultSummaryProducerDisposalFees = fixture.Create<CalcResultSummaryProducerDisposalFees>();

            // Act
            var result = ((ICalcResultCommsCostByMaterial2aJsonMapper)_testClass).Map(calcResultSummaryProducerDisposalFees);

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
