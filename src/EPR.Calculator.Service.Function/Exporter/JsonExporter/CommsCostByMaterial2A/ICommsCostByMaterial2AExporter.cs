using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.CommsCostByMaterial2A
{
    public interface ICommsCostByMaterial2AExporter
    {
        public CalcResult2ACommsDataByMaterial Export(IEnumerable<CalcResultCommsCostCommsCostByMaterial> commsCostByMaterial);
    }
}
