namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
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
            Assert.AreEqual(result.TotalBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees?.CommunicationCostsSectionTwoA?.BadDebtProvision ?? 0));
            Assert.AreEqual(result.EnglandTotalWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees?.CommunicationCostsSectionTwoA?.EnglandTotalWithBadDebtProvision ?? 0));
            Assert.AreEqual(result.WalesTotalWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees?.CommunicationCostsSectionTwoA?.WalesTotalWithBadDebtProvision ?? 0));
            Assert.AreEqual(result.ScotlandTotalWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees?.CommunicationCostsSectionTwoA?.ScotlandTotalWithBadDebtProvision ?? 0));
            Assert.AreEqual(result.NorthernIrelandTotalWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees?.CommunicationCostsSectionTwoA?.NorthernIrelandTotalWithBadDebtProvision ?? 0));
        }

        [TestMethod]
        public void CanCallMap_NullValues()
        {
            // Arrange
            var calcResultSummaryProducerDisposalFees = new CalcResultSummaryProducerDisposalFees
            {
                CommunicationCostsSectionTwoA = null,
                ProducerId = null!,
                SubsidiaryId = null!,
                ProducerName = null!
            };

            // Act
            var result = _testClass.Map(calcResultSummaryProducerDisposalFees);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.TotalBadDebtProvision, CurrencyConverter.ConvertToCurrency(0));
            Assert.AreEqual(result.EnglandTotalWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(0));
            Assert.AreEqual(result.WalesTotalWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(0));
            Assert.AreEqual(result.ScotlandTotalWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(0));
            Assert.AreEqual(result.NorthernIrelandTotalWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(0));
        }
    }
}