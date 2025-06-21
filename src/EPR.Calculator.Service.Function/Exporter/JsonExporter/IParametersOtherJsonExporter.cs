using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.ParametersOther
{
    public interface IParametersOtherJsonExporter
    {
        string Export(CalcResultParametersOther calcResultParametersOther);
    }
}