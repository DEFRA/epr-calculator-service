using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Mapper
{
    public interface IParametersOtherMapper
    {
        CalcResultParametersOtherJson Map(CalcResultParameterOtherCost calcResultsSummary);
    }
}