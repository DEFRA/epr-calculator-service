namespace EPR.Calculator.Service.Function.UnitTests
{
    using System;
    using AutoFixture;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CurrencyConverterTests
    {
        [TestMethod]
        public void CanCallGetDecimalValue()
        {
            // Act
            var result = CurrencyConverter.GetDecimalValue("650.95");

            // Assert
            Assert.AreEqual(result, 650.95M);
        }

        [TestMethod]
        public void CanCallFormatAsGbpCurrency()
        {
            // Act
            var result = CurrencyConverter.FormatAsGbpCurrency("650.95");

            // Assert
            Assert.AreEqual(result, "£650.95");
        }
    }
}