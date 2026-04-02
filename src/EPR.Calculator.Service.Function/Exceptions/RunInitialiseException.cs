namespace EPR.Calculator.Service.Function.Exceptions
{
    /// <summary>
    ///     Thrown when a run cannot be initialised from a Service Bus message.
    /// </summary>
    public class RunInitialiseException : Exception
    {
        public RunInitialiseException(string serviceBusMessage, Exception innerException)
            : base($"Unable to initialise run from message: {serviceBusMessage}", innerException)
        {
            ServiceBusMessage = serviceBusMessage;
        }

        /// <summary>
        ///     The raw Service Bus message that could not be used to initialise the run.
        /// </summary>
        public string ServiceBusMessage { get; }
    }
}
