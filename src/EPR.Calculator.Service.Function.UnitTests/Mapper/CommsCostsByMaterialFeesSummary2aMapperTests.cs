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
        private CommsCostsByMaterialFeesSummary2AMapper _testClass = null!;

        [TestInitialize]
        public void SetUp()
        {
            _testClass = new CommsCostsByMaterialFeesSummary2AMapper();
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
            Assert.AreEqual(result.TotalBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.CommunicationCostsSectionTwoA.BadDebtProvision));
            Assert.AreEqual(result.EnglandTotalWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.CommunicationCostsSectionTwoA.EnglandTotalWithBadDebtProvision));
            Assert.AreEqual(result.WalesTotalWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.CommunicationCostsSectionTwoA.WalesTotalWithBadDebtProvision));
            Assert.AreEqual(result.ScotlandTotalWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.CommunicationCostsSectionTwoA.ScotlandTotalWithBadDebtProvision));
            Assert.AreEqual(result.NorthernIrelandTotalWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.CommunicationCostsSectionTwoA.NorthernIrelandTotalWithBadDebtProvision));
        }
    }
}