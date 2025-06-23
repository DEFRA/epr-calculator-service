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
            var result = (_testClass as ICalculationOfSuggestedBillingInstructionsAndInvoiceAmountsMapper).Map(fees);

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
                TonnageChangeSinceLastInvoice = "50",
                LiabilityDifference = "200",
                MaterialThresholdBreached = "Yes",
                TonnageThresholdBreached = "No",
                PercentageLiabilityDifference = "10%",
                MaterialPercentageThresholdBreached = "5%",
                TonnagePercentageThresholdBreached = "2%",
                SuggestedBillingInstruction = "Instruction1",
                SuggestedInvoiceAmount = "500",
                ProducerId = "Producer123",
                ProducerName = "Producer Name",
                SubsidiaryId = "Subsidiary456",
            };

            // Act  
            var result = _testClass.Map(fees);

            // Assert  
            Assert.AreEqual("1000", result.CurrentYearInvoicedTotalToDate);
            Assert.AreEqual("50", result.TonnageChangeSinceLastInvoice);
            Assert.AreEqual("200", result.LiabilityDifferenceCalcVsPrev);
            Assert.AreEqual("Yes", result.MaterialThresholdBreached);
            Assert.AreEqual("No", result.TonnageThresholdBreached);
            Assert.AreEqual("10%", result.PercentageLiabilityDifferenceCalcVsPrev);
            Assert.AreEqual("5%", result.MaterialPercentageThresholdBreached);
            Assert.AreEqual("2%", result.TonnagePercentageThresholdBreached);
            Assert.AreEqual("Instruction1", result.SuggestedBillingInstruction);
            Assert.AreEqual("500", result.SuggestedInvoiceAmount);
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
            var result = _testClass.Map(fees);

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
