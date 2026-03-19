using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Lapcap
{
    public interface ICalcResultLapcapDataBuilder
    {
        Task<CalcResultLapcapData> ConstructAsync(CalcResultsRequestDto resultsRequestDto);
    }
}