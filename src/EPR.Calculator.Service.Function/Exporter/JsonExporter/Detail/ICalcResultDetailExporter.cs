using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.Detail
{
    public interface ICalcResultDetailExporter
    {
        string Export(CalcResultDetail calcResultDetail);
    }
}