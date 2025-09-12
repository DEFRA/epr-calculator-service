using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.RejectedProducers;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.RejectedProducers
{
    [TestClass]
    public class CalcResultRejectedProducersExporterTests
    {
        [TestMethod]
        public void Export_WritesDataRows()
        {
            // Arrange
            var exporter = new CalcResultRejectedProducersExporter();
            var csvContent = new StringBuilder();
            var rejectedProducers = new List<CalcResultRejectedProducer>
            {
                new CalcResultRejectedProducer
                {
                    ProducerId = 1,
                    ProducerName = "Producer A",
                    TradingName = "Trade A",
                    SuggestedBillingInstruction = "Bill A",
                    SuggestedInvoiceAmount = 123.45m,
                    InstructionConfirmedDate = new DateTime(2024, 1, 2),
                    InstructionConfirmedBy = "User A",
                    ReasonForRejection = "Reason A"
                }
            };

            // Act
            exporter.Export(rejectedProducers, csvContent);

            // Assert
            var lines = csvContent.ToString().Split(Environment.NewLine, StringSplitOptions.None);
            Assert.IsTrue(lines[3].Contains("1")); // ProducerId
            Assert.IsTrue(lines[3].Contains("Producer A")); // ProducerName
            Assert.IsTrue(lines[3].Contains("Trade A")); // TradingName
            Assert.IsTrue(lines[3].Contains("Bill A")); // SuggestedBillingInstruction
            Assert.IsTrue(lines[3].Contains("123.45")); // SuggestedInvoiceAmount
            Assert.IsTrue(lines[3].Contains("2024")); // InstructionConfirmedDate
            Assert.IsTrue(lines[3].Contains("User A")); // InstructionConfirmedBy
        }

        [TestMethod]
        public void Export_MultipleProducers_WritesMultipleRows()
        {
            // Arrange
            var exporter = new CalcResultRejectedProducersExporter();
            var csvContent = new StringBuilder();
            var rejectedProducers = new List<CalcResultRejectedProducer>
            {
                new CalcResultRejectedProducer
                {
                    ProducerId = 1,
                    ProducerName = "Producer A",
                    TradingName = "Trade A",
                    SuggestedBillingInstruction = "Bill A",
                    SuggestedInvoiceAmount = 123.45m,
                    InstructionConfirmedDate = new DateTime(2024, 1, 2),
                    InstructionConfirmedBy = "User A",
                    ReasonForRejection = "Reason A"
                },
                new CalcResultRejectedProducer
                {
                    ProducerId = 2,
                    ProducerName = "Producer B",
                    TradingName = "Trade B",
                    SuggestedBillingInstruction = "Bill B",
                    SuggestedInvoiceAmount = 678.90m,
                    InstructionConfirmedDate = new DateTime(2024, 2, 3),
                    InstructionConfirmedBy = "User B",
                    ReasonForRejection = "Reason B"
                }
            };

            // Act
            exporter.Export(rejectedProducers, csvContent);

            // Assert
            var lines = csvContent.ToString().Split(Environment.NewLine, StringSplitOptions.None);
            Assert.IsTrue(lines[3].Contains("Producer A"));
            Assert.IsTrue(lines[4].Contains("Producer B"));
        }
    }
}
