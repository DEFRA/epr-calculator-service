using EPR.Calculator.Service.Common;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class CalculatorRunParameterMapper 
    {
        public static CalculatorRunParameter Map(CalculatorParameter calculatorParameter)
        {
            return new CalculatorRunParameter()
            {
                FinancialYear = calculatorParameter.FinancialYear,
                User = calculatorParameter.CreatedBy,
                Id = calculatorParameter.CalculatorRunId
            };
        }
    }
}
