namespace EPR.Calculator.Service.Function
{
    /// <summary>
    /// Calculator Run Parameter
    /// </summary>
    public class CreateBillingFileMessage : MessageBase
    {
        /// <summary>
        /// Gets or sets the user who approved the billing file.
        /// </summary>
        public required string ApprovedBy { get; set; }
    }
}