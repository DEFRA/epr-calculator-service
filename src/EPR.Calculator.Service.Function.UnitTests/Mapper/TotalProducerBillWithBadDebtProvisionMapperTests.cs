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
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TotalProducerBillBreakdownCosts.TotalProducerFeeWithoutBadDebtProvision), result.TotalProducerBillWithoutBadDebtProvision);
            AssertAreEqual(calcResultSummaryProducerDisposalFees.TotalProducerBillBreakdownCosts.BadDebtProvision, result.BadDebtProvisionForTotalProducerBill);
            AssertAreEqual(calcResultSummaryProducerDisposalFees.TotalProducerBillBreakdownCosts.EnglandTotalWithBadDebtProvision, result.EnglandTotalForProducerBillWithBadDebtProvision);
            AssertAreEqual(calcResultSummaryProducerDisposalFees.TotalProducerBillBreakdownCosts.WalesTotalWithBadDebtProvision, result.WalesTotalForProducerBillWithBadDebtProvision);
            AssertAreEqual(calcResultSummaryProducerDisposalFees.TotalProducerBillBreakdownCosts.ScotlandTotalWithBadDebtProvision, result.ScotlandTotalForProducerBillWithBadDebtProvision);
            AssertAreEqual(calcResultSummaryProducerDisposalFees.TotalProducerBillBreakdownCosts.NorthernIrelandTotalWithBadDebtProvision, result.NorthernIrelandTotalForProducerBillWithBadDebtProvision);
            AssertAreEqual(calcResultSummaryProducerDisposalFees.TotalProducerBillBreakdownCosts.TotalProducerFeeWithBadDebtProvision, result.TotalProducerBillWithBadDebtProvisionAmount);
        }
    }
}
