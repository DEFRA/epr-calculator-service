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
        private SAOperatingCostsWithBadDebtProvisionMapper _testClass = null!;

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
            Assert.AreEqual(result.TotalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithBadDebtProvision, CurrencyConverter.ConvertToCurrency(fees.SchemeAdministratorOperatingCosts?.TotalProducerFeeWithBadDebtProvision ?? 0));
            Assert.AreEqual(result.TotalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithoutBadDebtProvision, CurrencyConverter.ConvertToCurrency(fees.SchemeAdministratorOperatingCosts?.TotalProducerFeeWithoutBadDebtProvision ?? 0));
            Assert.AreEqual(result.BadDebtProvisionFor3, CurrencyConverter.ConvertToCurrency(fees.SchemeAdministratorOperatingCosts?.BadDebtProvision ?? 0));
            Assert.AreEqual(result.EnglandTotalForSAOperatingCostsWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(fees.SchemeAdministratorOperatingCosts?.EnglandTotalWithBadDebtProvision ?? 0));
            Assert.AreEqual(result.ScotlandTotalForSAOperatingCostsWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(fees.SchemeAdministratorOperatingCosts?.ScotlandTotalWithBadDebtProvision ?? 0));
            Assert.AreEqual(result.WalesTotalForSAOperatingCostsWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(fees.SchemeAdministratorOperatingCosts?.WalesTotalWithBadDebtProvision ?? 0));
            Assert.AreEqual(result.NorthernIrelandTotalForSAOperatingCostsWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(fees.SchemeAdministratorOperatingCosts?.NorthernIrelandTotalWithBadDebtProvision ?? 0));
        }        
    }
}