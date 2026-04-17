using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EPR.Calculator.Service.Function.Converters
{
    public class DecimalPrecisionConverter(int precision) : JsonConverter<decimal>
    {
        public override decimal Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
            => Convert.ToDecimal(reader.GetString());

        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        {
            if (value == 0)
            {
                writer.WriteNumberValue(0);
            }
            else
            {
                writer.WriteRawValue(value.ToString(
                    $"N{precision}",
                    new NumberFormatInfo
                    {
                        // Don't use a comma in the number, or JSON will interpret it as the end of the line.
                        NumberGroupSeparator = string.Empty,
                    }));
            }
        }
    }
}