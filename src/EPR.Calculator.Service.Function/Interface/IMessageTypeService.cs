namespace EPR.Calculator.Service.Function.Interface
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
}
