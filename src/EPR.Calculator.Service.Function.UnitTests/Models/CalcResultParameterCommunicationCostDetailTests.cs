using AutoFixture;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.UnitTests.Models
{
    [TestClass]
    public class CalcResultParameterCommunicationCostDetailTests
    {
        private CalcResultParameterCommunicationCostDetail TestClass;
        private IFixture Fixture;

        public CalcResultParameterCommunicationCostDetailTests()
        {
            Fixture = new Fixture();
            TestClass = Fixture.Create<CalcResultParameterCommunicationCostDetail>();
        }

        [TestMethod]
        public void CanSetAndGetMaterial()
        {
            // Arrange
            var testValue = Fixture.Create<string>();

            // Act
            TestClass.Material = testValue;

            // Assert
            Assert.AreEqual(testValue, TestClass.Material);
        }

        [TestMethod]
        public void CanSetAndGetCost()
        {
            // Arrange
            var testValue = Fixture.Create<decimal>();

            // Act
            TestClass.Cost = testValue;

            // Assert
            Assert.AreEqual(testValue, TestClass.Cost);
        }

        [TestMethod]
        public void CanSetAndGetCountry()
        {
            // Arrange
            var testValue = Fixture.Create<string>();

            // Act
            TestClass.Country = testValue;

            // Assert
            Assert.AreEqual(testValue, TestClass.Country);
        }

        [TestMethod]
        public void CanSetAndGetIsApportionment()
        {
            // Arrange
            var testValue = Fixture.Create<bool>();

            // Act
            TestClass.IsApportionment = testValue;

            // Assert
            Assert.AreEqual(testValue, TestClass.IsApportionment);
        }

        [TestMethod]
        public void CanSetAndGetIsPercentage()
        {
            // Arrange
            var testValue = Fixture.Create<bool>();

            // Act
            TestClass.IsPercentage = testValue;

            // Assert
            Assert.AreEqual(testValue, TestClass.IsPercentage);
        }
    }
}