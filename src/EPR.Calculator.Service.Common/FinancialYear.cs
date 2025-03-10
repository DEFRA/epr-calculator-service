namespace EPR.Calculator.Service.Common
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// Represents a financial year, with the format yyyy-yy.
    /// </summary>
    public record struct FinancialYear
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialYear"/> class.
        /// </summary>
        /// <param name="value">The year in yyyy format.</param>
        /// <remarks>
        /// Use this and <see cref="CalendarYear"/> in place of strings in places where it's helpfull to avoid
        /// confusion between date types.
        /// </remarks>
        public FinancialYear(string value)
        {
            if (!Regex.IsMatch(value, "^[0-9]{4}-[0-9]{2}$", default, TimeSpan.FromSeconds(1)))
            {
                throw new System.ArgumentException("The year must be in the format yyyy-yy.");
            }

            this.Value = value;
        }

        private string Value { get; init; }

        public static implicit operator FinancialYear(string value) => new FinancialYear(value);

        /// <inheritdoc/>
        public override string ToString() => this.Value;
    }
}
