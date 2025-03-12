namespace EPR.Calculator.Service.Common.Utils
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// A util class providing various helper methods.
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// Extracts the first year from a financial year string in the format "YYYY-YY".
        /// </summary>
        /// <param name="value">The financial year string to parse, in the format "YYYY-YY".</param>
        /// <returns>The first year as a string.</returns>
        /// <exception cref="ArgumentException">Thrown when the financial year string is null or empty.</exception>
        /// <exception cref="FormatException">Thrown when the format is invalid.</exception>
        public static string GetStartYearFromFinancialYear(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Financial year cannot be null or empty", nameof(value));
            }

            string pattern = @"^\d{4}-\d{2}$";
            TimeSpan regexTimeout = TimeSpan.FromSeconds(1);

            if (!Regex.IsMatch(value, pattern, RegexOptions.None, regexTimeout))
            {
                throw new FormatException("Financial year format is invalid. Expected format is 'YYYY-YY'.");
            }

            // Since the regex ensures the format, we can safely split and return the first part
            var years = value.Split('-');
            return years[0];
        }

        /// <summary>
        /// Converts a financial year string to the previous calendar year as a string.
        /// </summary>
        /// <param name="financialYear">The financial year string to convert, in the format "YYYY-YY".</param>
        /// <returns>The previous calendar year as a string.</returns>
        /// <exception cref="ArgumentException">Thrown when the financial year string is null or empty.</exception>
        public static CalendarYear GetCalendarYearFromFinancialYear(FinancialYear financialYear)
        {
            int year = int.Parse(GetStartYearFromFinancialYear(financialYear.ToString()));
            return (year - 1).ToString();
        }

        public static FormattableString GetFormattedSqlString(string procedureName, int runId, string calendarYear, string createdBy)
        {
            return $"exec {procedureName} @RunId ={runId}, @calendarYear = {calendarYear}, @createdBy = {createdBy}";
        }
    }
}