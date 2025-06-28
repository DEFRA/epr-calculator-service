using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter
{
    public interface IParametersOtherJsonExporter
    {
        CalcResultParametersOtherJson Export(CalcResultParameterOtherCost calcResultParametersOther);
    }
}