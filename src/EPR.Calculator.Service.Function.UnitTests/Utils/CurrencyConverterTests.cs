namespace EPR.Calculator.Service.Function.UnitTests
{
    using System;
    using AutoFixture;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using EPR.Calculator.Service.Common.Utils;

    [TestClass]
    public class CurrencyConverterTests
    {
        [TestMethod]
        public void CanCallGetDecimalValue()
        {
            // Act
            var result = CurrencyConverter.GetDecimalValue("650.95");

            // Assert
            Assert.AreEqual(650.95M, result);
        }

        [TestMethod]
        public void CanCallFormatAsGbpCurrency()
        {
            // Act
            var result = CurrencyConverter.ConvertToCurrency("650.95");

            // Assert
            Assert.AreEqual("£650.95", result);
        }
    }
}