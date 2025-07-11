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
                LocalAuthorityDataPreparationCosts = new CalcResultSummaryBadDebtProvision
                {
                    TotalProducerFeeWithoutBadDebtProvision = 100m,
                    BadDebtProvision = 10.06m,
                    TotalProducerFeeWithBadDebtProvision = 110m,
                    EnglandTotalWithBadDebtProvision = 50m,
                    WalesTotalWithBadDebtProvision = 30m,
                    ScotlandTotalWithBadDebtProvision = 20m,
                    NorthernIrelandTotalWithBadDebtProvision = 10m
                },
                ProducerId = "Producer123",
                ProducerName = "Test Producer",
                SubsidiaryId = "Subsidiary456",
                BillingInstructionSection = new CalcResultSummaryBillingInstruction()
            };

            // Act  
            var result = _mapper.Map(input);

            // Assert  
            Assert.IsNotNull(result);            
            Assert.AreEqual("£100.00", result.TotalProducerFeeForLADataPrepCostsWithoutBadDebtProvision);
            Assert.AreEqual("£10.06", result.BadDebtProvisionFor4);
            Assert.AreEqual("£110.00", result.TotalProducerFeeForLADataPrepCostsWithBadDebtProvision);
            Assert.AreEqual("£50.00", result.EnglandTotalForLADataPrepCostsWithBadDebtProvision);
            Assert.AreEqual("£30.00", result.WalesTotalForLADataPrepCostsWithBadDebtProvision);
            Assert.AreEqual("£20.00", result.ScotlandTotalForLADataPrepCostsWithBadDebtProvision);
            Assert.AreEqual("£10.00", result.NorthernIrelandTotalForLADataPrepCostsWithBadDebtProvision);
        }
    }
}
