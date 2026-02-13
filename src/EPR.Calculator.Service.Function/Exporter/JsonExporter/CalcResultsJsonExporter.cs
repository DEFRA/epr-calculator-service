using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using EPR.Calculator.API.Exporter;
using EPR.Calculator.Service.Function.Converter;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using EPR.Calculator.Service.Function.Services;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter
{
    public class CalcResultsJsonExporter : ICalcBillingJsonExporter<CalcResult>
    {
        private const int decimalPrecision = 3;

        private readonly IMaterialService materialService; 

        public CalcResultsJsonExporter(IMaterialService materialService)
        {
            this.materialService = materialService;
        }

        public string Export(CalcResult results, IEnumerable<int> acceptedProducerIds)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results), "The results parameter cannot be null.");
            }

            var materials = this.materialService.GetMaterials().Result;

            var billingFileContent = BillingFileJson.From(results, acceptedProducerIds, materials);
           
            return JsonSerializer.Serialize(
                billingFileContent,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    Converters = { new DecimalPrecisionConverter(decimalPrecision), },
                });
        }
    }
}
