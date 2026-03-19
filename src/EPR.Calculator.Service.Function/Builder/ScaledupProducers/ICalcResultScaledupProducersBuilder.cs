using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.ScaledupProducers
{
    public interface ICalcResultScaledupProducersBuilder
    {
        Task<CalcResultScaledupProducers> ConstructAsync(CalcResultsRequestDto resultsRequestDto);
    }
}
