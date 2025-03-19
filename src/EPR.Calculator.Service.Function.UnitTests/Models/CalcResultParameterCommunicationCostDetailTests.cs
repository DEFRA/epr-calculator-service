namespace EPR.Calculator.Service.Function.UnitTests.Models
{
    using System;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalcResultParameterCommunicationCostDetailTests
    {
        private CalcResultParameterCommunicationCostDetail? TestClass;
        private IFixture? Fixture;

        [TestInitialize]
        public void SetUp()
        {
            Fixture = new Fixture();
            this.TestClass = Fixture.Create<CalcResultParameterCommunicationCostDetail>();
        }

        [TestMethod]
        public void CanSetAndGetMaterial()
        {
            // Arrange
            var testValue = Fixture.Create<string>();

            // Act
            this.TestClass.Material = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.Material);
        }

        [TestMethod]
        public void CanSetAndGetCost()
        {
            // Arrange
            var testValue = Fixture.Create<decimal>();

            // Act
            this.TestClass.Cost = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.Cost);
        }

        [TestMethod]
        public void CanSetAndGetCountry()
        {
            // Arrange
            var testValue = Fixture.Create<string>();

            // Act
            this.TestClass.Country = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.Country);
        }

        [TestMethod]
        public void CanSetAndGetIsApportionment()
        {
            // Arrange
            var testValue = Fixture.Create<bool>();

            // Act
            this.TestClass.IsApportionment = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.IsApportionment);
        }

        [TestMethod]
        public void CanSetAndGetIsPercentage()
        {
            // Arrange
            var testValue = Fixture.Create<bool>();

            // Act
            this.TestClass.IsPercentage = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.IsPercentage);
        }
    }
}