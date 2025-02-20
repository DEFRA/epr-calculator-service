using Newtonsoft.Json;
using System;

namespace EPR.Calculator.API.Utils
{
    public static class CsvSanitiser
    {
        public static string SanitiseData<T>(T value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            // If the value is a string, use it directly; otherwise, serialize the object to JSON.
            var stringToSanitise = value is string
                ? value.ToString()
                : JsonConvert.SerializeObject(value);

            // Remove newline, carriage returns, and commas, then trim
            stringToSanitise = stringToSanitise?.Replace(Environment.NewLine, string.Empty)
                                   .Replace("\t", string.Empty)
                                   .Replace(",", string.Empty)
                                   .Trim();

            return stringToSanitise ?? string.Empty;
        }

        public static string SanitiseData(string value, bool delimitedRequired = true)
        {
            return delimitedRequired
                ? $"{SanitiseData(value)},"
                : SanitiseData(value);
        }

        public static string SanitiseData(decimal value, int roundTo, string? valueFormat, bool isCurrency = false, bool delimitedRequired = true)
        {
            var formattedValue = valueFormat == null
                ? Math.Round(value, roundTo).ToString()
                : Math.Round(value, roundTo).ToString(valueFormat);

            if (isCurrency)
            {
                formattedValue = $"£{formattedValue}";
            }

            return delimitedRequired
                ? $"{SanitiseData(formattedValue)},"
                : SanitiseData(formattedValue);
        }
    }
}
