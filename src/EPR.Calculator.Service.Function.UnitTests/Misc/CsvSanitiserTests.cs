namespace EPR.Calculator.Service.Function.UnitTests
{
    using System;
    using AutoFixture;
    using EPR.Calculator.API.Utils;
    using EPR.Calculator.Service.Function.Enums;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using T = System.String;

    [TestClass]
    public class CsvSanitiserTests
    {
        [TestMethod]
        public void CanCallSanitiseDataWithValueAndDelimitedRequired()
        {
            // Arrange
            var fixture = new Fixture();
            var value = fixture.Create<T>();
            var delimitedRequired = fixture.Create<bool>();

            // Act
            var result = CsvSanitiser.SanitiseData<T>(value, delimitedRequired);

            // Assert
            Assert.Fail("Create or modify test");
        }

        [TestMethod]
        public void CanCallSanitiseDataWithValueAndRoundToAndValueFormatAndIsCurrencyAndIsPercentageAndDelimitedRequired()
        {
            // Arrange
            var fixture = new Fixture();
            var value = fixture.Create<decimal>();
            var roundTo = fixture.Create<DecimalPlaces?>();
            var valueFormat = fixture.Create<DecimalFormats?>();
            var isCurrency = fixture.Create<bool>();
            var isPercentage = fixture.Create<bool>();
            var delimitedRequired = fixture.Create<bool>();

            // Act
            var result = CsvSanitiser.SanitiseData(value, roundTo, valueFormat, isCurrency, isPercentage, delimitedRequired);

            // Assert
            Assert.Fail("Create or modify test");
        }
    }
}