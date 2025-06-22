using System;
using AutoFixture;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    [TestClass]
    public class FeeForCommsCostsWithBadDebtProvision2bMapperTests
    {
        private FeeForCommsCostsWithBadDebtProvision2bMapper _testClass;

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
            var result = _testClass.Map(calcResultSummaryProducerDisposalFees);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.TotalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision, calcResultSummaryProducerDisposalFees.TotalProducerFeeWithoutBadDebtFor2bComms);
            Assert.AreEqual(result.BadDebtProvisionFor2bComms, calcResultSummaryProducerDisposalFees.BadDebtProvisionFor2bComms);
            Assert.AreEqual(result.TotalProducerFeeForCommsCostsUKWideWithBadDebtProvision, calcResultSummaryProducerDisposalFees.TotalProducerFeeWithBadDebtFor2bComms);
            Assert.AreEqual(result.EnglandTotalWithBadDebtProvision, calcResultSummaryProducerDisposalFees.EnglandTotalWithBadDebtFor2bComms);
            Assert.AreEqual(result.WalesTotalWithBadDebtProvision, calcResultSummaryProducerDisposalFees.WalesTotalWithBadDebtFor2bComms);
            Assert.AreEqual(result.ScotlandTotalWithBadDebtProvision, calcResultSummaryProducerDisposalFees.ScotlandTotalWithBadDebtFor2bComms);
            Assert.AreEqual(result.NorthernIrelandTotalWithBadDebtProvision, calcResultSummaryProducerDisposalFees.NorthernIrelandTotalWithBadDebtFor2bComms);
        }
    }
}