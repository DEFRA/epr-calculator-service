using AutoFixture;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    [TestClass]
    public class CalculationOfSuggestedBillingInstructionsAndInvoiceAmountsMapperTests
    {

        private CalculationOfSuggestedBillingInstructionsAndInvoiceAmountsMapper _testClass;

        [TestInitialize]
        public void Setup()
        {
            _testClass = new CalculationOfSuggestedBillingInstructionsAndInvoiceAmountsMapper();
        }

        [TestMethod]
        public void CanCallMap()
        {
            // Arrange
            var fixture = new Fixture();
            var fees = fixture.Create<CalcResultSummaryProducerDisposalFees>();

            // Act
            var result = ((ICalculationOfSuggestedBillingInstructionsAndInvoiceAmountsMapper)_testClass).Map(fees);

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
