namespace EPR.Calculator.Service.Function.UnitTests
{
    using EPR.Calculator.API.Utils;
    using EPR.Calculator.Service.Function.Enums;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CsvSanitiserTests
    {
        [TestMethod]
        public void ShouldSanitiseDataWithDelimiter()
        {
            // Arrange
            var data = "Some data\n\t";

            // Act
            var result = CsvSanitiser.SanitiseData(data);

            // Assert
            Assert.AreEqual("\"Some data\",", result);
        }

        [TestMethod]
        public void ShouldSanitiseDataWithoutDemiliter()
        {
            // Arrange
            var data = "Some,\t data";

            // Act
            var result = CsvSanitiser.SanitiseData(data, false);

            // Assert
            Assert.AreEqual("\"Some data\"", result);
        }

        [TestMethod]
        public void ShouldReturnCommaIfNoDataWithDelimitedEnabled()
        {
            // Arrange
            var data = string.Empty;

            // Act
            var result = CsvSanitiser.SanitiseData(data);

            // Assert
            Assert.AreEqual("\"\",", result);
        }

        [TestMethod]
        public void ShouldReturnEmptyStringIfNoDataWithoutDelimiter()
        {
            // Arrange
            var data = string.Empty;

            // Act
            var result = CsvSanitiser.SanitiseData(data, false);

            // Assert
            Assert.AreEqual("\"\"", result);
        }

        [TestMethod]
        public void ShouldSanitiseCurrencyDataWithDelimiter()
        {
            // Arrange
            var data = 100.5987m;
            var decimalPlaces = DecimalPlaces.Two;
            var isCurrency = true;

            // Act
            var result = CsvSanitiser.SanitiseData(data, decimalPlaces, null, isCurrency);

            // Assert
            Assert.AreEqual("\"£100.60\",", result);
        }

        [TestMethod]
        public void ShouldSanitiseCurrencyDataWithoutDelimiter()
        {
            // Arrange
            var data = 290.5987432m;
            var decimalPlaces = DecimalPlaces.Three;
            var isCurrency = true;
            var isPercentage = false;

            // Act
            var result = CsvSanitiser.SanitiseData(data, decimalPlaces, null, isCurrency, isPercentage, false);

            // Assert
            Assert.AreEqual("\"£290.599\"", result);
        }

        [TestMethod]
        public void ShouldSanitisePercentageDataWithDelimiter()
        {
            // Arrange
            var data = 79.798m;
            var decimalPlaces = DecimalPlaces.Two;
            var isCurrency = false;
            var isPercentage = true;

            // Act
            var result = CsvSanitiser.SanitiseData(data, decimalPlaces, null, isCurrency, isPercentage);

            // Assert
            Assert.AreEqual("\"79.80%\",", result);
        }

        [TestMethod]
        public void ShouldSanitisePercentageDataWithoutDelimiter()
        {
            // Arrange
            var data = 83.456m;
            var decimalPlaces = DecimalPlaces.Two;
            var isCurrency = false;
            var isPercentage = true;

            // Act
            var result = CsvSanitiser.SanitiseData(data, decimalPlaces, null, isCurrency, isPercentage, false);

            // Assert
            Assert.AreEqual("\"83.46%\"", result);
        }
    }
}