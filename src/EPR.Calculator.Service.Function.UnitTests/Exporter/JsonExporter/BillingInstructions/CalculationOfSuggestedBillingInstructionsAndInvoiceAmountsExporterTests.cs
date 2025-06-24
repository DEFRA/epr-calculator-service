using AutoFixture;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.BillingInstructions;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using Moq;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.JsonExporter.BillingInstructions
{
    [TestClass]
    public class CalculationOfSuggestedBillingInstructionsAndInvoiceAmountsExporterTests
    {
        private CalculationOfSuggestedBillingInstructionsAndInvoiceAmountsExporter? _testClass;
        private Mock<ICalculationOfSuggestedBillingInstructionsAndInvoiceAmountsMapper>? _testMapper;

        [TestInitialize]
        public void Setup()
        {
            _testMapper = new Mock<ICalculationOfSuggestedBillingInstructionsAndInvoiceAmountsMapper>();
            _testClass = new CalculationOfSuggestedBillingInstructionsAndInvoiceAmountsExporter(_testMapper.Object);
        }

        [TestMethod]
        public void CanCallExport()
        {
            // Arrange
            var fixture = new Fixture();
            var fees = fixture.Create<CalcResultSummaryProducerDisposalFees>();

            _testMapper?.Setup(mock => mock.Map(It.IsAny<CalcResultSummaryProducerDisposalFees>())).Returns(fixture.Create<CalculationOfSuggestedBillingInstructionsAndInvoiceAmounts>());

            // Act
            var result = _testClass?.Export(fees);

            // Assert
            _testMapper?.Verify(mock => mock.Map(It.IsAny<CalcResultSummaryProducerDisposalFees>()));

            Assert.AreNotEqual(string.Empty, result);
        }
    }
}
