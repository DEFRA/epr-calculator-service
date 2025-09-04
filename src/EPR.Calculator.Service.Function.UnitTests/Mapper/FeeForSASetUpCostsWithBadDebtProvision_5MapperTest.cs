namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    using AutoFixture;
    using EPR.Calculator.Service.Common.Utils;
    using EPR.Calculator.Service.Function.Mapper;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class FeeForSASetUpCostsWithBadDebtProvision_5MapperTests
    {
        private FeeForSaSetUpCostsWithBadDebtProvision5Mapper? _testClass;

        [TestInitialize]
        public void SetUp()
        {
            _testClass = new FeeForSaSetUpCostsWithBadDebtProvision5Mapper();
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
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees?.OneOffSchemeAdministrationSetupCosts?.TotalProducerFeeWithoutBadDebtProvision ?? 0), result.TotalProducerOneOffFeeForSASetUpCostsWithoutBadDebtProvision);
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees?.OneOffSchemeAdministrationSetupCosts?.BadDebtProvision ?? 0), result.BadDebtProvisionFor5);
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees?.OneOffSchemeAdministrationSetupCosts?.TotalProducerFeeWithBadDebtProvision ?? 0), result.TotalProducerOneOffFeeForSASetUpCostsWithBadDebtProvision);
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees?.OneOffSchemeAdministrationSetupCosts?.EnglandTotalWithBadDebtProvision ?? 0), result.EnglandTotalForSASetUpCostsWithBadDebtProvision);
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees?.OneOffSchemeAdministrationSetupCosts?.WalesTotalWithBadDebtProvision ?? 0), result.WalesTotalForSASetUpCostsWithBadDebtProvision);
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees?.OneOffSchemeAdministrationSetupCosts?.ScotlandTotalWithBadDebtProvision ?? 0), result.ScotlandTotalForSASetUpCostsWithBadDebtProvision);
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees?.OneOffSchemeAdministrationSetupCosts?.NorthernIrelandTotalWithBadDebtProvision ?? 0), result.NorthernIrelandTotalForSASetUpCostsWithBadDebtProvision);
        }

        [TestMethod]
        public void CanCallMap_NullValues()
        {
            // Arrange
            var calcResultSummaryProducerDisposalFees = new CalcResultSummaryProducerDisposalFees
            {
                OneOffSchemeAdministrationSetupCosts = null,
                ProducerId = null!,
                SubsidiaryId = null!,
                ProducerName = null!
            };

            // Act
            var result = _testClass?.Map(calcResultSummaryProducerDisposalFees);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(0), result.TotalProducerOneOffFeeForSASetUpCostsWithoutBadDebtProvision);
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(0), result.BadDebtProvisionFor5);
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(0), result.TotalProducerOneOffFeeForSASetUpCostsWithBadDebtProvision);
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(0), result.EnglandTotalForSASetUpCostsWithBadDebtProvision);
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(0), result.WalesTotalForSASetUpCostsWithBadDebtProvision);
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(0), result.ScotlandTotalForSASetUpCostsWithBadDebtProvision);
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(0), result.NorthernIrelandTotalForSASetUpCostsWithBadDebtProvision);
        }
    }
}