using EPR.Calculator.Service.Function.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.UnitTests.Models
{
    [TestClass]
    public class CalcResultCancelledProducersResponseTests
    {
        [TestMethod]
        public void CalcResultCancelledProducersResponse_ShouldInitializeProperties()
        {
            // Arrange  
            var response = new CalcResultCancelledProducersResponse();

            // Act  
            response.TitleHeader = "Test Title";
            response.CancelledProducers = new List<CalcResultCancelledProducersDto>
           {
               new CalcResultCancelledProducersDto
               {
                   ProducerId_Header = "Producer ID Header",
                   ProducerIdValue = "Producer ID Value"
               }
           };

            // Assert  
            Assert.AreEqual("Test Title", response.TitleHeader);
            Assert.IsNotNull(response.CancelledProducers);
            Assert.AreEqual(1, response.CancelledProducers.Count());
            Assert.AreEqual("Producer ID Header", response.CancelledProducers.First().ProducerId_Header);
            Assert.AreEqual("Producer ID Value", response.CancelledProducers.First().ProducerIdValue);
        }

        [TestMethod]
        public void CalcResultCancelledProducersDTO_ShouldInitializeProperties()
        {
            // Arrange  
            var dto = new CalcResultCancelledProducersDto();

            // Act  
            dto.ProducerId_Header = "Producer ID Header";
            dto.ProducerIdValue = "Producer ID Value";
            dto.LastTonnage = new LastTonnage
            {
                AluminiumValue = 100.5m
            };
            dto.LatestInvoice = new LatestInvoice
            {
                CurrentYearInvoicedTotalToDateValue = 200.75m
            };

            // Assert  
            Assert.AreEqual("Producer ID Header", dto.ProducerId_Header);
            Assert.AreEqual("Producer ID Value", dto.ProducerIdValue);
            Assert.IsNotNull(dto.LastTonnage);
            Assert.AreEqual(100.5m, dto.LastTonnage.AluminiumValue);
            Assert.IsNotNull(dto.LatestInvoice);
            Assert.AreEqual(200.75m, dto.LatestInvoice.CurrentYearInvoicedTotalToDateValue);
        }
    }
}
