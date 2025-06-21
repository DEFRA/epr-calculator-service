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
    public class CommsCostByMaterial2A : ICommsCostByMaterial2A
    {
        private ICalcResult2aCommsDataByMaterialMapper mapper;

        public CommsCostByMaterial2A(ICalcResult2aCommsDataByMaterialMapper mapper)
        {
            this.mapper = mapper;
        }

        public string Export(List<CalcResultCommsCostCommsCostByMaterial> commsCostByMaterial)
        {
            var result = this.mapper.Map(commsCostByMaterial);
            return JsonConvert.SerializeObject(result);
        }
    }
}
