namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    using AutoFixture;
    using EPR.Calculator.API.Utils;
    using EPR.Calculator.Service.Common.Utils;
    using EPR.Calculator.Service.Function.Enums;
    using EPR.Calculator.Service.Function.Mapper;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class FeeForCommsCostsWithBadDebtProvision2aMapperTests
    {
        private FeeForCommsCostsWithBadDebtProvision2aMapper _testClass;

        [TestInitialize]
        public void SetUp()
        {
            _testClass = new FeeForCommsCostsWithBadDebtProvision2aMapper();
        }

        [TestMethod]
        public void CanCallMap()
        {
            // Arrange
            var fixture = new Fixture();
            var calcResultSummaryProducerDisposalFees = fixture.Create<CalcResultSummaryProducerDisposalFees>();

            // Act
            var result = _testClass.Map(calcResultSummaryProducerDisposalFees);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.TotalProducerFeeForCommsCostsWithoutBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision));
            Assert.AreEqual(result.BadDebProvisionFor2a, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.BadDebtProvisionFor2A));
            Assert.AreEqual(result.TotalProducerFeeForCommsCostsWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision));
            Assert.AreEqual(result.PercentageOfProducerTonnageVsAllProducers, $"{Math.Round(calcResultSummaryProducerDisposalFees.PercentageofProducerReportedTonnagevsAllProducers, (int)DecimalPlaces.Eight).ToString()}%");
            Assert.AreEqual(result.EnglandTotalWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.EnglandTotalWithBadDebtProvision2A));
            Assert.AreEqual(result.WalesTotalWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.WalesTotalWithBadDebtProvision2A));
            Assert.AreEqual(result.NorthernIrelandTotalWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.NorthernIrelandTotalWithBadDebtProvision2A));
            Assert.AreEqual(result.ScotlandTotalWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.ScotlandTotalWithBadDebtProvision2A));
        }
    }
}