using AutoFixture;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using static EPR.Calculator.Service.Common.UnitTests.Utils.JsonNodeComparer;

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
                BillingInstructionSection = new CalcResultSummaryBillingInstruction
                {
                    CurrentYearInvoiceTotalToDate = 1000m,
                    TonnageChangeSinceLastInvoice = "Tonnage Changed",
                    LiabilityDifference = -200,
                    MaterialThresholdBreached = "-ve",
                    TonnageThresholdBreached = "-ve",
                    PercentageLiabilityDifference = 10.05m,
                    MaterialPercentageThresholdBreached = "-ve",
                    TonnagePercentageThresholdBreached = "-ve",
                    SuggestedBillingInstruction = "INITIAL",
                    SuggestedInvoiceAmount = "500"
                },
                ProducerId = "Producer123",
                ProducerName = "Producer Name",
                SubsidiaryId = "Subsidiary456",
            };

            // Act  
            var result = _testClass?.Map(fees);
            
            Assert.IsNotNull(result);

            // Assert  
            Assert.AreEqual("£1000.00", result.CurrentYearInvoicedTotalToDate);
            Assert.AreEqual("Tonnage Changed", result.TonnageChangeSinceLastInvoice);
            Assert.AreEqual(-200.00m, result.LiabilityDifferenceCalcVsPrev);
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
                BillingInstructionSection = new CalcResultSummaryBillingInstruction
                {
                    CurrentYearInvoiceTotalToDate = null,
                    TonnageChangeSinceLastInvoice = null,
                    LiabilityDifference = null,
                    MaterialThresholdBreached = null,
                    TonnageThresholdBreached = null,
                    PercentageLiabilityDifference = null,
                    MaterialPercentageThresholdBreached = null,
                    TonnagePercentageThresholdBreached = null,
                    SuggestedBillingInstruction = null,
                    SuggestedInvoiceAmount = null
                }
            };

            // Act  
            var result = _testClass?.Map(fees);
            Assert.IsNotNull(result);

            // Assert  
            Assert.AreEqual("-", result.CurrentYearInvoicedTotalToDate);
            Assert.AreEqual("-", result.TonnageChangeSinceLastInvoice);
            Assert.AreEqual(null, result.LiabilityDifferenceCalcVsPrev);
            Assert.AreEqual(string.Empty, result.MaterialThresholdBreached);
            Assert.AreEqual(string.Empty, result.TonnageThresholdBreached);
            Assert.AreEqual("-", result.PercentageLiabilityDifferenceCalcVsPrev);
            Assert.AreEqual(string.Empty, result.MaterialPercentageThresholdBreached);
            Assert.AreEqual(string.Empty, result.TonnagePercentageThresholdBreached);
            Assert.AreEqual(string.Empty, result.SuggestedBillingInstruction);
            Assert.AreEqual(string.Empty, result.SuggestedInvoiceAmount);
        }

        [TestMethod]
        public void Map_ShouldHandleHyphenValues()
        {
            // Arrange  
            var fees = new CalcResultSummaryProducerDisposalFees
            {
                BillingInstructionSection = new CalcResultSummaryBillingInstruction
                {
                    CurrentYearInvoiceTotalToDate = null,
                    TonnageChangeSinceLastInvoice = "-",
                    LiabilityDifference = null,
                    MaterialThresholdBreached = "-",
                    TonnageThresholdBreached = "-",
                    PercentageLiabilityDifference = null,
                    MaterialPercentageThresholdBreached = "-",
                    TonnagePercentageThresholdBreached = "-",
                    SuggestedBillingInstruction = "INITIAL",
                    SuggestedInvoiceAmount = "500"
                },
                ProducerId = "Producer123",
                ProducerName = "Producer Name",
                SubsidiaryId = "Subsidiary456",
            };

            // Act  
            var result = _testClass?.Map(fees);

            Assert.IsNotNull(result);

            // Assert  
            Assert.AreEqual("-", result.CurrentYearInvoicedTotalToDate);
            Assert.AreEqual("-", result.TonnageChangeSinceLastInvoice);
            Assert.AreEqual(null, result.LiabilityDifferenceCalcVsPrev);
            Assert.AreEqual("-", result.MaterialThresholdBreached);
            Assert.AreEqual("-", result.TonnageThresholdBreached);
            Assert.AreEqual("-", result.PercentageLiabilityDifferenceCalcVsPrev);
            Assert.AreEqual("-", result.MaterialPercentageThresholdBreached);
            Assert.AreEqual("-", result.TonnagePercentageThresholdBreached);
            Assert.AreEqual("INITIAL", result.SuggestedBillingInstruction);
            Assert.AreEqual("£500.00", result.SuggestedInvoiceAmount);
        }

    }
}
