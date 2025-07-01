using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Models;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Builder.CancelledProducers
{
    public interface ICalcResultCancelledProducersBuilder
    {
        Task<CalcResultCancelledProducersResponse> Construct(CalcResultsRequestDto resultsRequestDto);
    }
}