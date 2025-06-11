namespace EPR.Calculator.Service.Common
{
    /// <summary>
    /// CalculatorRunParameter model class.
    /// </summary>
    public class CalculatorRunParameter
    {
        /// <summary>
        /// Gets or sets the identifier for the calculator run.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the user who initiated the calculator run.
        /// </summary>
        required public string User { get; set; }

        /// <summary>
        /// Gets or sets the financial year for the calculator run.
        /// </summary>
        required public FinancialYear FinancialYear { get; set; }

        /// <summary>
        /// Gets or sets the message type for calculator.
        /// </summary>
        required public string MessageType { get; set; }
    }
}
