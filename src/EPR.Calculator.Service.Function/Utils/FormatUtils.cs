using System.Globalization;

namespace EPR.Calculator.Service.Function.Utils;

public static class FormatUtils
{
    public static string FormatCurrency(decimal decimalValue, int precision = 2, string currencyGroupSeparator = "")
    {
        var culture = CultureInfo.CreateSpecificCulture("en-GB");
        culture.NumberFormat.CurrencySymbol = "£";
        culture.NumberFormat.CurrencyPositivePattern = 0;
        culture.NumberFormat.CurrencyGroupSeparator = currencyGroupSeparator;

        return MathUtils
                .RoundAwayFromZero(decimalValue, precision)
                .ToString($"C{precision}", culture);
    }

    public static string FormatPercentage(decimal value, int precision = 8) =>
        MathUtils
            .RoundAwayFromZero(value, precision)
             .ToString($"F{precision}") + "%";
}
