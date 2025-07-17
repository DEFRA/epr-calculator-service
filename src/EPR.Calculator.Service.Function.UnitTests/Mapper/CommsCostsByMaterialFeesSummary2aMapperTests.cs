namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using AutoFixture;
    using EPR.Calculator.Service.Common.Utils;
    using EPR.Calculator.Service.Function.Mapper;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CommsCostsByMaterialFeesSummary2aMapperTests
    {
        private CommsCostsByMaterialFeesSummary2aMapper _testClass;

        [TestInitialize]
        public void SetUp()
        {
            _testClass = new CommsCostsByMaterialFeesSummary2aMapper();
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
            Assert.AreEqual(result.WalesTotalWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.WalesTotalWithBadDebtProvision2A));
            Assert.AreEqual(result.EnglandTotalWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.EnglandTotalWithBadDebtProvision2A));
            Assert.AreEqual(result.TotalBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.BadDebtProvisionFor2A));
            Assert.AreEqual(result.NorthernIrelandTotalWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.NorthernIrelandTotalWithBadDebtProvision2A));
            Assert.AreEqual(result.ScotlandTotalWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.ScotlandTotalWithBadDebtProvision2A));

        }
    }
}