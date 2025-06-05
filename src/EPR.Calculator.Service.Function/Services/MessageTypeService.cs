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
            if (string.IsNullOrWhiteSpace(json))
                throw new JsonException("Input JSON is null or empty.");

            var jObject = JObject.Parse(json);
            var messageType = jObject["MessageType"]?.ToString();

            var targetType = messageType != null ? _typeMappings.GetValueOrDefault(messageType) : null;

            if (messageType != null && targetType == null)
                throw new NotSupportedException($"Unsupported MessageType: {messageType}");

            var typeToDeserialize = targetType ?? typeof(CreateResultFileMessage);

            var result = JsonConvert.DeserializeObject(json, typeToDeserialize);

            if (result is not MessageBase message)
                throw new InvalidCastException($"Deserialized object is not of type {nameof(MessageBase)}.");

            return message;
        }
    }
}