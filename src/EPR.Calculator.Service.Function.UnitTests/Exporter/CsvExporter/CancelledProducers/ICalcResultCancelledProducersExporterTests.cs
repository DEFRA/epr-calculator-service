using EPR.Calculator.Service.Function.Exporter.CsvExporter.CancelledProducers;
using EPR.Calculator.Service.Function.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.CancelledProducers
{
    [TestClass]
    public class ICalcResultCancelledProducersExporterTests
    {
        private Mock<ICalcResultCancelledProducersExporter> _exporterMock;

        [TestInitialize]
        public void Setup()
        {
            _exporterMock = new Mock<ICalcResultCancelledProducersExporter>();
        }

        [TestMethod]
        public void Export_ShouldBeCalledWithCorrectParameters()
        {
            // Arrange
            var response = new CalcResultCancelledProducersResponse
            {
                TitleHeader = "Cancelled Producers Export",
                CancelledProducers = new List<CalcResultCancelledProducersDTO>
                {
                    new CalcResultCancelledProducersDTO
                    {
                        ProducerIdValue = "PR123",
                        TradingNameValue = "Acme Ltd",
                        LastTonnage = new LastTonnage
                        {
                            AluminiumValue = 25.5M
                        },
                        LatestInvoice = new LatestInvoice
                        {
                            LastInvoicedTotalValue = 1010.75M
                        }
                    }
                }
            };

            var sb = new StringBuilder();

            // Act
            _exporterMock.Object.Export(response, sb);

            // Assert
            _exporterMock.Verify(e => e.Export(response, sb), Times.Once);
        }
    }
}
