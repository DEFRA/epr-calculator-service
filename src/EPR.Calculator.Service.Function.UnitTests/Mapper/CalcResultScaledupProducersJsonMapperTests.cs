using AutoFixture;
using EPR.Calculator.Service.Function.Mappers;
using EPR.Calculator.Service.Function.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    [TestClass]
    public class CalcResultScaledupProducersJsonMapperTests
    {
        private CalcResultScaledupProducersJsonMapper _testClass;

        [TestInitialize]
        public void Setup()
        {
            _testClass = new CalcResultScaledupProducersJsonMapper();
        }
        
        [TestMethod]
        public void CanCallMap()
        {
            // Arrange
            var fixture = new Fixture();
            var calcResultScaledupProducers = fixture.Create<CalcResultScaledupProducers>();

            // Act
            var result = ((ICalcResultScaledupProducersJsonMapper)_testClass).Map(calcResultScaledupProducers);

            // Assert
            Assert.IsNotNull(result);
        }

    }
}
