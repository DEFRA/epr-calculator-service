using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder
{
    public interface ICalcResultBuilder
    {
        Task<CalcResult> BuildAsync(CalcResultsRequestDto resultsRequestDto);
    }
}
