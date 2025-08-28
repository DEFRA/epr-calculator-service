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
        private FeeForCommsCostsWithBadDebtProvision2BMapper? _testClass;

        [TestInitialize]
        public void SetUp()
        {
            _testClass = new FeeForCommsCostsWithBadDebtProvision2BMapper();
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
            Assert.AreEqual(result.TotalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees?.CommunicationCostsSectionTwoB?.TotalProducerFeeWithoutBadDebtProvision ?? 0));
            Assert.AreEqual(result.BadDebtProvisionFor2b, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees?.CommunicationCostsSectionTwoB?.BadDebtProvision ?? 0));
            Assert.AreEqual(result.TotalProducerFeeForCommsCostsUKWideWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees?.CommunicationCostsSectionTwoB?.TotalProducerFeeWithBadDebtProvision ?? 0));
            Assert.AreEqual(result.EnglandTotalWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees?.CommunicationCostsSectionTwoB?.EnglandTotalWithBadDebtProvision ?? 0));
            Assert.AreEqual(result.WalesTotalWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees?.CommunicationCostsSectionTwoB?.WalesTotalWithBadDebtProvision ?? 0));
            Assert.AreEqual(result.ScotlandTotalWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees?.CommunicationCostsSectionTwoB?.ScotlandTotalWithBadDebtProvision ?? 0));
            Assert.AreEqual(result.NorthernIrelandTotalWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees?.CommunicationCostsSectionTwoB?.NorthernIrelandTotalWithBadDebtProvision ?? 0));
        }
    }
}