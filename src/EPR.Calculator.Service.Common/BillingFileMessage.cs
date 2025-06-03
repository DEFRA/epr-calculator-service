namespace EPR.Calculator.Service.Common
{
    /// <summary>
    /// Billing file message model class.
    /// </summary>
    public class BillingFileMessage
    {
        /// <summary>
        /// Gets or sets the identifier for the calculator run.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the user who approved billing file.
        /// </summary>
        required public string ApprovedBy { get; set; }

        /// <summary>
        /// Gets or sets the message type for calculator.
        /// </summary>
        required public string MessageType { get; set; }
    }
}
