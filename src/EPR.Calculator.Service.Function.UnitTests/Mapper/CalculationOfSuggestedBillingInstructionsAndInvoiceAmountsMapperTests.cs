using AutoFixture;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    [TestClass]
    public class CalculationOfSuggestedBillingInstructionsAndInvoiceAmountsMapperTests
    {

        private CalculationOfSuggestedBillingInstructionsAndInvoiceAmountsMapper? _testClass;

        [TestInitialize]
        public void Setup()
        {
            _testClass = new CalculationOfSuggestedBillingInstructionsAndInvoiceAmountsMapper();
        }

        [TestMethod]
        public void CanCallMap()
        {
            // Arrange
            var fixture = new Fixture();
            var fees = fixture.Create<CalcResultSummaryProducerDisposalFees>();


            // Act
            var result = _testClass?.Map(fees);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Map_ShouldMapAllPropertiesCorrectly()
        {
            // Arrange  
            var fees = new CalcResultSummaryProducerDisposalFees
            {
                CurrentYearInvoiceTotalToDate = "1000",
                TonnageChangeSinceLastInvoice = "Tonnage Changed",
                LiabilityDifference = "-200",
                MaterialThresholdBreached = "-ve",
                TonnageThresholdBreached = "-ve",
                PercentageLiabilityDifference = "10.05",
                MaterialPercentageThresholdBreached = "-ve",
                TonnagePercentageThresholdBreached = "-ve",
                SuggestedBillingInstruction = "INITIAL",
                SuggestedInvoiceAmount = "500",
                ProducerId = "Producer123",
                ProducerName = "Producer Name",
                SubsidiaryId = "Subsidiary456",
            };

            // Act  
            var result = _testClass?.Map(fees);
            
            Assert.IsNotNull(result);

            // Assert  
            Assert.AreEqual("£1,000.00", result.CurrentYearInvoicedTotalToDate);
            Assert.AreEqual("Tonnage Changed", result.TonnageChangeSinceLastInvoice);
            Assert.AreEqual("-£200.00", result.LiabilityDifferenceCalcVsPrev);
            Assert.AreEqual("-ve", result.MaterialThresholdBreached);
            Assert.AreEqual("-ve", result.TonnageThresholdBreached);
            Assert.AreEqual("10.05%", result.PercentageLiabilityDifferenceCalcVsPrev);
            Assert.AreEqual("-ve", result.MaterialPercentageThresholdBreached);
            Assert.AreEqual("-ve", result.TonnagePercentageThresholdBreached);
            Assert.AreEqual("INITIAL", result.SuggestedBillingInstruction);
            Assert.AreEqual("£500.00", result.SuggestedInvoiceAmount);
        }


        [TestMethod]
        public void Map_ShouldHandleNullValues()
        {
            // Arrange  
            var fees = new CalcResultSummaryProducerDisposalFees
            {
                ProducerId = "Producer123",
                ProducerName = "Producer Name",
                SubsidiaryId = "Subsidiary456",
            };

            // Act  
            var result = _testClass?.Map(fees);
            Assert.IsNotNull(result);

            // Assert  
            Assert.AreEqual(string.Empty, result.CurrentYearInvoicedTotalToDate);
            Assert.AreEqual(string.Empty, result.TonnageChangeSinceLastInvoice);
            Assert.AreEqual(string.Empty, result.LiabilityDifferenceCalcVsPrev);
            Assert.AreEqual(string.Empty, result.MaterialThresholdBreached);
            Assert.AreEqual(string.Empty, result.TonnageThresholdBreached);
            Assert.AreEqual(string.Empty, result.PercentageLiabilityDifferenceCalcVsPrev);
            Assert.AreEqual(string.Empty, result.MaterialPercentageThresholdBreached);
            Assert.AreEqual(string.Empty, result.TonnagePercentageThresholdBreached);
            Assert.AreEqual(string.Empty, result.SuggestedBillingInstruction);
            Assert.AreEqual(string.Empty, result.SuggestedInvoiceAmount);
        }

    }
}
