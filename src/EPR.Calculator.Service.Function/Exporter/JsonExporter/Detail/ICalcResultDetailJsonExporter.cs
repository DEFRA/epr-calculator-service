using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using System.Text;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.Detail
{
    public interface ICalcResultDetailJsonExporter
    {
        CalcResultDetailJson Export(CalcResultDetail calcResultDetail);
    }
}