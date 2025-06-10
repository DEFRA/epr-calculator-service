using EPR.Calculator.Service.Function.Services;
using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    [TestClass]
    public class MessageTypeServiceTests
    {
        private MessageTypeService _service;

        [TestInitialize]
        public void Setup()
        {
            _service = new MessageTypeService();
        }

        [TestMethod]
        public void DeserializeMessage_ValidBillingMessage_ReturnsCreateBillingFileMessage()
        {
            // Arrange
            var json = @"{
                            'MessageType': 'Billing',
                            'CalculatorRunId': 123,
                            'ApprovedBy': 'Test User'
                }";

            // Act
            var result = _service.DeserializeMessage(json);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(CreateBillingFileMessage));

            var billingMessage = (CreateBillingFileMessage)result;
            Assert.AreEqual(123, billingMessage.CalculatorRunId);
            Assert.AreEqual("Billing", billingMessage.MessageType);
        }

        [TestMethod]
        public void DeserializeMessage_MissingMessageType_ReturnsCreateResultFileMessage()
        {
            // Arrange
            var json = @"{
                            'CalculatorRunId': 123,
                            'CreatedBy': 'Test User',
                            'FinancialYear': '2024-25',
                            'MessageType': 'Result',
                         }";

            // Act
            var result = _service.DeserializeMessage(json);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(CreateResultFileMessage));

            var billingMessage = (CreateResultFileMessage)result;
            Assert.AreEqual(123, billingMessage.CalculatorRunId);
            Assert.AreEqual("Test User", billingMessage.CreatedBy);
            Assert.AreEqual("2024-25", billingMessage.FinancialYear);
            Assert.AreEqual("Result", billingMessage.MessageType);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void DeserializeMessage_UnsupportedMessageType_ThrowsNotSupportedException()
        {
            // Arrange
            var json = @"{
                             'MessageType': 'UnknownType',
                             'Data': 'something'
                         }";

            // Act
            _service.DeserializeMessage(json);
        }

        [TestMethod]
        [ExpectedException(typeof(JsonException))]
        public void DeserializeMessage_NullOrEmpty_ThrowsJsonException()
        {
            // Act
            _service.DeserializeMessage(null);
        }

        [TestMethod]
        [ExpectedException(typeof(JsonReaderException))]
        public void DeserializeMessage_InvalidJson_ThrowsJsonException()
        {
            // Arrange
            var json = "invalid json";

            // Act
            _service.DeserializeMessage(json);
        }
    }
}
