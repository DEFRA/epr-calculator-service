using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EPR.Calculator.Service.Function.Messaging;

/// <summary>
///     Defines a service for deserializing JSON strings into specific message types.
/// </summary>
public interface IMessageTypeService
{
    /// <summary>
    ///     Deserializes the specified JSON string into a concrete <see cref="MessageBase" /> derived type
    ///     based on the contained message type information.
    /// </summary>
    MessageBase DeserializeMessage(string json);
}

/// <inheritdoc />
public class MessageTypeService : IMessageTypeService
{
    private readonly Dictionary<string, Type> _typeMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Billing", typeof(CreateBillingFileMessage) },
        { "Result", typeof(CreateResultFileMessage) }
    };

    /// <inheritdoc />
    public MessageBase DeserializeMessage(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        var jObject = JObject.Parse(json);
        var messageType = jObject["MessageType"]?.ToString() ?? "";

        if (!_typeMappings.TryGetValue(messageType, out var targetType))
        {
            throw new NotSupportedException($"Unsupported MessageType: '{messageType}'");
        }

        return (MessageBase)JsonConvert.DeserializeObject(json, targetType)!;
    }
}