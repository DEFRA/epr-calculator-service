using AutoFixture;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    [TestClass]
    public class CancelledProducersMapperTests
    {
        private CancelledProducersMapper _testClass = new CancelledProducersMapper();

        [TestMethod]
        public void CanCallMap()
        {
            // Arrange
            var fixture = new Fixture();
            var cancelledProducers = fixture.Create<CalcResultCancelledProducersResponse>();

            var counter = 1;
            foreach (var item in cancelledProducers.CancelledProducers)
            {
                item.ProducerId = counter;
                counter++;
            }

            // Act
            var result  = ((ICancelledProducersMapper)_testClass).Map(cancelledProducers);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void CanCallMap_ReturnsEmptyObject()
        {
            // Arrange
            CalcResultCancelledProducersResponse? cancelledProducers = null;

            // Act
            var result = ((ICancelledProducersMapper)_testClass).Map(cancelledProducers!);

            // Assert
            Assert.AreEqual(new CancelledProducers(), result);
        }

        [TestMethod]
        public void Map_ShouldMapCancelledProducersCorrectly()
        {
            // Arrange  
            var input = new CalcResultCancelledProducersResponse
            {
                CancelledProducers = new List<CalcResultCancelledProducersDto>
               {
                   new CalcResultCancelledProducersDto
                   {
                       ProducerId = 123,
                       ProducerOrSubsidiaryNameValue = "Producer A",
                       TradingNameValue = "Trading A",
                       LastTonnage = new LastTonnage
                       {
                           AluminiumValue = 10,
                           FibreCompositeValue = 20,
                           GlassValue = 30
                       }
                   }
               }
            };

            // Act  
            var result = _testClass.Map(input);

            // Assert  
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.CancelledProducerTonnageInvoice);
            Assert.AreEqual(CommonConstants.CancelledProducers, result.Name);
            Assert.AreEqual(1, result.CancelledProducerTonnageInvoice.Count());

            var invoice = result.CancelledProducerTonnageInvoice.First();
            Assert.AreEqual(123, invoice.ProducerId);
            Assert.AreEqual("Producer A", invoice.ProducerName);
            Assert.AreEqual("Trading A", invoice.TradingName);
            Assert.AreEqual(8, invoice.LastProducerTonnages.Count());
        }
    }
}
