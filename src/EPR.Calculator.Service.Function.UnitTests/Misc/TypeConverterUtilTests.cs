namespace EPR.Calculator.Service.Function.UnitTests.Misc
{
    using System;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Misc;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using T = System.String;

    [TestClass]
    public class TypeConverterUtilTests
    {
        [TestMethod]
        public void CanCallConvertToDecimal()
        {
            // Arrange
            var fixture = new Fixture();
            var value = "10.6";

            // Act
            var result = TypeConverterUtil.ConvertTo<decimal>(value);

            // Assert
            Assert.AreEqual(result,10.6m);
        }

        [TestMethod]
        public void CanCallConvertToWithValueNull()
        {
            // Arrange
            var fixture = new Fixture();
            string? value = null;

            // Act
            var result = TypeConverterUtil.ConvertTo<decimal>(value);

            // Assert
            Assert.AreEqual(result, 0);
        }

        [TestMethod]
        public void CanCallConvertToWithValueGuid()
        {
            // Arrange
            var fixture = new Fixture();
            string value = Guid.NewGuid().ToString();

            // Act
            var result = TypeConverterUtil.ConvertTo<Guid>(value);

            // Assert
            Assert.AreEqual(result, Guid.Parse(value));
        }

        [TestMethod]
        public void CannotCallConvertToWithObject()
        {
            // Arrange
            var fixture = new Fixture();
            var value = new object();

            // Act
            var result = TypeConverterUtil.ConvertTo<decimal>(value);


            // Assert
            Assert.AreEqual(result, 0);
        }
    }
}