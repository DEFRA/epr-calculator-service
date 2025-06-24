using AutoFixture;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;

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
            Assert.AreEqual(calcResultSummaryProducerDisposalFees.TotalProducerBillWithoutBadDebtProvision, result.TotalProducerBillWithoutBadDebtProvision);
            Assert.AreEqual(calcResultSummaryProducerDisposalFees.BadDebtProvisionForTotalProducerBill, result.BadDebtProvisionForTotalProducerBill);
            Assert.AreEqual(calcResultSummaryProducerDisposalFees.EnglandTotalWithBadDebtProvision, result.EnglandTotalForProducerBillWithBadDebtProvision);
            Assert.AreEqual(calcResultSummaryProducerDisposalFees.WalesTotalWithBadDebtProvision, result.WalesTotalForProducerBillWithBadDebtProvision);
            Assert.AreEqual(calcResultSummaryProducerDisposalFees.ScotlandTotalWithBadDebtProvision, result.ScotlandTotalForProducerBillWithBadDebtProvision);
            Assert.AreEqual(calcResultSummaryProducerDisposalFees.NorthernIrelandTotalWithBadDebtProvision, result.NorthernIrelandTotalForProducerBillWithBadDebtProvision);
            Assert.AreEqual(calcResultSummaryProducerDisposalFees.TotalProducerBillWithBadDebtProvision, result.TotalProducerBillWithBadDebtProvisionAmount);
        }
    }
}
