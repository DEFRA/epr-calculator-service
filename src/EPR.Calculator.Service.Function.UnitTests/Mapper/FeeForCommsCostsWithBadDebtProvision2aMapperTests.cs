namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    using AutoFixture;
    using EPR.Calculator.API.Utils;
    using EPR.Calculator.Service.Common.Utils;
    using EPR.Calculator.Service.Function.Enums;
    using EPR.Calculator.Service.Function.Mapper;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class FeeForCommsCostsWithBadDebtProvision2aMapperTests
    {
        private FeeForCommsCostsWithBadDebtProvision2AMapper _testClass = null!;

        [TestInitialize]
        public void SetUp()
        {
            _testClass = new FeeForCommsCostsWithBadDebtProvision2AMapper();
        }

        [TestMethod]
        public void CanCallMap()
        {
            // Arrange
            var fixture = new Fixture();
            var calcResultSummaryProducerDisposalFees = fixture.Create<CalcResultSummaryProducerDisposalFees>();

            // Act
            var result = _testClass.Map(calcResultSummaryProducerDisposalFees);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.TotalProducerFeeForCommsCostsWithoutBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees?.CommunicationCostsSectionTwoA?.TotalProducerFeeWithoutBadDebtProvision ?? 0));
            Assert.AreEqual(result.BadDebtProvisionFor2a, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees?.CommunicationCostsSectionTwoA?.BadDebtProvision ?? 0));
            Assert.AreEqual(result.TotalProducerFeeForCommsCostsWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees?.CommunicationCostsSectionTwoA?.TotalProducerFeeWithBadDebtProvision ?? 0));
            Assert.AreEqual(result.EnglandTotalWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees?.CommunicationCostsSectionTwoA?.EnglandTotalWithBadDebtProvision ?? 0));
            Assert.AreEqual(result.WalesTotalWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees?.CommunicationCostsSectionTwoA?.WalesTotalWithBadDebtProvision ?? 0));
            Assert.AreEqual(result.ScotlandTotalWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees?.CommunicationCostsSectionTwoA?.ScotlandTotalWithBadDebtProvision ?? 0));
            Assert.AreEqual(result.NorthernIrelandTotalWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees?.CommunicationCostsSectionTwoA?.NorthernIrelandTotalWithBadDebtProvision ?? 0));
            Assert.AreEqual(result.PercentageOfProducerTonnageVsAllProducers, $"{Math.Round(calcResultSummaryProducerDisposalFees?.PercentageofProducerReportedTonnagevsAllProducers ?? 0, (int)DecimalPlaces.Eight).ToString()}%");
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

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.TotalProducerFeeForCommsCostsWithoutBadDebtProvision, CurrencyConverter.ConvertToCurrency(0));
            Assert.AreEqual(result.BadDebtProvisionFor2a, CurrencyConverter.ConvertToCurrency(0));
            Assert.AreEqual(result.TotalProducerFeeForCommsCostsWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(0));
            Assert.AreEqual(result.EnglandTotalWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(0));
            Assert.AreEqual(result.WalesTotalWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(0));
            Assert.AreEqual(result.ScotlandTotalWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(0));
            Assert.AreEqual(result.NorthernIrelandTotalWithBadDebtProvision, CurrencyConverter.ConvertToCurrency(0));
            Assert.AreEqual(result.PercentageOfProducerTonnageVsAllProducers, "0%");
        }
    }
}