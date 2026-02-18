namespace EPR.Calculator.Service.Common
{
    using EPR.Calculator.API.Data.Models;
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
        /// Gets or sets the relative year for the calculator run.
        /// </summary>
        required public RelativeYear RelativeYear { get; set; }

        /// <summary>
        /// Gets or sets the message type for calculator.
        /// </summary>
        required public string MessageType { get; set; }
    }
}
