using AutoFixture;
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
                item.ProducerIdValue = counter.ToString();
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
    }
}
