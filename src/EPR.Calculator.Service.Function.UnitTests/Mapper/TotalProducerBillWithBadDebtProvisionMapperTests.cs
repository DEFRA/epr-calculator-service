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
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TotalProducerBillBreakdownSection.TotalProducerFeeWithoutBadDebtProvision), result.TotalProducerBillWithoutBadDebtProvision);
            AssertAreEqual(calcResultSummaryProducerDisposalFees.TotalProducerBillBreakdownSection.BadDebtProvision, result.BadDebtProvisionForTotalProducerBill);
            AssertAreEqual(calcResultSummaryProducerDisposalFees.TotalProducerBillBreakdownSection.EnglandTotalWithBadDebtProvision, result.EnglandTotalForProducerBillWithBadDebtProvision);
            AssertAreEqual(calcResultSummaryProducerDisposalFees.TotalProducerBillBreakdownSection.WalesTotalWithBadDebtProvision, result.WalesTotalForProducerBillWithBadDebtProvision);
            AssertAreEqual(calcResultSummaryProducerDisposalFees.TotalProducerBillBreakdownSection.ScotlandTotalWithBadDebtProvision, result.ScotlandTotalForProducerBillWithBadDebtProvision);
            AssertAreEqual(calcResultSummaryProducerDisposalFees.TotalProducerBillBreakdownSection.NorthernIrelandTotalWithBadDebtProvision, result.NorthernIrelandTotalForProducerBillWithBadDebtProvision);
            AssertAreEqual(calcResultSummaryProducerDisposalFees.TotalProducerBillBreakdownSection.TotalProducerFeeWithBadDebtProvision, result.TotalProducerBillWithBadDebtProvisionAmount);
        }
    }
}
