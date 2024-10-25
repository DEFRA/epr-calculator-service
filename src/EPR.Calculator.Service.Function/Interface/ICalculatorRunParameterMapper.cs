using EPR.Calculator.Service.Common;

namespace EPR.Calculator.Service.Function.Interface
{
    public interface ICalculatorRunParameterMapper
    {
        CalculatorRunParameter Map(CalculatorParameter calculatorParameter);
    }
}
