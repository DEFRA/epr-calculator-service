using EPR.Calculator.Service.Function.Mappers;
using EPR.Calculator.Service.Function.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.Detail
{
    public class CalcResultDetailExporter : ICalcResultDetailExporter
    {
        public string Export(CalcResultDetail calcResultDetail)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            };

            return JsonConvert.SerializeObject(CalcResultDetailJsonMapper.Map(calcResultDetail), settings);
        }
    }
}