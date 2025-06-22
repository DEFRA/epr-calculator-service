namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    using System;
    using AutoFixture;
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
            Assert.AreEqual(calcResultSummaryProducerDisposalFees.TwoCEnglandTotalWithBadDebt, result.EnglandTotalWithBadDebtProvision);
            Assert.AreEqual(calcResultSummaryProducerDisposalFees.TwoCWalesTotalWithBadDebt, result.WalesTotalWithBadDebtProvision);
            Assert.AreEqual(calcResultSummaryProducerDisposalFees.TwoCScotlandTotalWithBadDebt, result.ScotlandTotalWithBadDebtProvision);
            Assert.AreEqual(calcResultSummaryProducerDisposalFees.TwoCNorthernIrelandTotalWithBadDebt, result.NorthernIrelandTotalWithBadDebtProvision);
            Assert.AreEqual(calcResultSummaryProducerDisposalFees.TwoCTotalProducerFeeForCommsCostsWithBadDebt, result.TotalProducerFeeForCommsCostsByCountryWithBadDebtProvision);
            Assert.AreEqual(calcResultSummaryProducerDisposalFees.TwoCBadDebtProvision, result.BadDebProvisionFor2c);
            Assert.AreEqual(calcResultSummaryProducerDisposalFees.TwoCTotalProducerFeeForCommsCostsWithoutBadDebt, result.TotalProducerFeeForCommsCostsByCountryWithoutBadDebtProvision);
        }
    }
}