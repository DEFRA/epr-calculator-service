namespace EPR.Calculator.Service.Function.UnitTests.Models
{
    using System;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalcResultScaledupProducerHeaderTests
    {
        private CalcResultScaledupProducerHeader calcResultScaledupProducerHeader;

        [TestInitialize]
        public void SetUp()
        {
            calcResultScaledupProducerHeader = new CalcResultScaledupProducerHeader
            {
                Name = "Some column header name",
                ColumnIndex = 1,
            };
        }

        [TestMethod]
        public void CanSetAndGetName()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            calcResultScaledupProducerHeader.Name = testValue;

            // Assert
            Assert.AreEqual(testValue, calcResultScaledupProducerHeader.Name);
        }

        [TestMethod]
        public void CanSetAndGetColumnIndex()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<int?>();

            // Act
            calcResultScaledupProducerHeader.ColumnIndex = testValue;

            // Assert
            Assert.AreEqual(testValue, calcResultScaledupProducerHeader.ColumnIndex);
        }
    }
}