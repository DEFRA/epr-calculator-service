namespace EPR.Calculator.Service.Common
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// Represents a calendar year, with the format yyyy.
    /// </summary>
    public partial record struct CalendarYear
    {
        [GeneratedRegex("^[0-9]{4}$", RegexOptions.IgnoreCase, "en-GB")]
        private static partial Regex CalendarYearRegex();

        /// <summary>
        /// Initializes a new instance of the <see cref="CalendarYear"/> class.
        /// </summary>
        /// <param name="value">The year in yyyy format.</param>
        /// <remarks>
        /// Use this and <see cref="FinancialYear"/> in place of strings in places where it's helpfull to avoid
        /// confusion between date types.
        /// </remarks>
        public CalendarYear(string value)
        {
            if (!CalendarYearRegex().IsMatch(value))
            {
                throw new System.ArgumentException("The year must be in the format yyyy.");
            }

            this.Value = value;
        }

        private string Value { get; init; }

        public static implicit operator CalendarYear(string value) => new CalendarYear(value);

        public static implicit operator string(CalendarYear value) => value.ToString();

        /// <inheritdoc/>
        public override string ToString() => this.Value;
    }
}
