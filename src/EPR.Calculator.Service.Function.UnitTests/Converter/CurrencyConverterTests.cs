namespace EPR.Calculator.Service.Function.UnitTests.Converter
{
    using AutoFixture;
    using EPR.Calculator.Service.Function.Converter;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Text;
    using System.Text.Encodings.Web;
    using System.Text.Json;
    using System.Text.Unicode;

    [TestClass]
    public class CurrencyConverterTests
    {
        private CurrencyConverter TestClass { get; init; }

        private IFixture Fixture { get; init; }

        private JsonSerializerOptions SerialiserOptions { get; init; }

        public CurrencyConverterTests()
        {
            this.Fixture = new Fixture();
            this.SerialiserOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            };
            this.TestClass = new CurrencyConverter();
        }

        [TestMethod]
        public void Read_ReturnsValueWhenJsonIsValid()
        {
            // Arrange
            var expectedValue = Fixture.Create<decimal>();
            var testJson = $"{{\"testValue\":\"£{expectedValue}\"}}";
            var reader = BuildReader(testJson);

            // Act
            var result = this.TestClass.Read(ref reader, typeof(decimal), this.SerialiserOptions);

            // Assert
            Assert.AreEqual(expectedValue, result);
        }

        [TestMethod]
        [DataRow("\"\"")] // Empty string
        [DataRow("10")] // Non-string value
        [DataRow("\"10\"")] // String without currency symbol
        [DataRow("\"$10\"")] // Wrong currency symbol
        public void Read_ThrowsExceptionWhenValueIsNotValidCurrency(object value)
        {
            // Arrange
            var testJson = $"{{\"testValue\":{value}}}";
            var reader = BuildReader(testJson);
            Exception? result = null;

            // Act
            try
            {
                this.TestClass.Read(ref reader, typeof(decimal), this.SerialiserOptions);
            }
            catch (Exception ex)
            {
                result = ex;
            }

            // Assert
            Assert.IsInstanceOfType<JsonException>(result, "Expected a JsonException to be thrown.");
        }


        [TestMethod]
        public void CanCallWrite()
        {
            // Arrange
            var stream = new MemoryStream();
            var writer = new Utf8JsonWriter(stream, new JsonWriterOptions 
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            });
            var value = Fixture.Create<decimal>();

            // Act
            this.TestClass.Write(writer, value, this.SerialiserOptions);
            writer.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            var result = new StreamReader(stream).ReadToEnd();

            // Assert
            Assert.AreEqual($"\"{value.ToString("C")}\"", result);
        }

        private Utf8JsonReader BuildReader(string value)
        {
            var valueAsBytes = Encoding.UTF8.GetBytes(value);
            var reader = new Utf8JsonReader(valueAsBytes);

            // Fast forward the reader to the value.
            reader.Read();
            reader.Read();
            reader.Read();

            return reader;
        }
    }
}