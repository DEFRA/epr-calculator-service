namespace EPR.Calculator.Service.Function.Mapper
{
    using EPR.Calculator.Service.Common;
    using EPR.Calculator.Service.Function.Interface;

    public class CalculatorRunParameterMapper : ICalculatorRunParameterMapper
    {
        public CalculatorRunParameter Map(CalculatorParameter calculatorParameter)
        {
            return new CalculatorRunParameter()
            {
                FinancialYear = calculatorParameter.FinancialYear,
                User = calculatorParameter.CreatedBy,
                Id = calculatorParameter.CalculatorRunId,
            };
        }
    }
}
