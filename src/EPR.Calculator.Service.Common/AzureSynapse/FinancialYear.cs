namespace EPR.Calculator.Service.Common.AzureSynapse
{
    using System.Globalization;

    public record FinancialYear(DateTime value)
    {
        /// <summary>
        /// Gets the financial Year.
        /// </summary>
        public DateTime Value { get; } = value;

        /// <summary>
        /// Converts the financial year to the calendar year.
        /// </summary>
        /// <returns>The calendar year.</returns>
        // TODO: replace this with the actual conversion between financial and calendar year,
        // once we know the rules for it.
        public DateTime ToCalendarYear() => this.value;

        /// <summary>
        /// Parses a financial year from a string.
        /// </summary>
        /// <param name="value">The year string to parse, in the format yyyy-MM.</param>
        /// <returns>A <see cref="FinancialYear"/>.</returns>
        public static FinancialYear Parse(string value)
        => new FinancialYear(
            DateTime.ParseExact(value.Substring(0, 4), "yyyy", CultureInfo.InvariantCulture));
    }
}
