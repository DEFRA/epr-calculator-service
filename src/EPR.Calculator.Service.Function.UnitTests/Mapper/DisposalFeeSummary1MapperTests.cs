using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.UnitTests.Builder;

namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    [TestClass]
    public class DisposalFeeSummary1MapperTests
    {
        private DisposalFeeSummary1Mapper mapper;

        public DisposalFeeSummary1MapperTests()
        {
            this.mapper = new DisposalFeeSummary1Mapper();
        }

        [TestMethod]
        public void Map_ShouldMapValues()
        {
            // Arrange
            var calcResultSummaryProducerDisposalFees = TestDataHelper.GetProducerDisposalFees();

            // Act
            var result = mapper.Map(calcResultSummaryProducerDisposalFees[0]);

            // Assert  
            Assert.IsNotNull(result);
        }
    }
}
