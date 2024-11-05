﻿namespace EPR.Calculator.Service.Common.AzureSynapse
{
    using System.Globalization;

    public record FinancialYear(DateTime value)
    {
        /// <summary>
        /// Gets the financial Year.
        /// </summary>
        public DateTime Value { get; } = value;

        /// <summary>
        /// Converts the financial year to the previous calendar year.
        /// </summary>
        /// <returns>The previous calendar year as a <see cref="DateTime"/>.</returns>
        public DateTime ToCalendarYear() => new (this.value.Year - 1, 1, 1);

        /// <summary>
        /// Parses a financial year from a string.
        /// </summary>
        /// <param name="value">The year string to parse, in the format "YYYY-YY".</param>
        /// <returns>A <see cref="FinancialYear"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when the format is invalid.</exception>
        public static FinancialYear Parse(string value)
        {
            try
            {
                return new FinancialYear(
                    DateTime.ParseExact(value.Substring(0, 4), "yyyy", CultureInfo.InvariantCulture));
            }
            catch (FormatException)
            {
                throw new ArgumentException("Invalid format. Please use the format 'YYYY-YY'.");
            }
        }

        /// <summary>
        /// Extracts the first year from a financial year string in the format "YYYY-YY".
        /// </summary>
        /// <param name="value">The financial year string to parse, in the format "YYYY-YY".</param>
        /// <returns>The first year as a string.</returns>
        /// <exception cref="FormatException">Thrown when the format is invalid.</exception>
        public static string FinancialYearAsString(string value)
        {
            var years = value.Split('-');
            if (years.Length != 2 || !int.TryParse(years[0], out int year) || years[0].Length != 4 || years[1].Length != 2)
            {
                throw new FormatException("Financial year format is invalid. Expected format is 'YYYY-YY'.");
            }

            return years[0];
        }

        /// <summary>
        /// Converts a financial year string to the previous calendar year as a string.
        /// </summary>
        /// <param name="value">The financial year string to convert, in the format "YYYY-YY".</param>
        /// <returns>The previous calendar year as a string.</returns>
        /// <exception cref="ArgumentException">Thrown when the financial year string is null or empty.</exception>
        public static string ToCalendarYearAsString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Financial year cannot be null or empty", nameof(value));
            }

            int year = int.Parse(FinancialYearAsString(value));
            return (year - 1).ToString();
        }
    }
}