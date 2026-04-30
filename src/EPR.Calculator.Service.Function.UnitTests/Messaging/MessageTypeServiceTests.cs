using EPR.Calculator.Service.Function.Messaging;
using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.UnitTests.Messaging;

[TestClass]
public class MessageTypeServiceTests
{
    private MessageTypeService _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _sut = new MessageTypeService();
    }

    [DataRow("Result")]
    [DataRow("result")]
    [DataRow("RESULT")]
    [TestMethod]
    public void Should_deserialize_CreateResultFileMessage(string messageType)
    {
        // Arrange
        var json = $$"""
        {
            "MessageType" : "{{messageType}}",
            "CalculatorRunId" : 123,
            "CreatedBy" : "Test User",
            "RelativeYear" : 2024
        }
        """;

        // Act
        var result = _sut.DeserializeMessage(json);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<CreateResultFileMessage>();

        var resultMessage = (CreateResultFileMessage)result;
        resultMessage.CalculatorRunId.ShouldBe(123);
        resultMessage.CreatedBy.ShouldBe("Test User");
    }

    [DataRow("Billing")]
    [DataRow("billing")]
    [DataRow("BILLING")]
    [TestMethod]
    public void Should_deserialize_CreateBillingFileMessage(string messageType)
    {
        // Arrange
        var json = $$"""
         {
             "MessageType" : "{{messageType}}",
             "CalculatorRunId" : 123,
             "ApprovedBy" : "Test User"
         }
         """;

        // Act
        var result = _sut.DeserializeMessage(json);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<CreateBillingFileMessage>();

        var billingMessage = (CreateBillingFileMessage)result;
        billingMessage.CalculatorRunId.ShouldBe(123);
    }

    [TestMethod]
    public void Should_throw_no_MessageType()
    {
        // Arrange
        string json = "{}";

        // Act & Assert
        Should.Throw<NotSupportedException>(() => _sut.DeserializeMessage(json));
    }

    [DataRow("\"unknown\"")]
    [DataRow("\"\"")]
    [DataRow("null")]
    [TestMethod]
    public void Should_throw_unknown_MessageType(string messageType)
    {
        // Arrange
        var json = $$"""
        {
            "MessageType": {{messageType}}
        }
        """;

        // Act & Assert
        Should.Throw<NotSupportedException>(() => _sut.DeserializeMessage(json));
    }

    [DataRow("invalid json")]
    [DataRow("")]
    [DataRow("[]")]
    [TestMethod]
    public void Should_throw_invalid_json(string? json)
    {
        // Act & Assert
        Should.Throw<JsonReaderException>(() => _sut.DeserializeMessage(json!));
    }

    [TestMethod]
    public void Should_throw_null_json()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => _sut.DeserializeMessage(null!));
    }
}