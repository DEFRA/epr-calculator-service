using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder
{
    public interface ICalcResultBuilder
    {
        Task<CalcResult> BuildAsync(CalcResultsRequestDto resultsRequestDto);
    }
}
