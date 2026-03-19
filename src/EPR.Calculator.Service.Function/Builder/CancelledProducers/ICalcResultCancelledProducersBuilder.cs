using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.CancelledProducers
{
    public interface ICalcResultCancelledProducersBuilder
    {
        Task<CalcResultCancelledProducersResponse> ConstructAsync(CalcResultsRequestDto resultsRequestDto);
    }
}