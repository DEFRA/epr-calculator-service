using AutoFixture;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    [TestClass]
    public class CancelledProducerMapperTests
    {
        private CancelledProducerMapper _testClass;

        [TestInitialize]
        public void Setup()
        {
            _testClass = new CancelledProducerMapper();
        }

        [TestMethod]
        public void CanCallMap()
        {
            // Arrange
            var fixture = new Fixture();
            var cancelledProducers = fixture.Create<CalcResultCancelledProducersResponse>();

            // Act
            var result  = ((ICancelledProducerMapper)_testClass).Map(cancelledProducers);

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
