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
            var result = CurrencyConverterUtils.GetDecimalValue("650.95");

            // Assert
            Assert.AreEqual(650.95M, result);
        }

        [TestMethod]
        public void CanCallFormatAsGbpCurrency()
        {
            // Act
            var result = CurrencyConverterUtils.ConvertToCurrency("650.95");

            // Assert
            Assert.AreEqual("£650.95", result);
        }

        [TestMethod]
        public void CanCallFormatAsGbpCurrencyForZeroCurrency()
        {
            // Act
            var result = CurrencyConverterUtils.ConvertToCurrency(0, 2);

            // Assert
            Assert.AreEqual("£0.00", result);
        }

        [TestMethod]
        public void CanCallFormatAsGbpCurrencyWithRounding()
        {
            // Act
            var result = CurrencyConverterUtils.ConvertToCurrency(10.578m, 2);

            // Assert
            Assert.AreEqual("£10.58", result);
        }

        [TestMethod]
        public void CanCallFormatAsGbpCurrencyWithRoundingFourDecimalPlaces()
        {
            // Act
            var result = CurrencyConverterUtils.ConvertToCurrency(10.53324785678m, 4);

            // Assert
            Assert.AreEqual("£10.5332", result);
        }
    }
}