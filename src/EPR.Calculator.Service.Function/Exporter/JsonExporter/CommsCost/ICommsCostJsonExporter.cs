using System.Collections.Generic;
using System.Text;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter
{
    public interface ICommsCostJsonExporter
    {
        public CalcResultCommsCostJson Export(CalcResultCommsCost communicationCost);     
    }
}
