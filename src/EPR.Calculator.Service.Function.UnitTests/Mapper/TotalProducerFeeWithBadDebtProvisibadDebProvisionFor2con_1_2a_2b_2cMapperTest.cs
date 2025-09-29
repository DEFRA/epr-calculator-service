namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    using AutoFixture;
    using EPR.Calculator.Service.Common.Utils;
    using EPR.Calculator.Service.Function.Mapper;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using static EPR.Calculator.Service.Common.UnitTests.Utils.JsonNodeComparer;

    [TestClass]
    public class TotalProducerFeeWithBadDebtProvisionFor2Con1And2AAnd2BAnd2CMapperTests
    {
        private TotalProducerFeeWithBadDebtProvisionFor2Con1And2AAnd2BAnd2CMapper? _testClass;

        [TestInitialize]
        public void SetUp()
        {
            _testClass = new TotalProducerFeeWithBadDebtProvisionFor2Con1And2AAnd2BAnd2CMapper();
        }

        [TestMethod]
        public void CanCallMap()
        {
            // Arrange
            var fixture = new Fixture();
            var calcResultSummaryProducerDisposalFees = fixture.Create<CalcResultSummaryProducerDisposalFees>();

            // Act
            var result = _testClass?.Map(calcResultSummaryProducerDisposalFees);

            // Assert
            Assert.IsNotNull(result);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.ProducerTotalOnePlus2A2B2CWithBadDeptProvision), result.TotalFeeWithBadDebtProvision);
            AssertAreEqual($"{calcResultSummaryProducerDisposalFees.ProducerOverallPercentageOfCostsForOnePlus2A2B2C.ToString("F8")}%", result.ProducerPercentageOfOverallProducerCost);
        }
    }
}