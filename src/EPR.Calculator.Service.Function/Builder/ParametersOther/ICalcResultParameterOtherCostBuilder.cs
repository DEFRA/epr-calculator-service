using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.ParametersOther
{
    public interface ICalcResultParameterOtherCostBuilder
    {
        Task<CalcResultParameterOtherCost> ConstructAsync(CalcResultsRequestDto resultsRequestDto);
    }
}
