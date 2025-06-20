using AutoFixture;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    [TestClass]
    public class CalcResultBillingInstructionsJsonMapperTests
    {

        private CalcResultBillingInstructionsJsonMapper _testClass;

        [TestInitialize]
        public void Setup()
        {
            _testClass = new CalcResultBillingInstructionsJsonMapper();
        }

        [TestMethod]
        public void CanCallMap()
        {
            // Arrange
            var fixture = new Fixture();
            var fees = fixture.Create<CalcResultSummaryProducerDisposalFees>();

            // Act
            var result = ((IBillingInstructionsJsonMapper)_testClass).Map(fees);

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
