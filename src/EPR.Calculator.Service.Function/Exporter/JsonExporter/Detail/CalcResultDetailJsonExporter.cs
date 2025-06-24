using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.Detail
{
    public class CalcResultDetailJsonExporter : ICalcResultDetailJsonExporter
    {
        public string Export(CalcResultDetail calcResultDetail)
        {
            var settings = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                Converters = { new Converter.CurrencyConverter() },

                // This is required in order to output the £ symbol as-is rather than encoding it.
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            };

            return JsonSerializer.Serialize(CalcResultDetailJsonMapper.Map(calcResultDetail), settings);
        }
    }
}