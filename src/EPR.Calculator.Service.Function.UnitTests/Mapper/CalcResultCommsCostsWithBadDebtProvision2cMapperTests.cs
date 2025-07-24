namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    using AutoFixture;
    using EPR.Calculator.Service.Common.Utils;
    using EPR.Calculator.Service.Function.Mapper;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalcResultCommsCostsWithBadDebtProvision2cMapperTests
    {
        private CalcResultCommsCostsWithBadDebtProvision2cMapper _testClass;

        [TestInitialize]
        public void SetUp()
        {
            _testClass = new CalcResultCommsCostsWithBadDebtProvision2cMapper();
        }

        [TestMethod]
        public void CanCallMap()
        {
            // Arrange
            var fixture = new Fixture();
            var calcResultSummaryProducerDisposalFees = fixture.Create<CalcResultSummaryProducerDisposalFees>();

            // Act
            var result = _testClass.Map(calcResultSummaryProducerDisposalFees);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void MapPerformsMapping()
        {
            // Arrange
            var fixture = new Fixture();
            var calcResultSummaryProducerDisposalFees = fixture.Create<CalcResultSummaryProducerDisposalFees>();

            // Act
            var result = _testClass.Map(calcResultSummaryProducerDisposalFees);

            // Assert
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TwoCEnglandTotalWithBadDebt), result.EnglandTotalWithBadDebtProvision);
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TwoCWalesTotalWithBadDebt), result.WalesTotalWithBadDebtProvision);
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TwoCScotlandTotalWithBadDebt), result.ScotlandTotalWithBadDebtProvision);
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TwoCNorthernIrelandTotalWithBadDebt), result.NorthernIrelandTotalWithBadDebtProvision);
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TwoCTotalProducerFeeForCommsCostsWithBadDebt), result.TotalProducerFeeForCommsCostsByCountryWithBadDebtProvision);
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TwoCBadDebtProvision), result.BadDebtProvisionFor2c);
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TwoCTotalProducerFeeForCommsCostsWithoutBadDebt), result.TotalProducerFeeForCommsCostsByCountryWithoutBadDebtProvision);
        }
    }
}