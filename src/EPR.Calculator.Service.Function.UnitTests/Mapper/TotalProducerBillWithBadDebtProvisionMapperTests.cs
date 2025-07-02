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
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TotalProducerBillWithoutBadDebtProvision), result.TotalProducerBillWithoutBadDebtProvision);
            AssertAreEqual(calcResultSummaryProducerDisposalFees.BadDebtProvisionForTotalProducerBill, result.BadDebtProvisionForTotalProducerBill);
            AssertAreEqual(calcResultSummaryProducerDisposalFees.EnglandTotalWithBadDebtProvisionTotalBill, result.EnglandTotalForProducerBillWithBadDebtProvision);
            AssertAreEqual(calcResultSummaryProducerDisposalFees.WalesTotalWithBadDebtProvisionTotalBill, result.WalesTotalForProducerBillWithBadDebtProvision);
            AssertAreEqual(calcResultSummaryProducerDisposalFees.ScotlandTotalWithBadDebtProvisionTotalBill, result.ScotlandTotalForProducerBillWithBadDebtProvision);
            AssertAreEqual(calcResultSummaryProducerDisposalFees.NorthernIrelandTotalWithBadDebtProvisionTotalBill, result.NorthernIrelandTotalForProducerBillWithBadDebtProvision);
            AssertAreEqual(calcResultSummaryProducerDisposalFees.TotalProducerBillWithBadDebtProvision, result.TotalProducerBillWithBadDebtProvisionAmount);
        }
    }
}
