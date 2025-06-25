using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.Detail
{
    public class CalcResultDetailJsonExporter : ICalcResultDetailJsonExporter
    {
        public CalcResultDetailJson Export(CalcResultDetail calcResultDetail)
        {
            //var settings = new JsonSerializerOptions
            //{
            //    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            //    WriteIndented = true,
            //    Converters = { new Converter.CurrencyConverter() },

            //    // This is required in order to output the £ symbol as-is rather than encoding it.
            //    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            //};

            return CalcResultDetailJsonMapper.Map(calcResultDetail);
        }
    }
}