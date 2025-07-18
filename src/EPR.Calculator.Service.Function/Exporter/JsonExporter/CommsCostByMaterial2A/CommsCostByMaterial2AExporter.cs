using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using System.Collections.Generic;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.CommsCostByMaterial2A
{
    public class CommsCostByMaterial2AExporter : ICommsCostByMaterial2AExporter
    {
        private readonly ICalcResult2ACommsDataByMaterialMapper mapper;

        public CommsCostByMaterial2AExporter(ICalcResult2ACommsDataByMaterialMapper mapper)
        {
            this.mapper = mapper;
        }

        public CalcResult2ACommsDataByMaterial Export(IEnumerable<CalcResultCommsCostCommsCostByMaterial> commsCostByMaterial)
        {
            return this.mapper.Map(commsCostByMaterial);
        }
    }
}
