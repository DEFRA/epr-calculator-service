namespace EPR.Calculator.Service.Common.UnitTests.Utils
{
    using System;
    using System.Globalization;
    using AutoFixture;
    using EPR.Calculator.Service.Common.Utils;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CurrencyUtilTests
    {
        [TestMethod]
        public void CanCallConvertToCurrency()
        {
            // Arrange
            var fixture = new Fixture();
            var detail = 100.00m;

            // Act
            var result = CurrencyConverterUtils.ConvertToCurrency(detail);
            bool iscurrency = decimal.TryParse(result, NumberStyles.Currency, new CultureInfo("en-GB"), out _);

            // Assert
            Assert.IsTrue(iscurrency);
        }
    }
}