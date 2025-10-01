using EPR.Calculator.Service.Function.Builder.CancelledProducers;
using EPR.Calculator.Service.Function.Builder.LateReportingTonnages;
using EPR.Calculator.Service.Function.Builder.ScaledupProducers;
using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.CancelledProducers
{
    [TestClass]
    public class ICalcResultCancelledProducersBuilderTests
    {
        private Mock<ICalcResultCancelledProducersBuilder> _builderMock;

        public ICalcResultCancelledProducersBuilderTests()
        {
            _builderMock = new Mock<ICalcResultCancelledProducersBuilder>();
        }

        [TestMethod]
        public async Task Construct_ShouldReturnCancelledProducersResponse()
        {
            // Arrange
            var requestDto = new CalcResultsRequestDto
            {
                // populate required properties here
            };

            var expectedResponse = new CalcResultCancelledProducersResponse
            {
                TitleHeader = "Cancelled Producers",
                CancelledProducers = new List<CalcResultCancelledProducersDto>
                {
                    new CalcResultCancelledProducersDto
                    {
                        ProducerId = 123,
                        TradingNameValue = "ABC Ltd",
                        LastTonnage = new LastTonnage
                        {
                            AluminiumValue = 123.45M
                        },
                        LatestInvoice = new LatestInvoice
                        {
                            CurrentYearInvoicedTotalToDateValue = 999.99M
                        }
                    }
                }
            };

            _builderMock
                .Setup(b => b.ConstructAsync(It.IsAny<CalcResultsRequestDto>(), It.IsAny<string>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _builderMock.Object.ConstructAsync(requestDto,"2025-26");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Cancelled Producers", result.TitleHeader);
            var producer = AssertSingle(result.CancelledProducers);
            Assert.AreEqual(123, producer.ProducerId);
            Assert.AreEqual(123.45M, producer.LastTonnage?.AluminiumValue);
            Assert.AreEqual(999.99M, producer.LatestInvoice?.CurrentYearInvoicedTotalToDateValue);
        }
        private T AssertSingle<T>(IEnumerable<T> enumerable)
        {
            var list = new List<T>(enumerable);
            Assert.AreEqual(1, list.Count);
            return list[0];
        }
    }
}
