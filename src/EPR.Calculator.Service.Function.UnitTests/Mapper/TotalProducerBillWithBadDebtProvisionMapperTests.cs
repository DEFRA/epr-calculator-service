using AutoFixture;
using EPR.Calculator.Service.Common.Utils;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using static EPR.Calculator.Service.Common.UnitTests.Utils.JsonNodeComparer;

namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    [TestClass]
    public class TotalProducerBillWithBadDebtProvisionMapperTests
    {
        private TotalProducerBillWithBadDebtProvisionMapper Mapper;
        public TotalProducerBillWithBadDebtProvisionMapperTests()
        {
            this.Mapper = new TotalProducerBillWithBadDebtProvisionMapper();
        }

        [TestMethod]
        public void Map_ShouldMapCalcResultSummaryProducerDisposalFeesToTotalProducerBillWithBadDebtProvision()
        {
            // Arrange
            var fixture = new Fixture();
            var calcResultSummaryProducerDisposalFees = fixture.Create<CalcResultSummaryProducerDisposalFees>();

            // Act
            var result = Mapper.Map(calcResultSummaryProducerDisposalFees);

            // Assert  
            Assert.IsNotNull(result);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees?.TotalProducerBillBreakdownCosts?.TotalProducerFeeWithoutBadDebtProvision ?? 0), result.TotalProducerBillWithoutBadDebtProvision);
            AssertAreEqual(calcResultSummaryProducerDisposalFees?.TotalProducerBillBreakdownCosts?.BadDebtProvision ?? 0, result.BadDebtProvisionForTotalProducerBill);
            AssertAreEqual(calcResultSummaryProducerDisposalFees?.TotalProducerBillBreakdownCosts?.EnglandTotalWithBadDebtProvision ?? 0, result.EnglandTotalForProducerBillWithBadDebtProvision);
            AssertAreEqual(calcResultSummaryProducerDisposalFees?.TotalProducerBillBreakdownCosts?.WalesTotalWithBadDebtProvision ?? 0, result.WalesTotalForProducerBillWithBadDebtProvision);
            AssertAreEqual(calcResultSummaryProducerDisposalFees?.TotalProducerBillBreakdownCosts?.ScotlandTotalWithBadDebtProvision ?? 0, result.ScotlandTotalForProducerBillWithBadDebtProvision);
            AssertAreEqual(calcResultSummaryProducerDisposalFees?.TotalProducerBillBreakdownCosts?.NorthernIrelandTotalWithBadDebtProvision ?? 0, result.NorthernIrelandTotalForProducerBillWithBadDebtProvision);
            AssertAreEqual(calcResultSummaryProducerDisposalFees?.TotalProducerBillBreakdownCosts?.TotalProducerFeeWithBadDebtProvision ?? 0, result.TotalProducerBillWithBadDebtProvisionAmount);
        }
    }
}
