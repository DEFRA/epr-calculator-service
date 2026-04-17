using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using EPR.Calculator.Service.Function.Converters;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.Service.Function.UnitTests.Converter;

[TestClass]
public class CurrencyConverterTests
{
    public CurrencyConverterTests()
    {
        SerialiserOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };
        TestClass = new CurrencyConverter();
    }

    private CurrencyConverter TestClass { get; }

    private JsonSerializerOptions SerialiserOptions { get; }

    [TestMethod]
    public void Read_ReturnsValueWhenJsonIsValid()
    {
        // Arrange
        var expectedValue = TestFixtures.Default.Create<decimal>();
        var testJson = $"{{\"testValue\":\"£{expectedValue}\"}}";
        var reader = BuildReader(testJson);

        // Act
        var result = TestClass.Read(ref reader, typeof(decimal), SerialiserOptions);

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
            TestClass.Read(ref reader, typeof(decimal), SerialiserOptions);
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
        var value = TestFixtures.Default.Create<decimal>();

        // Act
        TestClass.Write(writer, value, SerialiserOptions);
        writer.Flush();
        stream.Seek(0, SeekOrigin.Begin);
        var result = new StreamReader(stream).ReadToEnd();

        // Assert
        Assert.AreEqual($"\"{value.ToString("C", CultureInfo.GetCultureInfo("en-GB"))}\"", result);
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