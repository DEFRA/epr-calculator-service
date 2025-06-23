using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter
{
    public class CommsCostJsonExporter : ICommsCostJsonExporter
    {
        private readonly ICommsCostMapper mapper;

        public CommsCostJsonExporter(ICommsCostMapper mapper)
        {
            this.mapper = mapper;
        }

        public string Export(CalcResultCommsCost communicationCost)
        {
            var result = this.mapper.Map(communicationCost);
            return JsonConvert.SerializeObject(result);
        }       
    }
}
