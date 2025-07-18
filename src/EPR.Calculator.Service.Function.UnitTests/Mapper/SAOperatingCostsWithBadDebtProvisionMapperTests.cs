namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    using System;
    using AutoFixture;
    using EPR.Calculator.Service.Common.Utils;
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
            Assert.AreEqual(result.TotalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithBadDebtProvision, CurrencyConverter.ConvertToCurrency(fees.SchemeAdministratorOperatingCosts.TotalProducerFeeWithBadDebtProvision));
            Assert.AreEqual(result.TotalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithoutBadDebtProvision, CurrencyConverter.ConvertToCurrency(fees.SchemeAdministratorOperatingCosts.TotalProducerFeeWithoutBadDebtProvision));
            Assert.AreEqual(result.BadDebtProvisionFor3, CurrencyConverter.ConvertToCurrency(fees.SchemeAdministratorOperatingCosts.BadDebtProvision));
            Assert.AreEqual(result.EnglandTotalForSAOperatingCostsWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(fees.SchemeAdministratorOperatingCosts.EnglandTotalWithBadDebtProvision));
            Assert.AreEqual(result.ScotlandTotalForSAOperatingCostsWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(fees.SchemeAdministratorOperatingCosts.ScotlandTotalWithBadDebtProvision));
            Assert.AreEqual(result.WalesTotalForSAOperatingCostsWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(fees.SchemeAdministratorOperatingCosts.WalesTotalWithBadDebtProvision));
            Assert.AreEqual(result.NorthernIrelandTotalForSAOperatingCostsWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(fees.SchemeAdministratorOperatingCosts.NorthernIrelandTotalWithBadDebtProvision));
        }        
    }
}