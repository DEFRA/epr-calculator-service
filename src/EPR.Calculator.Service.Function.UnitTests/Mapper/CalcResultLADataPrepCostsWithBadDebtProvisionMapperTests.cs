using AutoFixture;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    [TestClass]
    public class CalcResultLADataPrepCostsWithBadDebtProvision4MapperTests
    {
        private CalcResultLADataPrepCostsWithBadDebtProvision4Mapper _mapper;

        public CalcResultLADataPrepCostsWithBadDebtProvision4MapperTests()
        {
            _mapper = new CalcResultLADataPrepCostsWithBadDebtProvision4Mapper();
        }

        [TestMethod]
        public void Map_ShouldMapCorrectly()
        {
            // Arrange  
            var input = new CalcResultSummaryProducerDisposalFees
            {
                LaDataPrepCostsTotalWithoutBadDebtProvisionSection4 = 100m,
                LaDataPrepCostsBadDebtProvisionSection4 = 10m,
                LaDataPrepCostsTotalWithBadDebtProvisionSection4 = 110m,
                LaDataPrepCostsEnglandTotalWithBadDebtProvisionSection4 = 50m,
                LaDataPrepCostsWalesTotalWithBadDebtProvisionSection4 = 30m,
                LaDataPrepCostsScotlandTotalWithBadDebtProvisionSection4 = 20m,
                LaDataPrepCostsNorthernIrelandTotalWithBadDebtProvisionSection4 = 10m,
                ProducerId = "Producer123",
                ProducerName = "Test Producer",
                SubsidiaryId = "Subsidiary456",
            };

            // Act  
            var result = _mapper.Map(input);

            // Assert  
            Assert.IsNotNull(result);            
            Assert.AreEqual(100m, result.TotalProducerFeeForLADataPrepCostsWithoutBadDebtProvision);
            Assert.AreEqual(10m, result.BadDebtProvisionFor4);
            Assert.AreEqual(110m, result.TotalProducerFeeForLADataPrepCostsWithBadDebtProvision);
            Assert.AreEqual(50m, result.EnglandTotalForLADataPrepCostsWithBadDebtProvision);
            Assert.AreEqual(30m, result.WalesTotalForLADataPrepCostsWithBadDebtProvision);
            Assert.AreEqual(20m, result.ScotlandTotalForLADataPrepCostsWithBadDebtProvision);
            Assert.AreEqual(10m, result.NorthernIrelandTotalForLADataPrepCostsWithBadDebtProvision);
        }
    }
}
