namespace EPR.Calculator.Service.Function.Messaging
{
    /// <summary>
    /// Represents the base class for all message types.
    /// </summary>
    public abstract class MessageBase
    {
        public const string Billing = "Billing";
        public const string Result = "Result";

        /// <summary>
        /// Gets or sets the type identifier of the message.
        /// This value is typically used to determine the specific message subclass during deserialization.
        /// </summary>
        required public string MessageType { get; set; }

        /// <summary>
        /// Gets or sets the identifier for the calculator run.
        /// </summary>
        public int CalculatorRunId { get; set; }
    }
}
