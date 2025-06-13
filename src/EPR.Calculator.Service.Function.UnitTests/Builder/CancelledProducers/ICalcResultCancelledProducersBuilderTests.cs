using EPR.Calculator.Service.Function.Builder.CancelledProducers;
using EPR.Calculator.Service.Function.Builder.LateReportingTonnages;
using EPR.Calculator.Service.Function.Builder.ScaledupProducers;
using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Models;
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

        [TestInitialize]
        public void Setup()
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
                CancelledProducers = new List<CalcResultCancelledProducersDTO>
                {
                    new CalcResultCancelledProducersDTO
                    {
                        ProducerIdValue = "P123",
                        TradingNameValue = "ABC Ltd",
                        LastTonnage = new LastTonnage
                        {
                            AluminiumValue = 123.45M
                        },
                        LatestInvoice = new LatestInvoice
                        {
                            LastInvoicedTotalValue = 999.99M
                        }
                    }
                }
            };

            _builderMock
                .Setup(b => b.Construct(It.IsAny<CalcResultsRequestDto>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _builderMock.Object.Construct(requestDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Cancelled Producers", result.TitleHeader);
            var producer = AssertSingle(result.CancelledProducers);
            Assert.AreEqual("P123", producer.ProducerIdValue);
            Assert.AreEqual(123.45M, producer.LastTonnage?.AluminiumValue);
            Assert.AreEqual(999.99M, producer.LatestInvoice?.LastInvoicedTotalValue);
        }
        private T AssertSingle<T>(IEnumerable<T> enumerable)
        {
            var list = new List<T>(enumerable);
            Assert.AreEqual(1, list.Count);
            return list[0];
        }
    }
}
