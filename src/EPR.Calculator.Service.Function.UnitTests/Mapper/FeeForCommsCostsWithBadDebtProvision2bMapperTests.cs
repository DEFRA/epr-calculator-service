using System;
using AutoFixture;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EPR.Calculator.Service.Common.Utils;

namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    [TestClass]
    public class FeeForCommsCostsWithBadDebtProvision2bMapperTests
    {
        private FeeForCommsCostsWithBadDebtProvision2bMapper? _testClass;

        [TestInitialize]
        public void SetUp()
        {
            _testClass = new FeeForCommsCostsWithBadDebtProvision2bMapper();
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
            Assert.AreEqual(result.TotalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.CommunicationCostsSectionTwoB.TotalProducerFeeWithoutBadDebtProvision));
            Assert.AreEqual(result.BadDebtProvisionFor2b, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.CommunicationCostsSectionTwoB.BadDebtProvision));
            Assert.AreEqual(result.TotalProducerFeeForCommsCostsUKWideWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.CommunicationCostsSectionTwoB.TotalProducerFeeWithBadDebtProvision));
            Assert.AreEqual(result.EnglandTotalWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.CommunicationCostsSectionTwoB.EnglandTotalWithBadDebtProvision));
            Assert.AreEqual(result.WalesTotalWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.CommunicationCostsSectionTwoB.WalesTotalWithBadDebtProvision));
            Assert.AreEqual(result.ScotlandTotalWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.CommunicationCostsSectionTwoB.ScotlandTotalWithBadDebtProvision));
            Assert.AreEqual(result.NorthernIrelandTotalWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.CommunicationCostsSectionTwoB.NorthernIrelandTotalWithBadDebtProvision));
        }
    }
}