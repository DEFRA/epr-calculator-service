using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.CommsCostByMaterial2A
{
    public class CommsCostByMaterial2AExporter : ICommsCostByMaterial2AExporter
    {
        private readonly ICalcResult2ACommsDataByMaterialMapper mapper;

        public CommsCostByMaterial2AExporter(ICalcResult2ACommsDataByMaterialMapper mapper)
        {
            this.mapper = mapper;
        }

        public string Export(IEnumerable<CalcResultCommsCostCommsCostByMaterial> commsCostByMaterial)
        {
            var result = this.mapper.Map(commsCostByMaterial);
            return JsonConvert.SerializeObject(result);
        }
    }
}
