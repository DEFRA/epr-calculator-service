using System;
using System.Globalization;

namespace EPR.Calculator.Service.Common.Utils
{
    public static class CurrencyConverter
    {
        /// <summary>
        /// Safely parses a string to decimal using invariant culture.
        /// Returns 0 if parsing fails.
        /// </summary>
        public static decimal GetDecimalValue(string value)
        {
            var isParseSuccessful = decimal.TryParse(value, CultureInfo.InvariantCulture, out decimal result);
            return isParseSuccessful ? result : 0;
        }

        /// <summary>
        /// Converts a string input to GBP currency format (e.g., £12.3456) if valid.
        /// Returns an empty string if input is not valid.
        /// </summary>
        public static string ConvertToCurrency(string value)
        {
            var decimalValue = GetDecimalValue(value);
            return FormatCurrencyWithGbpSymbol(decimalValue);
        }

        /// <summary>
        /// Converts a decimal input to GBP currency format (e.g., £12.3456) if valid.
        /// Returns an empty string if input is not valid.
        /// </summary>
        public static string ConvertToCurrency(decimal detail)
        {
            if (detail == 0)
            {
                return string.Empty;
            }

            return FormatCurrencyWithGbpSymbol(detail);
        }

        /// <summary>
        /// Formats decimal to gbp currency.
        /// </summary>
        /// <param name="decimalValue"></param>
        /// <returns>gbp currency.</returns>
        public static string FormatCurrencyWithGbpSymbol(decimal decimalValue)
        {
            var culture = CultureInfo.CreateSpecificCulture("en-GB");
            culture.NumberFormat.CurrencySymbol = "£";
            culture.NumberFormat.CurrencyPositivePattern = 0;
            culture.NumberFormat.CurrencyGroupSeparator = string.Empty;
            return decimalValue.ToString("C", culture);
        }

    }
}