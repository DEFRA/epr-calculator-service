using AutoFixture;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    [TestClass]
    public class CalcResultLADataPrepCostsWithBadDebtProvisionMapperTests
    {
        private CalcResultLADataPrepCostsWithBadDebtProvisionMapper _testClass;

        [TestInitialize]
        public void SetUp()
        {
            _testClass = new CalcResultLADataPrepCostsWithBadDebtProvisionMapper();
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
            Assert.AreEqual(result.TotalProducerFeeForLADataPrepCostsWithoutBadDebtProvision, fees.LaDataPrepCostsTotalWithoutBadDebtProvisionSection4);
            Assert.AreEqual(result.BadDebtProvisionFor4, fees.LaDataPrepCostsBadDebtProvisionSection4);
            Assert.AreEqual(result.TotalProducerFeeForLADataPrepCostsWithBadDebtProvision, fees.LaDataPrepCostsTotalWithBadDebtProvisionSection4);
            Assert.AreEqual(result.EnglandTotalForLADataPrepCostsWithBadDebtProvision, fees.LaDataPrepCostsEnglandTotalWithBadDebtProvisionSection4);
            Assert.AreEqual(result.WalesTotalForLADataPrepCostsWithBadDebtProvision, fees.LaDataPrepCostsWalesTotalWithBadDebtProvisionSection4);
            Assert.AreEqual(result.ScotlandTotalForLADataPrepCostsWithBadDebtProvision, fees.LaDataPrepCostsScotlandTotalWithBadDebtProvisionSection4);
            Assert.AreEqual(result.NorthernIrelandTotalForLADataPrepCostsWithBadDebtProvision, fees.LaDataPrepCostsNorthernIrelandTotalWithBadDebtProvisionSection4);
        }
    }
}
