namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    using System;
    using AutoFixture;
    using EPR.Calculator.Service.Common.Utils;
    using EPR.Calculator.Service.Function.Mapper;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalculationResultsProducerCalculationResultsFeeForLADisposalCosts1MapperTests
    {
        private CalculationResultsProducerCalculationResultsFeeForLADisposalCosts1Mapper? _testClass;

        [TestInitialize]
        public void SetUp()
        {
            _testClass = new CalculationResultsProducerCalculationResultsFeeForLADisposalCosts1Mapper();
        }

        [TestMethod]
        public void CanCallMap()
        {
            // Arrange
            var fixture = new Fixture();
            var calcResultSummaryProducerDisposalFees = fixture.Create<CalcResultSummaryProducerDisposalFees>();

            // Act
            var result = _testClass?.Map(calcResultSummaryProducerDisposalFees);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.TotalProducerFeeForLADisposalCostsWithoutBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TotalProducerFeeforLADisposalCostswoBadDebtprovision));
            Assert.AreEqual(result.BadDebtProvisionForLADisposalCosts, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.BadDebtProvisionFor1));
            Assert.AreEqual(result.TotalProducerFeeForLADisposalCostsWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TotalProducerFeeforLADisposalCostswithBadDebtprovision));
            Assert.AreEqual(result.EnglandTotalForLADisposalCostsWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.EnglandTotalWithBadDebtProvision));
            Assert.AreEqual(result.WalesTotalForLADisposalCostsWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.WalesTotalWithBadDebtProvision));
            Assert.AreEqual(result.ScotlandTotalForLADisposalCostsWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.ScotlandTotalWithBadDebtProvision));
            Assert.AreEqual(result.NorthernIrelandTotalForLADisposalCostsWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.NorthernIrelandTotalWithBadDebtProvision));
        }
    }
}