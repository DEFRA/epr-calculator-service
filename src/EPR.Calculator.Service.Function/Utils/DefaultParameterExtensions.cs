using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace EPR.Calculator.Service.Function.Utils;

[ExcludeFromCodeCoverage]
public static class DefaultParameterExtensions
{
    public static decimal ToDecimal(this string value)
    {
        if (decimal.TryParse(value, out var result))
        {
            return result;
        }

        throw new FormatException($"'{value}' is not a valid decimal.");
    }

    public static DateTime? ToOptionalDate(this string value)
    {
        if (value.Equals("NA", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (DateTime.TryParseExact(
                value,
                "dd/MM/yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var result))
        {
            return result;
        }

        throw new FormatException($"'{value}' is not a valid optional date.");
    }
}
