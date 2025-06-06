using EPR.Calculator.API.Utils;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.Model;
using EPR.Calculator.Service.Function.Mappers;
using EPR.Calculator.Service.Function.Models;
using Microsoft.Azure.Amqp.Encoding;
using Microsoft.Azure.Amqp.Framing;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Text;

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