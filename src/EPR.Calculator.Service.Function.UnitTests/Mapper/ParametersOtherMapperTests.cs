using AutoFixture;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    [TestClass]
    public class ParametersOtherMapperTests
    {
        private ParametersOtherMapper _testClass;

        [TestInitialize]
        public void Setup()
        {
            _testClass = new ParametersOtherMapper();
        }

        [TestMethod]
        public void CanCallMap()
        {
            // Arrange
            var fixture = new Fixture();
            var otherCost = fixture.Create<CalcResultParameterOtherCost>();

            // Act
            var result = ((IParametersOtherMapper)_testClass).Map(otherCost);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ParametersOther);
        }
    }
}