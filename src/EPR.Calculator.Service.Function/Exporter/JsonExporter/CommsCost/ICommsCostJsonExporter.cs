using System.Collections.Generic;
using System.Text;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter
{
    public interface ICommsCostJsonExporter
    {
        public string Export(CalcResultCommsCost communicationCost);     
    }
}
