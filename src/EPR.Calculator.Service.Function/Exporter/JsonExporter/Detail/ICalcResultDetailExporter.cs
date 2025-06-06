using EPR.Calculator.Service.Function.Models;
using System.Text;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.Detail
{
    public interface ICalcResultDetailExporter
    {
        string Export(CalcResultDetail calcResultDetail);
    }
}