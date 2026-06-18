using System.Globalization;
using System.Text.Json;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.Misc
{
    public static class CsvSanitiser
    {
        public static string SanitiseData<T>(T value, bool delimiterRequired = true, bool appendLrmCharacterToPreventRenderedAsFormula = false)
        {
            if (value is null)
            {
                return delimiterRequired
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

            if (appendLrmCharacterToPreventRenderedAsFormula &&
                stringToSanitise.Length > 0 &&
                stringToSanitise != CommonConstants.Hyphen)
            {
                stringToSanitise = "\u200E" + stringToSanitise;
            }

            // Apply the speech marks to handle the comma in the text and currency values
            stringToSanitise = $"{CommonConstants.DoubleQuote}{stringToSanitise}{CommonConstants.DoubleQuote}";

            return delimiterRequired
                ? $"{stringToSanitise}{CommonConstants.CsvFileDelimiter}"
                : stringToSanitise;
        }

        public static string SanitiseData(
            decimal? value,
            DecimalPlaces? roundTo,
            DecimalFormats? valueFormat,
            bool isCurrency = false,
            bool isPercentage = false,
            bool delimiterRequired = true,
            bool canBeEmpty = false)
        {
            if (canBeEmpty && value is null)
                return delimiterRequired
                    ? $"\"-\"{CommonConstants.CsvFileDelimiter}"
                    : $"\"-\"";

            var decimalValue = value.GetValueOrDefault();

            string formattedValue;

            if (isCurrency)
            {
                formattedValue = FormatUtils.FormatCurrency(decimalValue, ((int?)roundTo) ?? 2);
            }
            else if (isPercentage)
            {
                formattedValue = FormatUtils.FormatPercentage(decimalValue, ((int?)roundTo) ?? 8);
            }
            else
            {
                var roundedValue = roundTo == null
                    ? decimalValue
                    : MathUtils.RoundAwayFromZero(decimalValue, (int)roundTo);

                formattedValue = valueFormat == null
                    ? roundedValue.ToString(CultureInfo.InvariantCulture)
                    : roundedValue.ToString(valueFormat.ToString(), CultureInfo.InvariantCulture);
            }

            return delimiterRequired
                ? $"\"{formattedValue}\"{CommonConstants.CsvFileDelimiter}"
                : $"\"{formattedValue}\"";
        }
    }
}
