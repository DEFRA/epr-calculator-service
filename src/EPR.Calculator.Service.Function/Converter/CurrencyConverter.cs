using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Converter
{
    /// <summary>
    /// Converts a decimal value to and from a currency string representation for use in JSON serialisation.
    /// </summary>
    internal class CurrencyConverter : JsonConverter<decimal>
    {
        /// <inheritdoc/>
        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException("Expected a string representing a currency value.");
            }
            string? currencyString = reader.GetString();
            if (currencyString is not null
                && decimal.TryParse(currencyString.Substring(1), out decimal result)) // Remove currency symbol and try parsing
            {
                return result;
            }
            throw new JsonException($"Could not parse currency value: {currencyString}");
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("C", CultureInfo.GetCultureInfo("en-GB")));
        }
    }
}
