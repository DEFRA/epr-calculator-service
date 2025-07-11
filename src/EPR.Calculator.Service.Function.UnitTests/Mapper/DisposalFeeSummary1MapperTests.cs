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
        public void Map_ShouldMapValues_ForLevelOne()
        {
            // Arrange
            var calcResultSummaryProducerDisposalFees = TestDataHelper.GetProducerDisposalFees()[0];
            calcResultSummaryProducerDisposalFees.Level = "1";
            calcResultSummaryProducerDisposalFees.TonnageChangeCount = "-";
            calcResultSummaryProducerDisposalFees.TonnageChangeAdvice = "-";

            // Act
            var result = mapper.Map(calcResultSummaryProducerDisposalFees);

            // Assert  
            Assert.IsNotNull(result);
            Assert.AreEqual("0", result.TonnageChangeCount);
            Assert.AreEqual("", result.TonnageChangeAdvice);
        }

        [TestMethod]
        public void Map_ShouldMapValues_ForLevelTwo()
        {
            // Arrange
            var calcResultSummaryProducerDisposalFees = TestDataHelper.GetProducerDisposalFees()[0];
            calcResultSummaryProducerDisposalFees.Level = "2";

            // Act
            var result = mapper.Map(calcResultSummaryProducerDisposalFees);

            // Assert  
            Assert.IsNotNull(result);
            Assert.AreEqual("-", result.TonnageChangeCount);
            Assert.AreEqual("-", result.TonnageChangeAdvice);
        }
    }
}
