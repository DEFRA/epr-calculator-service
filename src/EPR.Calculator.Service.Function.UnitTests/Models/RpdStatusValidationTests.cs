namespace EPR.Calculator.Service.Function.UnitTests.Models
{
    using System;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RpdStatusValidationTests
    {
        private RpdStatusValidation _testClass;

        public RpdStatusValidationTests()
        {
            _testClass = new RpdStatusValidation();
        }

        [TestMethod]
        public void CanSetAndGetisValid()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<bool>();

            // Act
            _testClass.isValid = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.isValid);
        }

        [TestMethod]
        public void CanSetAndGetStatusCode()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<int>();

            // Act
            _testClass.StatusCode = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.StatusCode);
        }

        [TestMethod]
        public void CanSetAndGetErrorMessage()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            _testClass.ErrorMessage = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.ErrorMessage);
        }
    }
}