namespace EPR.Calculator.API.Utils
{
    using System;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Enums;
    using Newtonsoft.Json;

    public static class CsvSanitiser
    {
        public static string SanitiseData<T>(T value, bool delimiterRequired = true)
        {
            if (value == null)
            {
                return delimiterRequired
                    ? CommonConstants.CsvFileDelimiter
                    : string.Empty;
            }

            // If the value is a string, use it directly; otherwise, serialize the object to JSON.
            var stringToSanitise = value is string
                ? value.ToString()
                : JsonConvert.SerializeObject(value);

            // Remove newline, carriage returns, and commas, then trim
            stringToSanitise = stringToSanitise?.Replace(Environment.NewLine, string.Empty)
                                   .Replace(CommonConstants.TabSpace, string.Empty)
                                   .Replace(CommonConstants.CsvFileDelimiter, string.Empty)
                                   .Trim() ?? string.Empty;

            return delimiterRequired
                ? $"{stringToSanitise},"
                : stringToSanitise;
        }

        public static string SanitiseData(
            decimal value,
            DecimalPlaces? roundTo,
            DecimalFormats? valueFormat,
            bool isCurrency = false,
            bool isPercentage = false,
            bool delimiterRequired = true)
        {
            var roundedValue = roundTo == null
                ? value
                : Math.Round(value, (int)roundTo);

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
