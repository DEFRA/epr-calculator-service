namespace EPR.Calculator.Service.Common.UnitTests.Utils
{
    using System;
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
            var detail = fixture.Create<decimal>();

            // Act
            var result = CurrencyUtil.ConvertToCurrency(detail);

            // Assert
            Assert.IsTrue(result.Contains("£"));
        }
    }
}