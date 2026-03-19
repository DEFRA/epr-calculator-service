using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.ParametersOther
{
    public interface ICalcResultParameterOtherCostBuilder
    {
        Task<CalcResultParameterOtherCost> ConstructAsync(CalcResultsRequestDto resultsRequestDto);
    }
}
