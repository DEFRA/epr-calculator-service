namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using AutoFixture;
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
            Assert.AreEqual(result.WalesTotalWithBadDebtProvision, calcResultSummaryProducerDisposalFees.WalesTotalWithBadDebtProvision2A);
            Assert.AreEqual(result.EnglandTotalWithBadDebtProvision, calcResultSummaryProducerDisposalFees.EnglandTotalWithBadDebtProvision2A);
            Assert.AreEqual(result.TotalBadDebtProvision, calcResultSummaryProducerDisposalFees.BadDebtProvisionFor2A);
            Assert.AreEqual(result.NorthernIrelandTotalWithBadDebtProvision, calcResultSummaryProducerDisposalFees.NorthernIrelandTotalWithBadDebtProvision2A);
            Assert.AreEqual(result.ScotlandTotalWithBadDebtProvision, calcResultSummaryProducerDisposalFees.ScotlandTotalWithBadDebtProvision2A);

        }
    }
}