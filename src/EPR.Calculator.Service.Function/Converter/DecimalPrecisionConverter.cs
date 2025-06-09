using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Converter
{
    public class DecimalPrecisionConverter : JsonConverter
    {
        private readonly int _precision;

        public DecimalPrecisionConverter(int precision)
        {
            _precision = precision;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var rounded = Math.Round((decimal)value, _precision);
            writer.WriteValue(rounded);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return Convert.ToDecimal(reader.Value);
        }

        public override bool CanConvert(Type objectType) => objectType == typeof(decimal);
    }
}
