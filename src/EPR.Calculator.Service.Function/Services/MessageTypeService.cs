using EPR.Calculator.Service.Function.Constants;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EPR.Calculator.Service.Function.Services
{
    /// <summary>
    /// Defines a service for deserializing JSON strings into specific message types.
    /// </summary>
    public interface IMessageTypeService
    {
        /// <summary>
        /// Deserializes the specified JSON string into a concrete <see cref="MessageBase"/> derived type
        /// based on the contained message type information.
        /// </summary>
        MessageBase DeserializeMessage(string json);
    }

    /// <summary>
    /// Service for handling Mmssage type service operations.
    /// </summary>
    public class MessageTypeService : IMessageTypeService
    {
        private readonly Dictionary<string, Type> _typeMappings = new()
        {
            { MessageTypes.Billing, typeof(CreateBillingFileMessage) },
            { MessageTypes.Result, typeof(CreateResultFileMessage) }
        };

        public MessageBase DeserializeMessage(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new JsonException("Input JSON is null or empty.");

            var jObject = JObject.Parse(json);
            var messageType = jObject["MessageType"]?.ToString();

            if (string.IsNullOrWhiteSpace(messageType))
                throw new ArgumentException("MessageType not found in the message.");

            if (!_typeMappings.TryGetValue(messageType, out var targetType))
                throw new NotSupportedException($"Unsupported MessageType: {messageType}");

            var result = JsonConvert.DeserializeObject(json, targetType);

            if (result is not MessageBase message)
                throw new InvalidCastException($"Deserialized object is not of type {nameof(MessageBase)}.");

            return message;
        }
    }
}
