namespace EPR.Calculator.API.Utils
{
    using System;
    using System.Globalization;
    using System.Text.Json;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Enums;

    public static class CsvSanitiser
    {
        public static string SanitiseData<T>(T value, bool csvDelimiterRequired = true)
        {
            if (value == null)
            {
                return csvDelimiterRequired
                    ? CommonConstants.CsvFileDelimiter
                    : string.Empty;
            }

            // If the value is a string, use it directly; otherwise, serialize the object to JSON.
            var stringToSanitise = value is string
                ? value.ToString()
                : JsonSerializer.Serialize(value);

            // Remove newline, carriage returns, and commas, then trim
            stringToSanitise = stringToSanitise?.Replace(Environment.NewLine, string.Empty)
                                   .Replace(CommonConstants.TabSpace, string.Empty)
                                   .Replace(CommonConstants.CsvFileDelimiter, string.Empty)
                                   .Trim() ?? string.Empty;

            // Apply the speech marks to handle the comma in the text and currency values
            stringToSanitise = $"{CommonConstants.DoubleQuote}{stringToSanitise}{CommonConstants.DoubleQuote}";

            return csvDelimiterRequired
                ? $"{stringToSanitise}{CommonConstants.CsvFileDelimiter}"
                : stringToSanitise;
        }

        public static string SanitiseData<T>(
            T value,
            DecimalPlaces? roundTo,
            DecimalFormats? valueFormat,
            bool isCurrency = false,
            bool isPercentage = false,
            bool delimiterRequired = true)
        {
            decimal decimalValue;
            if (value is string)
            {
                var isParseSuccessful = decimal.TryParse(value.ToString(), CultureInfo.InvariantCulture, out decimal result);
                if (!isParseSuccessful)
                {
                    return SanitiseData(value, delimiterRequired);
                }

                decimalValue = result;
            }
            else
            {
                decimalValue = Convert.ToDecimal(value);
            }

            var roundedValue = roundTo == null
                ? decimalValue
                : Math.Round(decimalValue, (int)roundTo);

            var formattedValue = valueFormat == null
                ? roundedValue.ToString()
                : roundedValue.ToString(valueFormat.ToString());

            if (isCurrency)
            {
                formattedValue = $"£{formattedValue}";
            }

            if (isPercentage)
            {
                formattedValue = $"{formattedValue}%";
            }

            return SanitiseData(formattedValue, delimiterRequired);
        }
    }
}
