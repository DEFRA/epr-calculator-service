using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.RejectedProducers;
using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Models;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.RejectedProducers
{
    [TestClass]
    public class CalcResultRejectedProducersBuilderTests
    {
        private ApplicationDBContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDBContext(options);
        }

        [TestMethod]
        public async Task Construct_ReturnsRejectedProducers()
        {
            // Arrange
            var context = CreateDbContext();

            // Seed ProducerDetail
            context.ProducerDetail.Add(new ProducerDetail
            {
                CalculatorRunId = 1,
                ProducerId = 100,
                ProducerName = "Producer A",
                TradingName = "Trade A"
            });

            // Seed ProducerResultFileSuggestedBillingInstruction
            context.ProducerResultFileSuggestedBillingInstruction.Add(new ProducerResultFileSuggestedBillingInstruction
            {
                CalculatorRunId = 1,
                ProducerId = 100,
                SuggestedBillingInstruction = "Instruction A",
                SuggestedInvoiceAmount = 123.45m,
                BillingInstructionAcceptReject = "Rejected",
                ReasonForRejection = "Invalid Data",
                LastModifiedAcceptReject = new DateTime(2024, 1, 1),
                LastModifiedAcceptRejectBy = "User A"
            });

            await context.SaveChangesAsync();

            var builder = new CalcResultRejectedProducersBuilder(context);
            var requestDto = new CalcResultsRequestDto { RunId = 1 };

            // Act
            var result = await builder.Construct(requestDto);

            // Assert
            var list = new List<CalcResultRejectedProducer>(result);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(100, list[0].ProducerId);
            Assert.AreEqual("Producer A", list[0].ProducerName);
            Assert.AreEqual("Trade A", list[0].TradingName);
            Assert.AreEqual("Instruction A", list[0].SuggestedBillingInstruction);
            Assert.AreEqual(123.45m, list[0].SuggestedInvoiceAmount);
            Assert.AreEqual(new DateTime(2024, 1, 1), list[0].InstructionConfirmedDate);
            Assert.AreEqual("User A", list[0].InstructionConfirmedBy);
            Assert.AreEqual("Invalid Data", list[0].ReasonForRejection);
        }

        [TestMethod]
        public async Task Construct_EmptyResult_ReturnsEmptyList()
        {
            // Arrange
            var context = CreateDbContext();
            var builder = new CalcResultRejectedProducersBuilder(context);
            var requestDto = new CalcResultsRequestDto { RunId = 99 };

            // Act
            var result = await builder.Construct(requestDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, new List<CalcResultRejectedProducer>(result).Count);
        }

        [TestMethod]
        public async Task Construct_FiltersByRunIdAndRejectedStatus()
        {
            // Arrange
            var context = CreateDbContext();

            context.ProducerDetail.Add(new ProducerDetail
            {
                CalculatorRunId = 2,
                ProducerId = 200,
                ProducerName = "Producer B",
                TradingName = "Trade B"
            });

            context.ProducerResultFileSuggestedBillingInstruction.Add(new ProducerResultFileSuggestedBillingInstruction
            {
                CalculatorRunId = 2,
                ProducerId = 200,
                SuggestedBillingInstruction = "Instruction B",
                SuggestedInvoiceAmount = 222.22m,
                BillingInstructionAcceptReject = "Accepted", // Should not be included
                ReasonForRejection = null,
                LastModifiedAcceptReject = DateTime.Now,
                LastModifiedAcceptRejectBy = "User B"
            });

            context.ProducerResultFileSuggestedBillingInstruction.Add(new ProducerResultFileSuggestedBillingInstruction
            {
                CalculatorRunId = 2,
                ProducerId = 200,
                SuggestedBillingInstruction = "Instruction B",
                SuggestedInvoiceAmount = 222.22m,
                BillingInstructionAcceptReject = "Rejected", // Should be included
                ReasonForRejection = "Some Reason",
                LastModifiedAcceptReject = DateTime.Now,
                LastModifiedAcceptRejectBy = "User B"
            });

            await context.SaveChangesAsync();

            var builder = new CalcResultRejectedProducersBuilder(context);
            var requestDto = new CalcResultsRequestDto { RunId = 2 };

            // Act
            var result = await builder.Construct(requestDto);

            // Assert
            var list = new List<CalcResultRejectedProducer>(result);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("Some Reason", list[0].ReasonForRejection);
        }
    }
}
