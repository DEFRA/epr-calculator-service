namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    using System;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Mapper;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SAOperatingCostsWithBadDebtProvisionMapperTests
    {
        private SAOperatingCostsWithBadDebtProvisionMapper _testClass;

        [TestInitialize]
        public void SetUp()
        {
            _testClass = new SAOperatingCostsWithBadDebtProvisionMapper();
        }

        [TestMethod]
        public void CanCallMap()
        {
            // Arrange
            var fixture = new Fixture();
            var fees = fixture.Create<CalcResultSummaryProducerDisposalFees>();

            // Act
            var result = _testClass.Map(fees);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.TotalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithBadDebtProvision, fees.Total3SAOperatingCostswithBadDebtprovision);
            Assert.AreEqual(result.TotalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithoutBadDebtProvision, fees.Total3SAOperatingCostwoBadDebtprovision);
            Assert.AreEqual(result.BadDebProvisionFor3, fees.BadDebtProvisionFor3);
            Assert.AreEqual(result.EnglandTotalForSAOperatingCostsWithBadDebtProvision, fees.EnglandTotalWithBadDebtProvision3);
            Assert.AreEqual(result.ScotlandTotalForSAOperatingCostsWithBadDebtProvision, fees.ScotlandTotalWithBadDebtProvision3);
            Assert.AreEqual(result.WalesTotalForSAOperatingCostsWithBadDebtProvision, fees.WalesTotalWithBadDebtProvision3);
            Assert.AreEqual(result.NorthernIrelandTotalForSAOperatingCostsWithBadDebtProvision, fees.NorthernIrelandTotalWithBadDebtProvision3);
        }        
    }
}