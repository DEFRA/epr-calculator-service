namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    using System;
    using AutoFixture;
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
            Assert.AreEqual(result.TotalProducerFeeForLADisposalCostsWithoutBadDebtProvision, calcResultSummaryProducerDisposalFees.TotalProducerFeeforLADisposalCostswoBadDebtprovision);
            Assert.AreEqual(result.BadDebtProvisionForLADisposalCosts, calcResultSummaryProducerDisposalFees.BadDebtProvisionFor1);
            Assert.AreEqual(result.TotalProducerFeeForLADisposalCostsWithBadDebtProvision, calcResultSummaryProducerDisposalFees.TotalProducerFeeforLADisposalCostswithBadDebtprovision);
            Assert.AreEqual(result.EnglandTotalForLADisposalCostsWithBadDebtProvision, calcResultSummaryProducerDisposalFees.EnglandTotalWithBadDebtProvision);
            Assert.AreEqual(result.WalesTotalForLADisposalCostsWithBadDebtProvision, calcResultSummaryProducerDisposalFees.WalesTotalWithBadDebtProvision);
            Assert.AreEqual(result.ScotlandTotalForLADisposalCostsWithBadDebtProvision, calcResultSummaryProducerDisposalFees.ScotlandTotalWithBadDebtProvision);
            Assert.AreEqual(result.NorthernIrelandTotalForLADisposalCostsWithBadDebtProvision, calcResultSummaryProducerDisposalFees.NorthernIrelandTotalWithBadDebtProvision);
        }
    }
}