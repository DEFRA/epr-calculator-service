namespace EPR.Calculator.Service.Function.Services
{
    using System;
    using System.Collections.Generic;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Interface;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

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
            var jObject = JObject.Parse(json);

            var messageType = jObject["MessageType"]?.ToString();
            if (string.IsNullOrWhiteSpace(messageType))
                throw new ArgumentException("MessageType not found in the message.");

            if (!_typeMappings.TryGetValue(messageType, out var targetType))
                throw new NotSupportedException($"Unsupported MessageType: {messageType}");

            var result = JsonConvert.DeserializeObject(json, targetType);

            if (result == null)
                throw new JsonException("Deserialized object is null");

            if (result is not MessageBase message)
                throw new InvalidCastException($"Deserialized object is not of type IMessage.");

            return message;
        }
    }
}