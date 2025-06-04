using EPR.Calculator.Service.Function.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                            'RunId': 123,
                            'ApprovedBy': 'Test User'
                }";

            // Act
            var result = _service.DeserializeMessage(json);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(CreateBillingFileMessage));

            var billingMessage = (CreateBillingFileMessage)result;
            Assert.AreEqual(123, billingMessage.RunId);
            Assert.AreEqual("Billing", billingMessage.MessageType);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DeserializeMessage_MissingMessageType_ThrowsArgumentException()
        {
            // Arrange
            var json = @"{
                            'RunId': 123
                         }";

            // Act
            _service.DeserializeMessage(json);
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
