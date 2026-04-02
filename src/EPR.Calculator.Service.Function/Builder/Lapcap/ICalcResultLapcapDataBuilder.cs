using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Lapcap
{
    public interface ICalcResultLapcapDataBuilder
    {
        Task<CalcResultLapcapData> ConstructAsync(CalcResultsRequestDto resultsRequestDto);
    }
}