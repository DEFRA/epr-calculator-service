namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    using AutoFixture;
    using EPR.Calculator.Service.Common.Utils;
    using EPR.Calculator.Service.Function.Mapper;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class FeeForSASetUpCostsWithBadDebtProvision_5MapperTests
    {
        private FeeForSASetUpCostsWithBadDebtProvision_5Mapper? _testClass;

        [TestInitialize]
        public void SetUp()
        {
            _testClass = new FeeForSASetUpCostsWithBadDebtProvision_5Mapper();
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
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TotalProducerFeeWithoutBadDebtProvisionSection5), result.TotalProducerOneOffFeeForSASetUpCostsWithoutBadDebtProvision);
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.BadDebtProvisionSection5), result.BadDebtProvisionFor5);
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TotalProducerFeeWithBadDebtProvisionSection5), result.TotalProducerOneOffFeeForSASetUpCostsWithBadDebtProvision);
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.EnglandTotalWithBadDebtProvisionSection5), result.EnglandTotalForSASetUpCostsWithBadDebtProvision);
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.WalesTotalWithBadDebtProvisionSection5), result.WalesTotalForSASetUpCostsWithBadDebtProvision);
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.ScotlandTotalWithBadDebtProvisionSection5), result.ScotlandTotalForSASetUpCostsWithBadDebtProvision);
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.NorthernIrelandTotalWithBadDebtProvisionSection5), result.NorthernIrelandTotalForSASetUpCostsWithBadDebtProvision);
        }
    }
}